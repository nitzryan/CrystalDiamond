using Db;
using ShellProgressBar;
using SiteDb;

namespace SitePrep
{
    internal class PitcherPage
    {
        private static bool WritePlayerJson()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

            siteDb.PitcherYearStats.RemoveRange(siteDb.PitcherYearStats);
            siteDb.PitcherMonthStats.RemoveRange(siteDb.PitcherMonthStats);
            siteDb.SaveChanges();
            siteDb.ChangeTracker.Clear();

            var players = db.Model_Players.Where(f => f.IsPitcher == 1)
                .Join(db.Site_PlayerBio, mp => mp.MlbId, sbi => sbi.Id, (mp, sbi) => new { mp, sbi }); ;
            using (ProgressBar progressBar = new ProgressBar(players.Count(), "Generating Pitcher Site Data"))
            {
                foreach (var playerTuple in players)
                {
                    var player = playerTuple.mp;
                    var bio = playerTuple.sbi;

                    // Model Output
                    var opws = db.Output_PlayerWarAggregation.Where(f => f.MlbId == player.MlbId && f.ModelName.Equals("P")).OrderBy(f => f.Year).ThenBy(f => f.Month);
                    foreach (var opw in opws)
                    {
                        var ranks = siteDb.PlayerRank.Where(f => f.Year == opw.Year && f.Month == opw.Month && f.MlbId == opw.MlbId && f.ModelName.Equals(opw.ModelName));
                        if (!ranks.Any() && opw.Month == 0)
                            ranks = siteDb.PlayerRank.Where(f => f.MlbId == opw.MlbId && f.ModelName.Equals(opw.ModelName))
                                .OrderBy(f => f.Year).ThenBy(f => f.Month);

                        siteDb.Add(new PlayerModel
                        {
                            MlbId = player.MlbId,
                            Year = opw.Year,
                            Month = opw.Month,
                            ModelName = opw.ModelName,
                            Probs = $"{opw.Prob0.ToString("0.000")}," +
                                    $"{opw.Prob1.ToString("0.000")}," +
                                    $"{opw.Prob2.ToString("0.000")}," +
                                    $"{opw.Prob3.ToString("0.000")}," +
                                    $"{opw.Prob4.ToString("0.000")}," +
                                    $"{opw.Prob5.ToString("0.000")}," +
                                    $"{opw.Prob6.ToString("0.000")}",
                            Rank = ranks.Any() ? ranks.First().Rank : null
                        });
                    }
                    // Demographic Data
                    // Check to make sure player isn't already in db (Two-Way Player)
                    Db.Player p = db.Player.Where(f => f.MlbId == player.MlbId).Single();
                    var prevPlayer = siteDb.Player.Where(f => f.MlbId == player.MlbId);
                    if (prevPlayer.Any())
                    {
                        prevPlayer.First().IsPitcher = 1;
                        goto annualStats;
                    }

                    // Get most recent org
                    var poms = db.Player_OrgMap.Where(f => f.MlbId == player.MlbId).OrderByDescending(f => f.Year).ThenByDescending(f => f.Month).ThenByDescending(f => f.Day);
                    siteDb.Add(new SiteDb.Player
                    {
                        MlbId = p.MlbId,
                        FirstName = p.UseFirstName,
                        LastName = p.UseLastName,
                        BirthYear = p.BirthYear,
                        BirthMonth = p.BirthMonth,
                        BirthDate = p.BirthDate,
                        StartYear = p.SigningYear.Value,
                        Position = bio.Position,
                        Status = bio.Status,
                        OrgId = poms.Any() ? poms.First().ParentOrgId : 0,
                        DraftPick = bio.DraftPick > 0 ? bio.DraftPick : null,
                        DraftRound = bio.DraftPick > 0 ? bio.DraftRound : null,
                        DraftBonus = bio.DraftPick > 0 ? bio.DraftBonus : null,
                        IsPitcher = 1,
                        IsHitter = 0
                    });

                    // Annual Stats
                    annualStats:
                    var annualStats = db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == player.MlbId).OrderBy(f => f.Year).ThenByDescending(f => f.LevelId).ThenBy(f => f.TeamId);
                    foreach (var stats in annualStats)
                    {
                        float hrRate = stats.Outs > 0 ? (float)(stats.HR) / stats.Outs * 27 : stats.HR * 27;

                        siteDb.PitcherYearStats.Add(new PitcherYearStats
                        {
                            MlbId = p.MlbId,
                            LevelId = stats.LevelId,
                            Year = stats.Year,
                            TeamId = stats.TeamId,
                            LeagueId = stats.LeagueId,
                            IP = $"{stats.Outs / 3}.{stats.Outs % 3}",
                            ERA = (float)Math.Round(stats.ERA, 2),
                            FIP = (float)Math.Round(stats.FIP, 2),
                            HR9 = (float)Math.Round(hrRate, 1),
                            BBPerc = (float)Math.Round(stats.BBPerc * 100, 1),
                            KPerc = (float)Math.Round(stats.KPerc * 100, 1),
                            GOPerc = (float)Math.Round(stats.GBRatio * 100, 1),
                        });
                    }

                    // Month Stats
                    var monthStats = db.Player_Pitcher_MonthAdvanced.Where(f => f.MlbId == player.MlbId)
                        .OrderBy(f => f.Year).ThenBy(f => f.Month).ThenByDescending(f => f.LevelId).ThenBy(f => f.TeamId);
                    foreach (var stats in monthStats)
                    {
                        float hrRate = stats.Outs > 0 ? (float)(stats.HR) / stats.Outs * 27 : stats.HR * 27;

                        siteDb.PitcherMonthStats.Add(new PitcherMonthStats
                        {
                            MlbId = p.MlbId,
                            LevelId = stats.LevelId,
                            Year = stats.Year,
                            Month = stats.Month,
                            TeamId = stats.TeamId,
                            LeagueId = stats.LeagueId,
                            IP = $"{stats.Outs / 3}.{stats.Outs % 3}",
                            ERA = (float)Math.Round(stats.ERA, 2),
                            FIP = (float)Math.Round(stats.FIP, 2),
                            HR9 = (float)Math.Round(hrRate, 1),
                            BBPerc = (float)Math.Round(stats.BBPerc * 100, 1),
                            KPerc = (float)Math.Round(stats.KPerc * 100, 1),
                            GOPerc = (float)Math.Round(stats.GBRatio * 100, 1),
                        });
                    }

                    progressBar.Tick();
                }
            }
            siteDb.SaveChanges();

            return true;
        }

        public static bool Main()
        {
            try
            {
                return WritePlayerJson();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in PitcherPage");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
