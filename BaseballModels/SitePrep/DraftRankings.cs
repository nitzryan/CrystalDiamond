using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ModelDb;
using ShellProgressBar;
using SiteDb;

namespace SitePrep
{
    internal class DraftRankings
    {
        private record DraftValue
        {
            public int tbcId;
            public float value;
            public required string position;
            public bool isHitter;
            public bool isEligible;
        }

        // All players with 3 years are eligible, 1 are not, 2 are eligible if 21 within 45 days of draft
        private static bool IsPlayerEligible(College_Player player, int year, int expYears, List<Draft_Results> draftResults)
        {
            // If player was drafted, they had to be eligible
            if (draftResults.Where(f => f.Year == year && f.MlbId == player.MlbId).Any())
                return true;
        
            if (expYears >= 3)
                return true;
            if (expYears <= 1)
                return false;

            // Get cutoff date
            int cutoffMonth = -1;
            int cutoffDay = -1;

            switch (year)
            {
                case 2004:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2005:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2006:
                    cutoffMonth = 7;
                    cutoffDay = 22;
                    break;
                case 2007:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2008:
                    cutoffMonth = 7;
                    cutoffDay = 21;
                    break;
                case 2009:
                    cutoffMonth = 7;
                    cutoffDay = 26;
                    break;
                case 2010:
                    cutoffMonth = 7;
                    cutoffDay = 24;
                    break;
                case 2011:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2012:
                    cutoffMonth = 7;
                    cutoffDay = 21;
                    break;
                case 2013:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2014:
                    cutoffMonth = 7;
                    cutoffDay = 22;
                    break;
                case 2015:
                    cutoffMonth = 7;
                    cutoffDay = 25;
                    break;
                case 2016:
                    cutoffMonth = 7;
                    cutoffDay = 26;
                    break;
                case 2017:
                    cutoffMonth = 7;
                    cutoffDay = 29;
                    break;
                case 2018:
                    cutoffMonth = 7;
                    cutoffDay = 21;
                    break;
                case 2019:
                    cutoffMonth = 7;
                    cutoffDay = 20;
                    break;
                case 2020:
                    cutoffMonth = 7;
                    cutoffDay = 26;
                    break;
                case 2021:
                    cutoffMonth = 8;
                    cutoffDay = 27;
                    break;
                case 2022:
                    cutoffMonth = 9;
                    cutoffDay = 1;
                    break;
                default: // Fixed August 1 rule applies onward until MLB changes it
                    cutoffMonth = 8;
                    cutoffDay = 1;
                    break;
            }

            float age = Utilities.GetAge1MinusAge0(year, cutoffMonth, cutoffDay, player.BirthYear, player.BirthMonth, player.BirthDay);
            return age >= 21;
        }

        private static DraftRank GetCollegeDraftRankFromPrevious(College_Player colPlayer, Model_Players mp, SiteDbContext siteDb, int year, int modelId, float warPost, int draftTeamId)
        {
            var lastDraftRank = siteDb.DraftRank
                .Where(f => f.MlbId == colPlayer.MlbId
                            && f.IsHitter == mp.IsHitter)
                .OrderByDescending(f => f.Year)
                .FirstOrDefault();

            // Player only has hitter/pitcher college stats and was drafted as opposite
            if (lastDraftRank == null)
                lastDraftRank = siteDb.DraftRank
                    .Where(f => f.MlbId == colPlayer.MlbId)
                    .OrderByDescending(f => f.Year)
                    .First();

            return new DraftRank
            {
                TbcId = colPlayer.TBCId,
                MlbId = colPlayer.MlbId,
                ModelId = modelId,
                Name = colPlayer.FirstName + " " + colPlayer.LastName,
                Position = lastDraftRank.Position,
                BirthYear = colPlayer.BirthYear,
                BirthMonth = colPlayer.BirthMonth,
                BirthDate = colPlayer.BirthDay,
                IsHitter = mp.IsHitter,
                Year = year,
                IsEligible = true,
                RankEligible = -1,
                WarPre = lastDraftRank.WarPre,
                WarPost = warPost,
                DraftPick = mp.DraftPick,
                DraftTeamid = draftTeamId,
                TrainingBias = lastDraftRank.TrainingBias,
                TimestepQuality = lastDraftRank.TimestepQuality
            };
        }

        public static void Update()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            siteDb.DraftRank.ExecuteDelete();

            var modelYears = modelDb.Output_College_HitterAggregation.Where(f => f.Year >= Constants.PUBLIC_DATA_START_YEAR)
                .Select(f => new { f.Year, f.ModelId })
                .Distinct()
                .Union(modelDb.ModelId.Select(f => new {Year=2020, ModelId = f.Id}))
                .OrderBy(f => f.ModelId).ThenBy(f => f.Year);

            // tbcId -> N, reset whenever the model changes since N is sequential within one model's timeline
            Dictionary<(int tbcId, bool isHitter), int> tbcTimestepCounts = new();
            int currentTrackedModel = -1;

            // Get the teams for all players that were actually drafted
            Dictionary<int, (int TeamId, bool IsHitter, bool IsPitcher)> draftTeamDict = db.Draft_Results
                .AsNoTracking()
                .Where(f => f.Signed == 1
                            && f.Year >= Constants.PUBLIC_DATA_START_YEAR
                )
                .ToDictionary(f => f.MlbId, f => (f.TeamId, false, false));
            foreach (var mlbId in draftTeamDict.Keys)
            {
                var playerModelArray = siteDb.PlayerModel
                    .Where(f => f.MlbId == mlbId)
                    .ToArray();
                bool isHitter = playerModelArray.Any(f => f.IsHitter);
                bool isPitcher = playerModelArray.Any(f => !f.IsHitter);

                var dtd = draftTeamDict[mlbId];
                draftTeamDict[mlbId] = (dtd.TeamId, isHitter, isPitcher);
            }

            // Store draft results to make sure all players who were drafted are set as eligible
            List<Draft_Results> draftResults = db.Draft_Results
                .AsNoTracking()
                .ToList();

            // Keep track of who is in training set.
            HashSet<(int TbcId, bool IsHitter)> draftTrainSet = new();

            using (ProgressBar progressBar = new ProgressBar(modelYears.Count(), "Generating Draft Rankings"))
            {
                foreach (var modelYear in modelYears)
                {
                    // Reset when model changes
                    if (modelYear.ModelId != currentTrackedModel)
                    {
                        tbcTimestepCounts.Clear();
                        currentTrackedModel = modelYear.ModelId;

                        draftTrainSet = modelDb.PlayersInTrainingData
                        .Where(f => f.ModelId == modelYear.ModelId && f.IsTrain)
                        .Select(f => new { f.TbcId, f.IsHitter })
                        .Distinct()
                        .AsEnumerable()
                        .Select(f => (f.TbcId, f.IsHitter))
                        .ToHashSet();
                    }

                    // Get all players ranked
                    var hitterStats = modelDb.Output_College_HitterAggregation
                        .Where(f => f.Year == modelYear.Year && f.ModelId == modelYear.ModelId);
                    var pitcherStats = modelDb.Output_College_PitcherAggregation
                        .Where(f => f.Year == modelYear.Year && f.ModelId == modelYear.ModelId);

                    // Get DraftValue for each hitter, pitcher in list
                    List<DraftValue> playerValues = new();
                    playerValues.Capacity = hitterStats.Count() + pitcherStats.Count();
                    foreach (var hs in hitterStats)
                    {
                        College_Player cp = db.College_Player.Where(f => f.TBCId == hs.TbcId).Single();
                        Model_College_HitterYear hitStats = db.Model_College_HitterYear.Where(f => f.TBCId == hs.TbcId && f.Year == hs.Year).Single();

                        playerValues.Add(new DraftValue
                        {
                            tbcId = hs.TbcId,
                            value = hs.War,
                            isHitter = true,
                            position = Db.DbEnums.GetFlagsDescription(hitStats.Pos),
                            isEligible = IsPlayerEligible(cp, hs.Year, hitStats.ExpYears, draftResults),
                        });
                    }
                    foreach (var ps in pitcherStats)
                    {
                        College_Player cp = db.College_Player.Where(f => f.TBCId == ps.TbcId).Single();
                        Model_College_PitcherYear pitStats = db.Model_College_PitcherYear.Where(f => f.TBCId == ps.TbcId && f.Year == ps.Year).Single();

                        playerValues.Add(new DraftValue
                        {
                            tbcId = ps.TbcId,
                            value = ps.War,
                            isHitter = false,
                            position = "P",
                            isEligible = IsPlayerEligible(cp, ps.Year, pitStats.ExpYears, draftResults),
                        });
                    }

                    // Order draft-eligible ones
                    List<DraftRank> draftRanks = new();
                    draftRanks.Capacity = playerValues.Count;
                    var draftRankings = playerValues
                        .Where(f => f.isEligible)
                        .OrderByDescending(f => f.value);

                    int rank = 1;
                    foreach (var dr in draftRankings)
                    {
                        var colPlayer = db.College_Player.Where(f => f.TBCId == dr.tbcId).Single();
                        int? draftPick = null;
                        int? draftTeamId = null;
                        float? warPost = null;

                        // Check if player was drafted
                        if (colPlayer.LastYear == modelYear.Year && (colPlayer.DraftOvrHitter + colPlayer.DraftOvrPitcher) > 0)
                        {
                            draftPick = Math.Max(colPlayer.DraftOvrPitcher, colPlayer.DraftOvrHitter);

                            if (draftTeamDict.TryGetValue(colPlayer.MlbId, out var signData))
                            {
                                // Don't assign draft/pro data to a college player who didn't hit/pitch in the pros
                                if ((colPlayer.IsHitter && !signData.IsHitter) || (colPlayer.IsPitcher && !signData.IsPitcher))
                                    continue;

                                draftTeamId = signData.TeamId;
                            }       

                            try
                            {
                                warPost = modelDb.Output_PlayerWarAggregation
                                .Where(f => f.MlbId == colPlayer.MlbId && f.ModelId == modelYear.ModelId && f.Year == 0)
                                .Max(f => f.War);
                            }
                            catch (Exception) { /* Drafted and not signed, so no data */ }
                        }

                        int n = tbcTimestepCounts.GetValueOrDefault((dr.tbcId, dr.isHitter), 0);
                        draftRanks.Add(new DraftRank
                        {
                            TbcId = dr.tbcId,
                            MlbId = colPlayer.MlbId,
                            ModelId = modelYear.ModelId,
                            Name = colPlayer.FirstName + " " + colPlayer.LastName,
                            Position = dr.position,
                            BirthYear = colPlayer.BirthYear,
                            BirthMonth = colPlayer.BirthMonth,
                            BirthDate = colPlayer.BirthDay,
                            IsHitter = dr.isHitter,
                            Year = modelYear.Year,
                            IsEligible = true,
                            RankEligible = rank,
                            WarPre = dr.value,
                            WarPost = warPost,
                            DraftPick = draftPick,
                            DraftTeamid = draftTeamId,
                            TrainingBias = draftTrainSet.Contains((dr.tbcId, dr.isHitter)),
                            TimestepQuality = dr.isHitter
                        ? Utilities.GetDraftHitterTimestepQuality(n)
                        : Utilities.GetDraftPitcherTimestepQuality(n),
                        });

                        rank++;
                        tbcTimestepCounts[(dr.tbcId, dr.isHitter)] = n + 1;
                    }

                    // Non-eligible rankings included for player history
                    var ineligiblePlayers = playerValues.Where(f => !f.isEligible);
                    foreach (var ip in ineligiblePlayers)
                    {
                        var colPlayer = db.College_Player.Where(f => f.TBCId == ip.tbcId).Single();
                        int n = tbcTimestepCounts.GetValueOrDefault((ip.tbcId, ip.isHitter), 0);

                        draftRanks.Add(new DraftRank
                        {
                            TbcId = ip.tbcId,
                            MlbId = colPlayer.MlbId,
                            ModelId = modelYear.ModelId,
                            Name = colPlayer.FirstName + " " + colPlayer.LastName,
                            Position = ip.position,
                            IsHitter = ip.isHitter,
                            Year = modelYear.Year,
                            BirthYear = colPlayer.BirthYear,
                            BirthMonth = colPlayer.BirthMonth,
                            BirthDate = colPlayer.BirthDay,
                            IsEligible = false,
                            RankEligible = -1,
                            WarPre = ip.value,
                            WarPost = null, // Ineligible, so not possible to be drafted
                            DraftPick = null,
                            TrainingBias = draftTrainSet.Contains((ip.tbcId, ip.isHitter)),
                            TimestepQuality = ip.isHitter
                                ? Utilities.GetDraftHitterTimestepQuality(n)
                                : Utilities.GetDraftPitcherTimestepQuality(n),
                        });

                        tbcTimestepCounts[(ip.tbcId, ip.isHitter)] = n + 1;
                    }

                    siteDb.BulkInsert(draftRanks);

                    // Add in all drafted players that don't have a rating (mostly HS players)
                    var draftedPlayers = db.Player
                        .Where(f => f.SigningYear == modelYear.Year
                                && f.DraftPick != null)
                        .Join(db.Model_Players,
                                p => p.MlbId,
                                mp => mp.MlbId,
                                (p, mp) => new { p, mp })
                        .AsNoTracking()
                        .ToList();
                    List<DraftRank> hsDraftedPlayers = new(draftedPlayers.Count());
                    foreach (var dp in draftedPlayers)
                    {
                        var existingDraftRanks = siteDb.DraftRank
                            .Where(f => f.MlbId == dp.p.MlbId && f.ModelId == modelYear.ModelId)
                            .ToList();
                        if (existingDraftRanks.Any(f => f.DraftPick != null))
                        {
                            continue;
                        }
                        float warPost = modelDb.Output_PlayerWarAggregation
                                .Where(f => f.MlbId == dp.p.MlbId
                                    && f.ModelId == modelYear.ModelId
                                    && f.Year == 0)
                                .Max(f => f.War);

                        // Player had previous college data but none for draft year
                        if (existingDraftRanks.Count > 0)
                        {
                            hsDraftedPlayers.Add(GetCollegeDraftRankFromPrevious(
                                db.College_Player.Where(f => f.MlbId == dp.p.MlbId).Single(),
                                dp.mp,
                                siteDb,
                                modelYear.Year,
                                modelYear.ModelId,
                                warPost,
                                draftTeamDict[dp.p.MlbId].TeamId // IsHitter/IsPitcher irrelevant, will only generate for pro position
                            ));

                            // If a player was missing draft info on college data, need to remove or it will blow up
                            if (existingDraftRanks.Any(f => f.Year == modelYear.Year 
                                                        && f.IsHitter == dp.mp.IsHitter))
                            {
                                var tmp = siteDb.DraftRank
                                    .Where(f => f.Year == modelYear.Year
                                            && f.ModelId == modelYear.ModelId
                                            && f.MlbId == dp.p.MlbId
                                            && f.IsHitter == dp.mp.IsHitter);

                                siteDb.RemoveRange(tmp);
                            }
                        }
                        else
                            hsDraftedPlayers.Add(new DraftRank
                            {
                                TbcId = -1,
                                MlbId = dp.p.MlbId,
                                ModelId = modelYear.ModelId,
                                Name = dp.p.UseFirstName + " " + dp.p.UseLastName,
                                Position = dp.p.Position,
                                IsHitter = dp.mp.IsHitter,
                                Year = modelYear.Year,
                                BirthYear = dp.p.BirthYear,
                                BirthMonth = dp.p.BirthMonth,
                                BirthDate = dp.p.BirthDate,
                                IsEligible = false,
                                RankEligible = -1,
                                WarPre = null,
                                WarPost = warPost,
                                DraftPick = dp.mp.DraftPick,
                                DraftTeamid = draftTeamDict[dp.p.MlbId].TeamId, // IsHitter/IsPitcher irrelevant, will only generate for pro position
                                TrainingBias = false,
                                TimestepQuality = SiteDb.DbEnums.TimestepQuality.HSVeryLow
                            });
                    }

                    siteDb.AddRange(hsDraftedPlayers);
                    siteDb.SaveChanges();

                    progressBar.Tick();
                }
            }
        }
    }
}
