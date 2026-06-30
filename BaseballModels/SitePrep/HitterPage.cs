using Db;
using ShellProgressBar;
using SiteDb;
using ModelDb;
using Microsoft.EntityFrameworkCore;

namespace SitePrep
{
    internal class HitterPage
    {
        private static void WritePlayerJson()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            siteDb.PlayerModel.ExecuteDelete();
            siteDb.Player.ExecuteDelete();
            siteDb.HitterYearStats.ExecuteDelete();
            siteDb.HitterMonthStats.ExecuteDelete();
            siteDb.SaveChanges();

            // Pre-load which players are in trainingData
            HashSet<(int MlbId, int ModelIdx)> hitterTrainSet = modelDb.PlayersInTrainingData
                .Where(f => f.IsHitter && f.IsTrain)
                .Select(f => new { f.MlbId, f.ModelIdx })
                .Distinct()
                .AsEnumerable()
                .Select(f => (f.MlbId, f.ModelIdx))
                .ToHashSet();

            var players = db.Model_Players.Where(f => f.IsHitter == 1)
                .Join(db.Site_PlayerBio, mp => mp.MlbId, sbi => sbi.Id, (mp, sbi) => new { mp, sbi });
            using (ProgressBar progressBar = new ProgressBar(players.Count(), "Generating Hitter Site Data"))
            {
                foreach (var playerTuple in players)
                {
                    var player = playerTuple.mp;
                    var bio = playerTuple.sbi;

                    // Model Output Buckets by model
                    var opwsByModel = modelDb.Output_PlayerWarAggregation
                        .Where(f => f.MlbId == player.MlbId && f.IsHitter)
                        .GroupBy(f => f.Model);

                    foreach (var modelGroup in opwsByModel)
                    {
                        int modelId = modelGroup.Key;
                        bool trainingBias = hitterTrainSet.Contains((player.MlbId, modelId));
                        int timestepIndex = 0; // N, 0-indexed count of sequential prospect rankings seen so far
                        foreach (var opw in modelGroup.OrderBy(f => f.Year).ThenBy(f => f.Month))
                        {
                            var ranks = siteDb.PlayerRank.Where(f => f.Year == opw.Year && f.Month == opw.Month && f.MlbId == opw.MlbId && f.ModelId == opw.Model);

                            siteDb.Add(new PlayerModel
                            {
                                MlbId = player.MlbId,
                                Year = opw.Year,
                                Month = opw.Month,
                                ModelId = opw.Model,
                                IsHitter = opw.IsHitter,
                                ProbsWar = $"{opw.War0.ToString("0.000")}," +
                                        $"{opw.War1.ToString("0.000")}," +
                                        $"{opw.War2.ToString("0.000")}," +
                                        $"{opw.War3.ToString("0.000")}," +
                                        $"{opw.War4.ToString("0.000")}," +
                                        $"{opw.War5.ToString("0.000")}," +
                                        $"{opw.War6.ToString("0.000")}",
                                RankWar = ranks.Any() ? ranks.First().RankWar : null,
                                TrainingBias = trainingBias,
                                TimestepQuality = Utilities.GetProHitterTimestepQuality(timestepIndex),
                            });

                            timestepIndex++;
                        }
                    }

                    // Demographic Data
                    Db.Player p = db.Player.Where(f => f.MlbId == player.MlbId).Single();

                    // Get most recent org
                    var poms = db.Player_OrgMap.Where(f => f.MlbId == player.MlbId).OrderByDescending(f => f.Year).ThenByDescending(f => f.Month).ThenByDescending(f => f.Day);

                    #pragma warning disable CS8629 // SigningValue will be not null at this point, otherwise won't have model data
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
                        IsPitcher = false,
                        IsHitter = true,
                        InTraining = modelDb.PlayersInTrainingData.Where(f => f.MlbId == p.MlbId).Any(),
                    });
                    #pragma warning restore CS8629

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
        }

        public static void Update()
        {
            try 
            {
                WritePlayerJson();
            } 
            catch (Exception e)
            {
                Console.WriteLine("Error in HitterPage");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
