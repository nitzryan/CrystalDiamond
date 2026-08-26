using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using SiteDb;
using static Db.DbEnums;

namespace SitePrep
{
    internal class GenerateTeamRank
    {
        public static void Update()
        {
            try
            {
                using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                siteDb.TeamRank.RemoveRange(siteDb.TeamRank);
                siteDb.SaveChanges();
                siteDb.ChangeTracker.Clear();

                // Pre-load data from Base DB
                // Drafted players and their pick
                Dictionary<int, int> draftPicks = db.Player
                    .Where(f => f.DraftPick != null)
                    .Select(f => new { f.MlbId, Pick = f.DraftPick!.Value })
                    .ToDictionary(f => f.MlbId, f => f.Pick);

                // Expected WAR by pick
                Dictionary<int, (float Hitter, float Pitcher)> pickValues = db.DraftPickValues
                    .Select(f => new { f.Pick, f.WarHitter, f.WarPitcher })
                    .ToList()
                    .ToDictionary(f => f.Pick, f => (f.WarHitter, f.WarPitcher));

                // Track how player arrived to each organization each time they arrived
                Dictionary<int, List<Arrival>> arrivals = db.Transaction_Log
                   .Where(f => f.ParentOrgId != 0
                            && (f.PrevParentOrgId == null || f.PrevParentOrgId != f.ParentOrgId))
                   .Select(f => new { f.MlbId, f.Year, f.Month, f.Day, f.ParentOrgId, f.TransactionType })
                   .ToList()
                   .GroupBy(f => f.MlbId)
                   .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => new Arrival(f.Year, f.Month, f.Day, f.ParentOrgId, f.TransactionType))
                             .OrderBy(f => f.Year).ThenBy(f => f.Month).ThenBy(f => f.Day)
                        .ToList()
                );

                // Caches Aquisition types, because they will be the same across different models
                Dictionary<(int, int, int, int), AcqType> acqCache = new();

                var combos = siteDb.PlayerRank.Select(f => new { f.Year, f.Month, f.ModelId }).Distinct();
                using (ProgressBar progressBar = new ProgressBar(combos.Count(), "Creating TeamRanks"))
                {
                    foreach (var combo in combos)
                    {
                        int year = combo.Year;
                        int month = combo.Month;
                        int model = combo.ModelId;

                        // Get ranks, group with how aquired and draft capital
                        var playerRanks = siteDb.PlayerRank.AsNoTracking()
                            .Where(f => f.Year == year && f.Month == month && f.TeamId != 0 && f.ModelId == model)
                            .Select(f => new { f.MlbId, f.TeamId, f.IsHitter, f.War, f.RankWar })
                            .ToList()
                            .Select(f => new
                            {
                                f.TeamId,
                                f.IsHitter,
                                f.War,
                                f.RankWar,
                                Acq = GetAcqType(f.MlbId, f.TeamId, year, month, arrivals, draftPicks, acqCache),
                                DraftCapital = GetDraftCapital(f.MlbId, f.IsHitter, draftPicks, pickValues)
                            })
                            .GroupBy(f => f.TeamId);

                        var teamRanksWar = playerRanks
                            .Select(g => new TeamRank
                            {
                                TeamId = g.Key,
                                ModelId = model,
                                Year = year,
                                Month = month,
                                HighestRank = g.Min(f => f.RankWar),
                                Top10 = g.Count(f => f.RankWar <= 10),
                                Top50 = g.Count(f => f.RankWar <= 50),
                                Top100 = g.Count(f => f.RankWar <= 100),
                                Top200 = g.Count(f => f.RankWar <= 200),
                                Top500 = g.Count(f => f.RankWar <= 500),
                                Rank = 0,
                                War = g.Sum(f => f.War),
                                WarHitter = g.Where(f => f.IsHitter).Sum(f => f.War),
                                WarPitcher = g.Where(f => !f.IsHitter).Sum(f => f.War),
                                WarDraftHitter = g.Where(f => f.IsHitter && f.Acq == AcqType.Draft).Sum(f => f.War),
                                WarDraftPitcher = g.Where(f => !f.IsHitter && f.Acq == AcqType.Draft).Sum(f => f.War),
                                DraftCapitalHitter = g.Where(f => f.IsHitter && f.Acq == AcqType.Draft).Sum(f => f.DraftCapital),
                                DraftCapitalPitcher = g.Where(f => !f.IsHitter && f.Acq == AcqType.Draft).Sum(f => f.DraftCapital),
                                WarTradeHitter = g.Where(f => f.IsHitter && f.Acq == AcqType.Trade).Sum(f => f.War),
                                WarTradePitcher = g.Where(f => !f.IsHitter && f.Acq == AcqType.Trade).Sum(f => f.War),
                                WarSignHitter = g.Where(f => f.IsHitter && f.Acq == AcqType.Sign).Sum(f => f.War),
                                WarSignPitcher = g.Where(f => !f.IsHitter && f.Acq == AcqType.Sign).Sum(f => f.War)
                            })
                            .OrderByDescending(g => g.War)
                            .ToList();

                        ScaleDraftCapital(teamRanksWar);

                        int rank = 1;
                        foreach (var tr in teamRanksWar)
                        {
                            int r = rank;
                            tr.Rank = r;
                            siteDb.TeamRank.Add(tr);
                            rank++;
                        }

                        progressBar.Tick();
                    }
                }
                siteDb.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GenerateTeamRank");
                Utilities.LogException(e);
                throw;
            }
        }

        private enum AcqType { Draft, Trade, Sign }

        // Data for when/how a player arrived to an org
        private sealed record Arrival(int Year, int Month, int Day, int OrgId, TransactionType Type);

        // Set of transaction types that are considered as traded for
        private static readonly HashSet<TransactionType> TRADE_TYPES = [TransactionType.Trade];

        // Get how a player was aquired for a given team on a given date
        private static AcqType GetAcqType(int mlbId, int teamId, int year, int month,
            Dictionary<int, List<Arrival>> arrivals,
            Dictionary<int, int> draftPicks,
            Dictionary<(int, int, int, int), AcqType> cache)
        {
            if (cache.TryGetValue((mlbId, teamId, year, month), out AcqType cached))
                return cached;

            AcqType result = draftPicks.ContainsKey(mlbId) ? AcqType.Draft : AcqType.Sign;
            if (arrivals.TryGetValue(mlbId, out var events))
            {
                // Most recent arrival at this org on or before the end of the snapshot month
                Arrival? latest = null;
                foreach (var e in events) // ascending by date
                {
                    if (e.Year > year || (e.Year == year && e.Month > month))
                        break;
                    if (e.OrgId == teamId)
                        latest = e;
                }
                if (latest != null && TRADE_TYPES.Contains(latest.Type))
                    result = AcqType.Trade;
            }
            cache[(mlbId, teamId, year, month)] = result;
            return result;
        }

        // Get the expected WAR for a player's draft slot
        private static float GetDraftCapital(int mlbId, bool isHitter,
            Dictionary<int, int> draftPicks,
            Dictionary<int, (float Hitter, float Pitcher)> pickValues)
        {
            if (!draftPicks.TryGetValue(mlbId, out int pick))
                return 0;
            if (!pickValues.TryGetValue(pick, out var v))
                return 0;
            return isHitter ? v.Hitter : v.Pitcher;
        }

        // Scales draftCapital so that the draftCapital is on the same scale
        // as WAR from drafted players, avoiding some biases in analysis
        private static void ScaleDraftCapital(List<TeamRank> teamRanks)
        {
            // Get league WAR for drafted players, expected value from draft capital
            float warHitter = teamRanks.Sum(f => f.WarDraftHitter);
            float capHitter = teamRanks.Sum(f => f.DraftCapitalHitter);
            float warPitcher = teamRanks.Sum(f => f.WarDraftPitcher);
            float capPitcher = teamRanks.Sum(f => f.DraftCapitalPitcher);

            // Scale each so that the draftWar is equal to draftCapital
            float hitterFactor = capHitter != 0 ? warHitter / capHitter : 1;
            float pitcherFactor = capPitcher != 0 ? warPitcher / capPitcher : 1;
            
            foreach (var tr in teamRanks)
            {
                tr.DraftCapitalHitter *= hitterFactor;
                tr.DraftCapitalPitcher *= pitcherFactor;
            }
        }
    }
}
