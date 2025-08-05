using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class ModelPlayers
    {
        public static bool Main(int year, int month)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Model_Players.RemoveRange(db.Model_Players);
                db.SaveChanges();

                var players = db.Player_CareerStatus
                    .Where(f => f.CareerStartYear >= Constants.START_YEAR
                        && f.IgnorePlayer == null);
                using (ProgressBar progressBar = new ProgressBar(players.Count(), $"Creating Model_Players for Month={month} Year={year}"))
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
                            lastMlbSeason = year;

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
                        else
                        {
                            lastProspectYear = year;
                            lastProspectMonth = month;
                        }

                        // Age at Signing
                        var player = db.Player.Where(f => f.MlbId == pcs.MlbId).First();
                        if (player.SigningYear == null || player.SigningMonth == null)
                            throw new Exception($"Null Signing Year/Month for {player.MlbId}");

                        float ageAtSigning = Utilities.GetAge1MinusAge0(player.SigningYear.Value, player.SigningMonth.Value, 1,
                                                                        player.BirthYear, player.BirthMonth, player.BirthDate);

                        db.Model_Players.Add(new Model_Players
                        {
                            MlbId = pcs.MlbId,
                            IsHitter = pcs.IsHitter,
                            IsPitcher = pcs.IsPitcher,
                            LastProspectYear = lastProspectYear,
                            LastProspectMonth = lastProspectMonth,
                            LastMLBSeason = lastMlbSeason,
                            AgeAtSigningYear = ageAtSigning
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
