using CsvHelper;
using Db;
using System.Diagnostics;
using System.Globalization;

namespace DataAquisition
{
    internal class UpdateCareers
    {
        public static bool Main(List<int> years)
        {
            try {
                

                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Player_CareerStatus.RemoveRange(db.Player_CareerStatus);
                db.SaveChanges();

                // Insert empty Career Status
                foreach (var player in db.Player)
                {
                    Db.Player dbPlayer = db.Player.Where(f => f.MlbId == player.MlbId).Single();

                    // Determine if player is hitter/pitcher/two-way
                    int pa = db.Player_Hitter_GameLog.Where(f => f.MlbId == player.MlbId && f.Position != 1).Sum(f => f.PA);
                    int bf = db.Player_Pitcher_GameLog.Where(f => f.MlbId == player.MlbId).Sum(f => f.BattersFaced);

                    int isHitter, isPitcher;
                    if (pa == 0 && bf == 0) // No stats, use position
                    {
                        isHitter = dbPlayer.Position.Equals("H") || dbPlayer.Position.Equals("TWP")
                            ? 1 : 0;
                        isPitcher = dbPlayer.Position.Equals("P") || dbPlayer.Position.Equals("TWP")
                            ? 1 : 0;
                    } else {
                        float prop_hitter = (float)(pa) / (pa + bf);
                        isHitter = prop_hitter > 0.1 ? 1 : 0;
                        isPitcher = prop_hitter < 0.9 ? 1 : 0;
                    }

                    db.Player_CareerStatus.Add(new Player_CareerStatus
                    {
                        MlbId = player.MlbId,
                        IsPitcher = isPitcher,
                        IsHitter = isHitter,
                    });
            }
                db.SaveChanges();

                // Update IsActive
                foreach (var pcs in db.Player_CareerStatus)
                {
                    int lastYear = -1;
                    try {
                        if (pcs.IsHitter == 1)
                            lastYear = db.Player_Hitter_MonthStats.Where(f => f.MlbId == pcs.MlbId)
                                .OrderByDescending(f => f.Year).Select(f => f.Year).Single();
                        else
                            lastYear = db.Player_Pitcher_MonthStats.Where(f => f.MlbId == pcs.MlbId)
                                .OrderByDescending(f => f.Year).Select(f => f.Year).Single();

                        if (years.Last() - lastYear >= 2)
                            pcs.IsActive = 0;
                        else
                            pcs.IsActive = 1;
                    } catch (Exception) // No stats, likely just drafted
                    {
                        pcs.IsActive = 1;
                    }
                }
                db.SaveChanges();

                // Set Career Start Year/Month
                //foreach (var pcs in db.Player_CareerStatus.Where(f => f.CareerStartYear == null))
                //{
                //    int hitterStartYear = 10000;
                //    int pitcherStartYear = 10000;
                //    int hitterStartMonth = 13;
                //    int pitcherStartMonth = 13;
                    
                //    if (pcs.IsHitter == 1)
                //    {
                //        var dates = db.Player_Hitter_MonthStats.Where(f => f.MlbId == pcs.MlbId).Select(f => new { f.Year, f.Month })
                //            .OrderBy(f => f.Year).ThenBy(f => f.Month);
                //        if (dates.Any())
                //        {
                //            var firstDate = dates.First();
                //            hitterStartYear = firstDate.Year;
                //            hitterStartMonth = firstDate.Month;
                //        }  
                //    }

                //    if (pcs.IsPitcher == 1)
                //    {
                //        var dates = db.Player_Pitcher_MonthStats.Where(f => f.MlbId == pcs.MlbId).Select(f => new { f.Year, f.Month })
                //            .OrderBy(f => f.Year).ThenBy(f => f.Month);
                //        if (dates.Any())
                //        {
                //            var firstDate = dates.First();
                //            pitcherStartYear = firstDate.Year;
                //            pitcherStartMonth = firstDate.Month;
                //        }
                //    }

                //    int startYear = Math.Min(hitterStartYear, pitcherStartYear);
                //    int startMonth = Math.Min(hitterStartMonth, pitcherStartMonth);
                //    if (startYear < 10000)
                //    {
                //        pcs.CareerStartYear = startYear;
                //        pcs.CareerStartMonth = startMonth;
                //    }
                //}
                //db.SaveChanges();

                // Get MLB Start Year
                foreach (var pcs in db.Player_CareerStatus)
                {
                    int hitterStartYear = 10000;
                    int pitcherStartYear = 10000;

                    if (pcs.IsHitter == 1)
                    {
                        var dates = db.Player_Hitter_MonthStats.Where(f => f.MlbId == pcs.MlbId && f.LevelId == 1)
                            .Select(f => f.Year)
                            .OrderBy(f => f);
                        if (dates.Any())
                        {
                            hitterStartYear = dates.First();
                        }
                    }

                    if (pcs.IsPitcher == 1)
                    {
                        var dates = db.Player_Pitcher_MonthStats.Where(f => f.MlbId == pcs.MlbId && f.LevelId == 1)
                            .Select(f => f.Year)
                            .OrderBy(f => f);
                        if (dates.Any())
                        {
                            pitcherStartYear = dates.First();
                        }
                    }

                    int startYear = Math.Min(hitterStartYear, pitcherStartYear);
                    if (startYear < 10000)
                    {
                        pcs.MlbStartYear = startYear;
                    }
                }
                db.SaveChanges();

                // Get Highest Level
                foreach (var pcs in db.Player_CareerStatus)
                {
                    if (pcs.IsHitter == 1)
                    {
                        try
                        {
                            int levelId = db.Player_Hitter_MonthStats.Where(f => f.MlbId == pcs.MlbId)
                                .Select(f => f.LevelId)
                                .OrderBy(f => f).First();
                            pcs.HighestLevelHitter = Utilities.MlbLevelToModelLevel(levelId);
                        }
                        catch (Exception) { }
                    }

                    if (pcs.IsPitcher == 1)
                    {
                        try
                        {
                            int levelId = db.Player_Pitcher_MonthStats.Where(f => f.MlbId == pcs.MlbId)
                                .Select(f => f.LevelId)
                                .OrderBy(f => f).First();
                            pcs.HighestLevelPitcher = Utilities.MlbLevelToModelLevel(levelId);
                        }
                        catch (Exception) { }
                    }
                }
                db.SaveChanges();

                // Set Rookie Pitchers (>=150 Outs)
                foreach (var pcs in db.Player_CareerStatus.Where(f => f.HighestLevelPitcher == 1))
                {
                    var pitcherStats = db.Player_Pitcher_MonthStats
                        .Where(f => f.MlbId == pcs.MlbId && f.LevelId == 1)
                        .OrderBy(f => f.Year).ThenBy(f => f.Month);

                    int outs = 0;
                    foreach (var stat in pitcherStats)
                    {
                        outs += stat.Outs;
                        if (outs >= 150)
                        {
                            pcs.MlbRookieYear = stat.Year;
                            pcs.MlbRookieMonth = stat.Month;
                            break;
                        }
                    }
                }
                db.SaveChanges();

                // Set Rookie Hitters (>=130 AB)
                foreach (var pcs in db.Player_CareerStatus.Where(f => f.HighestLevelHitter == 1))
                {
                    var hitterStats = db.Player_Hitter_MonthStats
                        .Where(f => f.MlbId == pcs.MlbId && f.LevelId == 1)
                        .OrderBy(f => f.Year).ThenBy(f => f.Month);

                    // Player could reach for pitcher before hitter
                    int reachedMonth = pcs.MlbRookieMonth ?? 13;
                    int reachedYear = pcs.MlbRookieYear ?? 10000;

                    int AB = 0;
                    foreach (var stat in hitterStats)
                    {
                        if (stat.Year > reachedYear || (stat.Year == reachedYear && stat.Month <= reachedMonth))
                            break; // Reached for pitcher first

                        AB += stat.AB;
                        if (AB >= 150)
                        {
                            pcs.MlbRookieYear = stat.Year;
                            pcs.MlbRookieMonth = stat.Month;
                            break;
                        }
                    }
                }
                db.SaveChanges();

                // Check Service Time
                foreach (var pcs in db.Player_CareerStatus.Where(f => f.MlbRookieYear != null))
                {
                    var serviceYears = db.Player_ServiceTime.Where(f => f.MlbId == pcs.MlbId)
                        .OrderByDescending(f => f.Year);

                    if (!serviceYears.Any() ||
                        serviceYears.First().ServiceYear < Constants.SERVICE_TIME_CUTOFF)
                    {
                        pcs.ServiceReached = 0;
                    } else {
                        pcs.ServiceReached = 1;
                        foreach(var sy in serviceYears.OrderBy(f => f.Year))
                        {
                            if (sy.ServiceYear == 6)
                            {
                                pcs.ServiceEndYear = sy.Year;
                                break;
                            }
                        }
                    }
                }
                db.SaveChanges();

                
                // Age Out (No MLB)
                int cutoffYear = years.Last() - Constants.AGED_OUT_AGE;
                foreach (var pcs in db.Player_CareerStatus.Where(f => f.MlbStartYear == null))
                {
                    Db.Player player = db.Player.Where(f => f.MlbId == pcs.MlbId).First();
                    int birthYear = player.BirthYear;
                    int birthMonth = player.BirthMonth;
                    if (birthMonth >= 4)
                        birthYear++;

                    if (birthYear < cutoffYear)
                        pcs.AgedOut = birthYear + Constants.AGED_OUT_AGE;
                }
                db.SaveChanges();

                // Aged Out (Some MLB)
                foreach (var pcs in db.Player_CareerStatus.Where(f => f.MlbStartYear != null))
                {
                    Db.Player player = db.Player.Where(f => f.MlbId == pcs.MlbId).First();
                    int birthYear = player.BirthYear;
                    int birthMonth = player.BirthMonth;
                    if (birthMonth >= 4)
                        birthYear++;

                    if (pcs.MlbStartYear - birthYear > cutoffYear)
                        pcs.AgedOut = birthYear + Constants.AGED_OUT_AGE;
                }
                db.SaveChanges();

                // Players to ignore for training (mostly players signed to MLB contracts from Asia)
                string[] ignoreIds = File.ReadAllText(Constants.IGNORE_PLAYERS_FILE).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ignoreIds)
                {
                    int idInt = Convert.ToInt32(id);
                    try {
                        Db.Player_CareerStatus pcs = db.Player_CareerStatus.Where(f => f.MlbId == idInt).First();
                        pcs.IgnorePlayer = 1;
                    } catch (Exception) { // Player not in db, this only hit if only fraction of data is run
                        continue;
                    }
                    
                }
                db.SaveChanges();

                // Service Lapse
                foreach (var pcs in db.Player_CareerStatus.Where(f => f.AgedOut == null &&
                                                                f.ServiceReached == null &&
                                                                f.IgnorePlayer == null &&
                                                                f.MlbStartYear != null))
                {
                    // Get Years in MLB Career
                    IEnumerable<int> playerYears = pcs.IsHitter == 1 ?
                        db.Player_Hitter_MonthStats.Where(f => f.MlbId == pcs.MlbId && f.LevelId == 1).Select(f => f.Year).OrderBy(f => f).Distinct() :
                        db.Player_Pitcher_MonthStats.Where(f => f.MlbId == pcs.MlbId && f.LevelId == 1).Select(f => f.Year).OrderBy(f => f).Distinct();

                    // Sanity Check
                    if (!playerYears.Any())
                        continue;

                    // Get date that player is too old to accumulate value as prospect
                    var player = db.Player.Where(f => f.MlbId == pcs.MlbId).First();
                    int birthYear = player.BirthYear;
                    int birthMonth = player.BirthMonth;
                    if (birthMonth < 4)
                        birthYear--;

                    int stopYear = Constants.STOP_YEAR + birthYear;

                    // Go through each year to see if a lapse occurs

                    int startYear = playerYears.First();
                    int prevYear = startYear;
                    bool logged = false;
                    foreach (var py in playerYears)
                    {
                            // Time Since Debut       Gap between Years        Too Old
                        if ((py - startYear) >= 9 || (py - prevYear) > 2 || py > stopYear)
                        {
                            pcs.ServiceLapseYear = py;
                            logged = true;
                            break;
                        }

                        prevYear = py;
                    }
                    // Go to next player since lapse was already found
                    if (logged)
                        continue;

                    // Make sure last year is not within 2 years of current year
                    int lastYear = playerYears.Last();
                    if (lastYear < years.Last() - 1)
                        pcs.ServiceLapseYear = lastYear + 2;
                }
                db.SaveChanges();

                // Add Playing Gap for players who hasn't played in the last 2 years
                foreach (var pcs in db.Player_CareerStatus
                    .Join(db.Player, pcs=>pcs.MlbId, p=>p.MlbId, (pl,p) => new {pl, p})
                    .Where(f => f.p.SigningYear != null))
                {
                    IEnumerable<int> playerYears = pcs.pl.IsHitter == 1 ?
                        db.Player_Hitter_MonthStats.Where(f => f.MlbId == pcs.pl.MlbId).Select(f => f.Year):
                        db.Player_Pitcher_MonthStats.Where(f => f.MlbId == pcs.pl.MlbId).Select(f => f.Year);
                    playerYears = playerYears.Distinct().OrderDescending();

                    int lastYear = -1;
                    if (!playerYears.Any())
                        lastYear = pcs.p.SigningYear.Value + 2;
                    else
                        lastYear = playerYears.First() + 2;

                    if (lastYear < Constants.CURRENT_YEAR)
                        pcs.pl.PlayingGap = lastYear;
                }

                // Update signing date for players that weren't drafted
                //foreach (var player in db.Player.Where(f => f.SigningYear == null))
                //{
                //    var pcs = db.Player_CareerStatus.Where(f => f.MlbId == player.MlbId).First();
                //    player.SigningYear = pcs.CareerStartYear;
                //    player.SigningMonth = pcs.CareerStartMonth;
                //}
                //db.SaveChanges();

                return true;
            } catch (Exception e)
            {
                Console.WriteLine("Error in UpdateCareers");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
