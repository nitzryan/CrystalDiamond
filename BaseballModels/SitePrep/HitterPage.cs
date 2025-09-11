using Db;
using ShellProgressBar;
using SiteDb;

namespace SitePrep
{
    internal class HitterPage
    {
        private static bool WritePlayerJson()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

            siteDb.PlayerModel.RemoveRange(siteDb.PlayerModel);
            siteDb.Player.RemoveRange(siteDb.Player);
            siteDb.HitterYearStats.RemoveRange(siteDb.HitterYearStats);
            siteDb.HitterMonthStats.RemoveRange(siteDb.HitterMonthStats);
            siteDb.SaveChanges();
            siteDb.ChangeTracker.Clear();

            var players = db.Model_Players.Where(f => f.IsHitter == 1)
                .Join(db.Site_PlayerBio, mp => mp.MlbId, sbi => sbi.Id, (mp, sbi) => new { mp, sbi });
            using (ProgressBar progressBar = new ProgressBar(players.Count(), "Generating Hitter Site Data"))
            {
                foreach (var playerTuple in players)
                {
                    var player = playerTuple.mp;
                    var bio = playerTuple.sbi;

                    // Model Output
                    var opws = db.Output_PlayerWarAggregation.Where(f => f.MlbId == player.MlbId && f.ModelName.Equals("H")).OrderBy(f => f.Year).ThenBy(f => f.Month);
                    foreach (var opw in opws)
                    {
                        var ranks = siteDb.PlayerRank.Where(f => f.Year == opw.Year && f.Month == opw.Month && f.MlbId == opw.MlbId && f.ModelName.Equals(opw.ModelName));

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
                    Db.Player p = db.Player.Where(f => f.MlbId == player.MlbId).Single();

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
                        IsPitcher = 0,
                        IsHitter = 1,
                        InTraining = db.PlayersInTrainingData.Where(f => f.MlbId == p.MlbId).Any() ? 1 : 0,
                    });

                    // Annual Stats
                    var annualStats = db.Player_Hitter_YearAdvanced.Where(f => f.MlbId == player.MlbId).OrderBy(f => f.Year).ThenByDescending(f => f.LevelId).ThenBy(f => f.TeamId);
                    foreach (var stats in annualStats)
                    {
                        siteDb.HitterYearStats.Add(new HitterYearStats
                        {
                            MlbId = p.MlbId,
                            LevelId = stats.LevelId,
                            Year = stats.Year,
                            TeamId = stats.TeamId,
                            LeagueId = stats.LeagueId,
                            PA = stats.PA,
                            AVG = (float)Math.Round(stats.AVG, 3),
                            OBP = (float)Math.Round(stats.OBP, 3),
                            SLG = (float)Math.Round(stats.SLG, 3),
                            ISO = (float)Math.Round(stats.ISO, 3),
                            WRC = (int)Math.Round(stats.WRC, 0),
                            HR = stats.HR,
                            BBPerc = (float)Math.Round(stats.BBPerc * 100, 1),
                            KPerc = (float)Math.Round(stats.KPerc * 100, 1),
                            SB = stats.SB,
                            CS = stats.CS,
                        });
                    }

                    // Month Stats
                    var monthStats = db.Player_Hitter_MonthAdvanced.Where(f => f.MlbId == player.MlbId)
                        .OrderBy(f => f.Year).ThenBy(f => f.Month).ThenByDescending(f => f.LevelId).ThenBy(f => f.TeamId);
                    foreach (var stats in monthStats)
                    {
                        siteDb.HitterMonthStats.Add(new HitterMonthStats
                        {
                            MlbId = p.MlbId,
                            LevelId = stats.LevelId,
                            Year = stats.Year,
                            Month = stats.Month,
                            TeamId = stats.TeamId,
                            LeagueId = stats.LeagueId,
                            PA = stats.PA,
                            AVG = (float)Math.Round(stats.AVG, 3),
                            OBP = (float)Math.Round(stats.OBP, 3),
                            SLG = (float)Math.Round(stats.SLG, 3),
                            ISO = (float)Math.Round(stats.ISO, 3),
                            WRC = (int)Math.Round(stats.WRC, 0),
                            HR = stats.HR,
                            BBPerc = (float)Math.Round(stats.BBPerc * 100, 1),
                            KPerc = (float)Math.Round(stats.KPerc * 100, 1),
                            SB = stats.SB,
                            CS = stats.CS,
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
                Console.WriteLine("Error in HitterPage");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
