using Db;
using Microsoft.EntityFrameworkCore.Migrations;
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

                var players = db.Player_CareerStatus.Join(db.Player, pcs => pcs.MlbId, p => p.MlbId, (pcs, p) => new { pcs, p })
                    .Where(f => f.p.SigningYear >= Constants.START_YEAR && f.pcs.IgnorePlayer == null);
                using (ProgressBar progressBar = new ProgressBar(players.Count(), $"Creating Model_Players"))
                {
                    foreach (var p in players)
                    {
                        // Last MLB Season
                        int lastMlbSeason = -1;
                        if (p.pcs.AgedOut != null)
                            lastMlbSeason = p.pcs.AgedOut.Value;
                        else if (p.pcs.ServiceLapseYear != null)
                            lastMlbSeason = p.pcs.ServiceLapseYear.Value;
                        else if (p.pcs.ServiceEndYear != null)
                            lastMlbSeason = p.pcs.ServiceEndYear.Value;
                        else if (p.pcs.PlayingGap != null)
                            lastMlbSeason = p.pcs.PlayingGap.Value;
                        else
                            lastMlbSeason = 10000;

                        // Last Prospect date
                        int lastProspectYear = -1;
                        int lastProspectMonth = -1;
                        if (p.pcs.AgedOut != null)
                        {
                            lastProspectYear = p.pcs.AgedOut.Value;
                            lastProspectMonth = 13;
                        }
                        else if (p.pcs.MlbRookieYear != null && p.pcs.MlbRookieMonth != null)
                        {
                            lastProspectYear = p.pcs.MlbRookieYear.Value;
                            lastProspectMonth = p.pcs.MlbRookieMonth.Value;
                        }
                        else if (p.pcs.ServiceLapseYear != null)
                        {
                            lastProspectYear = p.pcs.ServiceLapseYear.Value;
                            lastProspectMonth = 13;
                        }
                        else if (p.pcs.PlayingGap != null)
                        {
                            lastProspectYear = p.pcs.PlayingGap.Value;
                            lastProspectMonth = 13;
                        }
                        else
                        {
                            lastProspectYear = 10000;
                            lastProspectMonth = 13;
                        }

                        // Age at Signing
                        var player = db.Player.Where(f => f.MlbId == p.pcs.MlbId).First();
                        if (player.SigningYear == null || player.SigningMonth == null)
                            throw new Exception($"Null Signing Year/Month for {player.MlbId}");

                        float ageAtSigning = Utilities.GetAge1MinusAge0(player.SigningYear.Value, player.SigningMonth.Value, 1,
                                                                        player.BirthYear, player.BirthMonth, player.BirthDate);

                        // Get WAR
                        var fWar = db.Player_YearlyWar.Where(f => f.MlbId == p.pcs.MlbId);
                        if (p.pcs.ServiceEndYear != null)
                            fWar = fWar.Where(f => f.Year <= p.pcs.ServiceEndYear);
                        if (p.pcs.ServiceLapseYear != null)
                            fWar = fWar.Where(f => f.Year <= p.pcs.ServiceLapseYear);

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

                        float hitterValue = 0;
                        float starterValue = 0;
                        float relieverValue = 0;
                        float valueStarterPerc = 0;
                        
                        if (hitterWar.Any())
                        {
                            totalHitterWar = hitterWar.Sum(f => f.WAR_h);
                            peakHitterWar = hitterWar.Max(f => f.WAR_h);
                            totalPA = hitterWar.Sum(f => f.PA);
                            int tmpPA = totalPA > 0 ? totalPA : 1; // Prevent divide by 0

                            rateBSR = hitterWar.Sum(f => f.BSR) / tmpPA;
                            rateOFF = hitterWar.Sum(f => f.OFF) / tmpPA;
                            rateDEF = hitterWar.Sum(f => f.DEF) / tmpPA;

                            hitterValue = hitterWar.Sum(f => f.WAR_h > Constants.HITTER_WAR_INFLECTION ?
                                (Constants.HITTER_WAR_INFLECTION * Constants.HITTER_WAR_LOWER_RATE) + ((f.WAR_h - Constants.HITTER_WAR_INFLECTION) * Constants.HITTER_WAR_UPPER_RATE) :
                                Constants.HITTER_WAR_LOWER_RATE * f.WAR_h);
                            
                        }
                        if (pitcherWar.Any())
                        {
                            totalPitcherWar = pitcherWar.Sum(f => f.WAR_s + f.WAR_r);
                            peakPitcherWar = pitcherWar.Max(f => f.WAR_s + f.WAR_r);
                            totalOuts = pitcherWar.Sum(f => f.PA);

                            starterValue = pitcherWar.Sum(f => f.WAR_s > Constants.STARTER_WAR_INFLECTION ?
                                (Constants.STARTER_WAR_INFLECTION * Constants.STARTER_WAR_LOWER_RATE) + ((f.WAR_s - Constants.STARTER_WAR_INFLECTION) * Constants.STARTER_WAR_UPPER_RATE) :
                                Constants.STARTER_WAR_LOWER_RATE * f.WAR_s);

                            relieverValue = pitcherWar.Sum(f => f.WAR_r * Constants.RELIEVER_WAR_RATE);

                            if (starterValue + relieverValue != 0)
                                valueStarterPerc = starterValue / (starterValue + relieverValue);
                            else if (starterValue > 0)
                                valueStarterPerc = 1;
                        }

                        // Get Highest Level
                        int highLevelHitter = p.pcs.HighestLevelHitter != null ? p.pcs.HighestLevelHitter.Value : -1;
                        int highLevelPitcher = p.pcs.HighestLevelPitcher != null ? p.pcs.HighestLevelPitcher.Value : -1;

                        db.Model_Players.Add(new Model_Players
                        {
                            MlbId = p.pcs.MlbId,
                            IsHitter = p.pcs.IsHitter,
                            IsPitcher = p.pcs.IsPitcher,
                            SigningYear = player.SigningYear.Value,
                            LastProspectYear = lastProspectYear,
                            LastProspectMonth = lastProspectMonth,
                            LastMLBSeason = lastMlbSeason,
                            AgeAtSigningYear = ageAtSigning,
                            DraftPick = player.DraftPick != null ? player.DraftPick.Value : 2000,
                            DraftSignRank = player.DraftPick != null ?
                                db.Draft_Results.Where(f => f.Year == player.SigningYear && f.Pick == player.DraftPick.Value).Single().BonusRank
                                : 2000,
                            WarHitter = totalHitterWar,
                            WarPitcher = totalPitcherWar,
                            PeakWarHitter = peakHitterWar,
                            PeakWarPitcher = peakPitcherWar,
                            ValueHitter = hitterValue,
                            ValuePitcher = starterValue + relieverValue,
                            ValueStarterPerc = valueStarterPerc,
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
