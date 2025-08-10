using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class ModelPlayers
    {
        public static bool Main()
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Model_Players.RemoveRange(db.Model_Players);
                db.SaveChanges();

                var players = db.Player_CareerStatus
                    .Where(f => f.CareerStartYear >= Constants.START_YEAR
                        && f.IgnorePlayer == null);
                using (ProgressBar progressBar = new ProgressBar(players.Count(), $"Creating Model_Players"))
                {
                    foreach (var pcs in players)
                    {
                        // Last MLB Season
                        int lastMlbSeason = -1;
                        if (pcs.AgedOut != null)
                            lastMlbSeason = pcs.AgedOut.Value;
                        else if (pcs.ServiceLapseYear != null)
                            lastMlbSeason = pcs.ServiceLapseYear.Value;
                        else if (pcs.ServiceEndYear != null)
                            lastMlbSeason = pcs.ServiceEndYear.Value;
                        else
                            lastMlbSeason = 10000;

                        // Last Prospect date
                        int lastProspectYear = -1;
                        int lastProspectMonth = -1;
                        if (pcs.AgedOut != null)
                        {
                            lastProspectYear = pcs.AgedOut.Value;
                            lastProspectMonth = 13;
                        }
                        else if (pcs.MlbRookieYear != null && pcs.MlbRookieMonth != null)
                        {
                            lastProspectYear = pcs.MlbRookieYear.Value;
                            lastProspectMonth = pcs.MlbRookieMonth.Value;
                        }
                        else if (pcs.ServiceLapseYear != null)
                        {
                            lastProspectYear = pcs.ServiceLapseYear.Value;
                            lastProspectMonth = 13;
                        }
                        else if (pcs.PlayingGap != null)
                        {
                            lastProspectYear = pcs.PlayingGap.Value;
                            lastProspectMonth = 13;
                        }
                        else
                        {
                            lastProspectYear = 10000;
                            lastProspectMonth = 13;
                        }

                        // Age at Signing
                        var player = db.Player.Where(f => f.MlbId == pcs.MlbId).First();
                        if (player.SigningYear == null || player.SigningMonth == null)
                            throw new Exception($"Null Signing Year/Month for {player.MlbId}");

                        float ageAtSigning = Utilities.GetAge1MinusAge0(player.SigningYear.Value, player.SigningMonth.Value, 1,
                                                                        player.BirthYear, player.BirthMonth, player.BirthDate);

                        // Get WAR
                        var fWar = db.Player_YearlyWar.Where(f => f.MlbId == pcs.MlbId);
                        if (pcs.ServiceEndYear != null)
                            fWar = fWar.Where(f => f.Year <= pcs.ServiceEndYear);
                        if (pcs.ServiceLapseYear != null)
                            fWar = fWar.Where(f => f.Year <= pcs.ServiceLapseYear);

                        var hitterWar = fWar.Where(f => f.IsHitter == 1);
                        var pitcherWar = fWar.Where(f => f.IsHitter == 0);

                        float totalHitterWar = 0;
                        float peakHitterWar = 0;
                        float rateBSR = 0;
                        float rateOFF = 0;
                        float rateDEF = 0;
                        int totalPA = 0;

                        float totalPitcherWar = 0;
                        float peakPitcherWar = 0;
                        int totalOuts = 0;
                        
                        if (hitterWar.Any())
                        {
                            totalHitterWar = hitterWar.Sum(f => f.WAR);
                            peakHitterWar = hitterWar.Max(f => f.WAR);
                            totalPA = hitterWar.Sum(f => f.PA);
                            int tmpPA = totalPA > 0 ? totalPA : 1; // Prevent divide by 0

                            rateBSR = hitterWar.Sum(f => f.BSR) / tmpPA;
                            rateOFF = hitterWar.Sum(f => f.OFF) / tmpPA;
                            rateDEF = hitterWar.Sum(f => f.DEF) / tmpPA;
                            
                        }
                        if (pitcherWar.Any())
                        {
                            totalPitcherWar = pitcherWar.Sum(f => f.WAR);
                            peakPitcherWar = pitcherWar.Max(f => f.WAR);
                            totalOuts = pitcherWar.Sum(f => f.PA);
                        }

                        // Get Highest Level
                        int highLevelHitter = pcs.HighestLevelHitter != null ? pcs.HighestLevelHitter.Value : -1;
                        int highLevelPitcher = pcs.HighestLevelPitcher != null ? pcs.HighestLevelPitcher.Value : -1;

                        db.Model_Players.Add(new Model_Players
                        {
                            MlbId = pcs.MlbId,
                            IsHitter = pcs.IsHitter,
                            IsPitcher = pcs.IsPitcher,
                            LastProspectYear = lastProspectYear,
                            LastProspectMonth = lastProspectMonth,
                            LastMLBSeason = lastMlbSeason,
                            AgeAtSigningYear = ageAtSigning,
                            DraftPick = player.DraftPick != null ? player.DraftPick.Value : 2000,
                            WarHitter = totalHitterWar,
                            WarPitcher = totalPitcherWar,
                            PeakWarHitter = peakHitterWar,
                            PeakWarPitcher = peakPitcherWar,
                            TotalPA = totalPA,
                            TotalOuts = totalOuts,
                            RateOff = rateOFF,
                            RateDef = rateDEF,
                            RateBsr = rateBSR,
                            HighestLevelHitter = highLevelHitter,
                            HighestLevelPitcher = highLevelPitcher
                        });

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ModelPlayers");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
