using Db;
using ShellProgressBar;
using System.Net.NetworkInformation;

namespace DataAquisition
{
    internal class ModelMonthStats
    {
        public static bool Main(int endYear, int endMonth)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Model_HitterStats.RemoveRange(db.Model_HitterStats);
                db.Model_PitcherStats.RemoveRange(db.Model_PitcherStats);
                db.SaveChanges();

                var hitters = db.Model_Players.Where(f => f.IsHitter == 1);
                using (ProgressBar progressBar = new ProgressBar(hitters.Count(), $"Generating Model Hitter Month Stats"))
                {
                    foreach (var hitter in hitters)
                    {
                        var player = db.Player.Where(f => f.MlbId == hitter.MlbId).First();
                        var ratios = db.Player_Hitter_MonthlyRatios.Where(f => f.MlbId == player.MlbId)
                                                                    .OrderBy(f => f.Year).ThenBy(f => f.Month);

                        int prevYear = 0;
                        int prevMonth = 0;
                        float prevLevel = -1;
                        Model_HitterStats currentData = new()
                        {
                            MlbId = player.MlbId,
                            Age = -1,
                            InjStatus = -1,
                            PA = -1,
                            MonthFrac = -1,
                            ParkHRFactor = -1,
                            ParkRunFactor = -1,
                            Year = -1,
                            Month = -1,
                            LevelId = -1,
                            AVGRatio = -1,
                            OBPRatio = -1,
                            ISORatio = -1,
                            WOBARatio = -1,
                            SBPercRatio = -1,
                            SBRateRatio = -1,
                            HRPercRatio = -1,
                            BBPercRatio = -1,
                            KPercRatio = -1,
                            PercC = -1,
                            Perc1B = -1,
                            Perc2B = -1,
                            Perc3B = -1,
                            PercSS = -1,
                            PercLF = -1,
                            PercCF = -1,
                            PercRF = -1,
                            PercDH = -1
                        };

                        // Generate Hitter Stats
                        foreach (var r in ratios)
                        {
                            int level = r.LevelId == 1 ? 1 : r.LevelId - 9;
                            var stat = db.Player_Hitter_MonthStats.Where(f => f.MlbId == r.MlbId && f.Year == r.Year && f.Month == r.Month && f.LevelId == r.LevelId).Single();
                            if (r.Year == prevYear && r.Month == prevMonth)
                            {
                                int PA = stat.AB + stat.BB + stat.HBP;
                                float prop = (currentData.PA + PA) > 0 ? (float)(PA) / (currentData.PA + PA) : 0.5f;
                                float invProp = 1 - prop;
                                
                                currentData.PA += PA;
                                currentData.MonthFrac = (Utilities.GetGamesFrac(prevMonth, Utilities.ModelLevelToMlbLevel(level), prevYear, db) * prop) + (currentData.MonthFrac * invProp);
                                currentData.ParkHRFactor = (stat.ParkHRFactor * prop) + (currentData.ParkHRFactor * invProp);
                                currentData.ParkRunFactor = (stat.ParkRunFactor * prop) + (currentData.ParkRunFactor * invProp);
                                currentData.LevelId = (level * prop) + (currentData.LevelId * invProp);
                                currentData.AVGRatio = (r.AVGRatio * prop) + (currentData.AVGRatio * invProp);
                                currentData.OBPRatio = (r.OBPRatio * prop) + (currentData.OBPRatio * invProp);
                                currentData.ISORatio = (r.ISORatio * prop) + (currentData.ISORatio * invProp);
                                currentData.WOBARatio = (r.WOBARatio * prop) + (currentData.WOBARatio * invProp);
                                currentData.SBPercRatio = (r.SBPercRatio * prop) + (currentData.SBPercRatio * invProp);
                                currentData.SBRateRatio = (r.SBRateRatio * prop) + (currentData.SBRateRatio * invProp);
                                currentData.HRPercRatio = (r.HRPercRatio * prop) + (currentData.HRPercRatio * invProp);
                                currentData.BBPercRatio = (r.BBPercRatio * prop) + (currentData.BBPercRatio * invProp);
                                currentData.KPercRatio = (r.KPercRatio * prop) + (currentData.KPercRatio * invProp);
                                currentData.PercC = (r.PercC * prop) + (currentData.PercC * invProp);
                                currentData.Perc1B = (r.Perc1B * prop) + (currentData.Perc1B * invProp);
                                currentData.Perc2B = (r.Perc2B * prop) + (currentData.Perc2B * invProp);
                                currentData.Perc3B = (r.Perc3B * prop) + (currentData.Perc3B * invProp);
                                currentData.PercSS = (r.PercSS * prop) + (currentData.PercSS * invProp);
                                currentData.PercLF = (r.PercLF * prop) + (currentData.PercLF * invProp);
                                currentData.PercCF = (r.PercCF * prop) + (currentData.PercCF * invProp);
                                currentData.PercRF = (r.PercRF * prop) + (currentData.PercRF * invProp);
                                currentData.PercDH = (r.PercDH * prop) + (currentData.PercDH * invProp);
                            }
                            else // New Month
                            {
                                // Add Previous
                                if (currentData.Age > 0)
                                {
                                    db.Model_HitterStats.Add(currentData.Clone());
                                    prevMonth++;
                                    if (prevMonth > 9)
                                    {
                                        prevMonth = 4;
                                        prevYear++;
                                    }

                                    int prevLevelInt = Convert.ToInt32(Math.Floor(prevLevel));
                                    while (prevYear < r.Year || (prevYear == r.Year && prevMonth < r.Month))
                                    {
                                        // Fill Hitter Gaps
                                        //Console.WriteLine($"{prevMonth}, {level}, {prevYear}");
                                        
                                        if (Utilities.GamesAtLevel(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear, db))
                                            db.Model_HitterStats.Add(new Model_HitterStats
                                            {
                                                MlbId = r.MlbId,
                                                Year = prevYear,
                                                Month = prevMonth,
                                                Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                                PA = 0,
                                                InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, r.MlbId, db),
                                                MonthFrac = Utilities.GetGamesFrac(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear, db),
                                                LevelId = prevLevelInt,
                                                ParkHRFactor = 1,
                                                ParkRunFactor = 1,
                                                AVGRatio = 1,
                                                OBPRatio = 1,
                                                ISORatio = 1,
                                                WOBARatio = 1,
                                                SBPercRatio = 1,
                                                SBRateRatio = 1,
                                                HRPercRatio = 1,
                                                BBPercRatio = 1,
                                                KPercRatio = 1,
                                                PercC = 0,
                                                Perc1B = 0,
                                                Perc2B = 0,
                                                Perc3B = 0,
                                                PercSS = 0,
                                                PercLF = 0,
                                                PercCF = 0,
                                                PercRF = 0,
                                                PercDH = 0
                                            });

                                        prevMonth++;
                                        if (prevMonth > 9)
                                        {
                                            prevMonth = 4;
                                            prevYear++;
                                        }
                                    }
                                }
                                    
 
                                currentData.Age = Utilities.GetAge1MinusAge0(r.Year, r.Month, 15, player.BirthYear, player.BirthMonth, player.BirthDate);
                                currentData.PA = stat.AB + stat.BB + stat.HBP;
                                currentData.InjStatus = Utilities.GetInjStatus(r.Month, r.Year, r.MlbId, db);
                                currentData.ParkHRFactor = stat.ParkHRFactor;
                                currentData.ParkRunFactor = stat.ParkRunFactor;
                                currentData.Year = r.Year;
                                currentData.Month = r.Month;
                                currentData.LevelId = level;
                                currentData.MonthFrac = Utilities.GetGamesFrac(r.Month, Utilities.ModelLevelToMlbLevel(level), r.Year, db);
                                currentData.AVGRatio = r.AVGRatio;
                                currentData.OBPRatio = r.OBPRatio;
                                currentData.ISORatio = r.ISORatio;
                                currentData.WOBARatio = r.WOBARatio;
                                currentData.SBPercRatio = r.SBPercRatio;
                                currentData.SBRateRatio = r.SBRateRatio;
                                currentData.HRPercRatio = r.HRPercRatio;
                                currentData.BBPercRatio = r.BBPercRatio;
                                currentData.KPercRatio = r.KPercRatio;
                                currentData.PercC = r.PercC;
                                currentData.Perc1B = r.Perc1B;
                                currentData.Perc2B = r.Perc2B;
                                currentData.Perc3B = r.Perc3B;
                                currentData.PercSS = r.PercSS;
                                currentData.PercLF = r.PercLF;
                                currentData.PercCF = r.PercCF;
                                currentData.PercRF = r.PercRF;
                                currentData.PercDH = r.PercDH;

                                prevMonth = r.Month;
                                prevYear = r.Year;
                                prevLevel = level;
                            }
                        }

                        // Make sure that trailing gaps are included
                        if (ratios.Any())
                        {
                            int lastYear = ratios.Last().Year;
                            int pLevelInt = Convert.ToInt32(Math.Floor(prevLevel));
                            while (prevYear <= Math.Min(lastYear + 2, Constants.CURRENT_YEAR))
                            {
                                // Fill Hitter Gaps
                                //Console.WriteLine($"{prevMonth}, {level}, {prevYear}");

                                if (Utilities.GamesAtLevel(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear, db))
                                    db.Model_HitterStats.Add(new Model_HitterStats
                                    {
                                        MlbId = hitter.MlbId,
                                        Year = prevYear,
                                        Month = prevMonth,
                                        Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                        PA = 0,
                                        InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, hitter.MlbId, db),
                                        MonthFrac = Utilities.GetGamesFrac(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear, db),
                                        LevelId = pLevelInt,
                                        ParkHRFactor = 1,
                                        ParkRunFactor = 1,
                                        AVGRatio = 1,
                                        OBPRatio = 1,
                                        ISORatio = 1,
                                        WOBARatio = 1,
                                        SBPercRatio = 1,
                                        SBRateRatio = 1,
                                        HRPercRatio = 1,
                                        BBPercRatio = 1,
                                        KPercRatio = 1,
                                        PercC = 0,
                                        Perc1B = 0,
                                        Perc2B = 0,
                                        Perc3B = 0,
                                        PercSS = 0,
                                        PercLF = 0,
                                        PercCF = 0,
                                        PercRF = 0,
                                        PercDH = 0
                                    });

                                prevMonth++;
                                if (prevMonth > 9)
                                {
                                    prevMonth = 4;
                                    prevYear++;
                                }
                            }
                        } 
                        else // Player just signed, put in empty values for year(s) after signing
                        {
                            Player p = db.Player.Where(f => f.MlbId == hitter.MlbId).Single();
                            int signingYear = p.SigningYear.Value;

                            int y = signingYear + 1;
                            int m = 4;
                            const int MISSING_LEVEL = 7;
                            while ((y < endYear || (y == endYear && m <= endMonth)) && (y < (signingYear + 7)))
                            {
                                if (Utilities.GamesAtLevel(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y, db))
                                    db.Model_HitterStats.Add(new Model_HitterStats
                                    {
                                        MlbId = hitter.MlbId,
                                        Year = y,
                                        Month = m,
                                        Age = Utilities.GetAge1MinusAge0(y, m, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                        PA = 0,
                                        InjStatus = Utilities.GetInjStatus(m, y, hitter.MlbId, db),
                                        MonthFrac = Utilities.GetGamesFrac(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y, db),
                                        LevelId = MISSING_LEVEL,
                                        ParkHRFactor = 1,
                                        ParkRunFactor = 1,
                                        AVGRatio = 1,
                                        OBPRatio = 1,
                                        ISORatio = 1,
                                        WOBARatio = 1,
                                        SBPercRatio = 1,
                                        SBRateRatio = 1,
                                        HRPercRatio = 1,
                                        BBPercRatio = 1,
                                        KPercRatio = 1,
                                        PercC = 0,
                                        Perc1B = 0,
                                        Perc2B = 0,
                                        Perc3B = 0,
                                        PercSS = 0,
                                        PercLF = 0,
                                        PercCF = 0,
                                        PercRF = 0,
                                        PercDH = 0
                                    });

                                m++;
                                if (m > 9)
                                {
                                    m = 4;
                                    y++;
                                }
                            }
                        }


                        progressBar.Tick();
                    }
                    db.SaveChanges();
                }

                var pitchers = db.Model_Players.Where(f => f.IsPitcher == 1);
                using (ProgressBar progressBar = new ProgressBar(pitchers.Count(), $"Generating Model Pitcher Month Stats"))
                {
                    foreach (var pitcher in pitchers)
                    {
                        var player = db.Player.Where(f => f.MlbId == pitcher.MlbId).First();
                        var ratios = db.Player_Pitcher_MonthlyRatios.Where(f => f.MlbId == player.MlbId)
                                                                    .OrderBy(f => f.Year).ThenBy(f => f.Month);

                        int prevYear = 0;
                        int prevMonth = 0;
                        float prevLevel = -1;
                        Model_PitcherStats currentData = new()
                        {
                            MlbId = player.MlbId,
                            Age = -1,
                            BF = -1,
                            InjStatus = -1,
                            MonthFrac = -1,
                            ParkHRFactor = -1,
                            ParkRunFactor = -1,
                            Year = -1,
                            Month = -1,
                            LevelId = -1,
                            WOBARatio = -1,
                            HRPercRatio = -1,
                            BBPercRatio = -1,
                            KPercRatio = -1,
                            GBPercRatio = -1,
                            ERARatio = -1,
                            FIPRatio = -1
                        };

                        // Generate Hitter Stats
                        foreach (var r in ratios)
                        {
                            int level = r.LevelId == 1 ? 1 : r.LevelId - 9;
                            var stat = db.Player_Pitcher_MonthStats.Where(f => f.MlbId == r.MlbId && f.Year == r.Year && f.Month == r.Month && f.LevelId == r.LevelId).Single();
                            if (r.Year == prevYear && r.Month == prevMonth)
                            { 
                                
                                float prop = (currentData.BF + stat.BattersFaced) > 0 ? (float)(stat.BattersFaced) / (currentData.BF + stat.BattersFaced) : 0.5f;
                                float invProp = 1 - prop;

                                currentData.BF += stat.BattersFaced;
                                currentData.MonthFrac = (Utilities.GetGamesFrac(prevMonth, Utilities.ModelLevelToMlbLevel(level), prevYear, db) * prop) + (currentData.MonthFrac * invProp);
                                currentData.ParkHRFactor = (stat.ParkHRFactor * prop) + (currentData.ParkHRFactor * invProp);
                                currentData.ParkRunFactor = (stat.ParkRunFactor * prop) + (currentData.ParkRunFactor * invProp);
                                currentData.LevelId = (level * prop) + (currentData.LevelId * invProp);
                                currentData.WOBARatio = (r.WOBARatio * prop) + (currentData.WOBARatio * invProp);
                                currentData.HRPercRatio = (r.HRPercRatio * prop) + (currentData.HRPercRatio * invProp);
                                currentData.BBPercRatio = (r.BBPercRatio * prop) + (currentData.BBPercRatio * invProp);
                                currentData.KPercRatio = (r.KPercRatio * prop) + (currentData.KPercRatio * invProp);
                                currentData.GBPercRatio = (r.GBPercRatio * prop) + (currentData.GBPercRatio * invProp);
                                currentData.ERARatio = (r.ERARatio * prop) + (currentData.ERARatio * invProp);
                                currentData.FIPRatio = (r.FIPRatio * prop) + (currentData.FIPRatio * invProp);
                            }
                            else // New Month
                            {
                                // Add Previous
                                if (currentData.Age > 0)
                                {
                                    db.Model_PitcherStats.Add(currentData.Clone());
                                    prevMonth++;
                                    if (prevMonth > 9)
                                    {
                                        prevMonth = 4;
                                        prevYear++;
                                    }

                                    int prevLevelInt = Convert.ToInt32(Math.Floor(prevLevel));
                                    while (prevYear < r.Year || (prevYear == r.Year && prevMonth < r.Month))
                                    {
                                        // Fill Hitter Gaps
                                        //Console.WriteLine($"{prevMonth}, {level}, {prevYear}");

                                        if (Utilities.GamesAtLevel(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear, db))
                                            db.Model_PitcherStats.Add(new Model_PitcherStats
                                            {
                                                MlbId = r.MlbId,
                                                Year = prevYear,
                                                Month = prevMonth,
                                                Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                                BF = 0,
                                                InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, r.MlbId, db),
                                                MonthFrac = Utilities.GetGamesFrac(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear, db),
                                                LevelId = prevLevelInt,
                                                ParkHRFactor = 1,
                                                ParkRunFactor = 1,
                                                WOBARatio = 1,
                                                HRPercRatio = 1,
                                                BBPercRatio = 1,
                                                KPercRatio = 1,
                                                GBPercRatio = 1,
                                                FIPRatio = 1,
                                                ERARatio = 1
                                            });

                                        prevMonth++;
                                        if (prevMonth > 9)
                                        {
                                            prevMonth = 4;
                                            prevYear++;
                                        }
                                    }
                                }


                                currentData.Age = Utilities.GetAge1MinusAge0(r.Year, r.Month, 15, player.BirthYear, player.BirthMonth, player.BirthDate);
                                currentData.BF = stat.BattersFaced;
                                currentData.InjStatus = Utilities.GetInjStatus(r.Month, r.Year, r.MlbId, db);
                                currentData.ParkHRFactor = stat.ParkHRFactor;
                                currentData.ParkRunFactor = stat.ParkRunFactor;
                                currentData.Year = r.Year;
                                currentData.Month = r.Month;
                                currentData.LevelId = level;
                                currentData.MonthFrac = Utilities.GetGamesFrac(r.Month, Utilities.ModelLevelToMlbLevel(level), r.Year, db);
                                currentData.WOBARatio = r.WOBARatio;
                                currentData.HRPercRatio = r.HRPercRatio;
                                currentData.BBPercRatio = r.BBPercRatio;
                                currentData.KPercRatio = r.KPercRatio;
                                currentData.GBPercRatio = r.GBPercRatio;
                                currentData.ERARatio = r.ERARatio;
                                currentData.FIPRatio = r.FIPRatio;

                                prevMonth = r.Month;
                                prevYear = r.Year;
                                prevLevel = level;
                            }
                        }

                        // Make sure that trailing gaps are included
                        if (ratios.Any())
                        {
                            int lastYear = ratios.Last().Year;
                            int pLevelInt = Convert.ToInt32(Math.Floor(prevLevel));
                            while (prevYear <= Math.Min(lastYear + 2, Constants.CURRENT_YEAR))
                            {
                                // Fill Hitter Gaps
                                //Console.WriteLine($"{prevMonth}, {level}, {prevYear}");

                                if (Utilities.GamesAtLevel(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear, db))
                                    db.Model_PitcherStats.Add(new Model_PitcherStats
                                    {
                                        MlbId = pitcher.MlbId,
                                        Year = prevYear,
                                        Month = prevMonth,
                                        Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                        BF = 0,
                                        InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, pitcher.MlbId, db),
                                        MonthFrac = Utilities.GetGamesFrac(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear, db),
                                        LevelId = pLevelInt,
                                        ParkHRFactor = 1,
                                        ParkRunFactor = 1,
                                        WOBARatio = 1,
                                        HRPercRatio = 1,
                                        BBPercRatio = 1,
                                        KPercRatio = 1,
                                        GBPercRatio = 1,
                                        FIPRatio = 1,
                                        ERARatio = 1
                                    });

                                prevMonth++;
                                if (prevMonth > 9)
                                {
                                    prevMonth = 4;
                                    prevYear++;
                                }
                            }
                        }
                        else // Player just signed, put in empty values for year(s) after signing
                        {
                            Player p = db.Player.Where(f => f.MlbId == pitcher.MlbId).Single();
                            int signingYear = p.SigningYear.Value;

                            int y = signingYear + 1;
                            int m = 4;
                            const int MISSING_LEVEL = 7;
                            while ((y < endYear || (y == endYear && m <= endMonth)) && (y < (signingYear + 7)))
                            {
                                if (Utilities.GamesAtLevel(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y, db))
                                    db.Model_PitcherStats.Add(new Model_PitcherStats
                                    {
                                        MlbId = pitcher.MlbId,
                                        Year = y,
                                        Month = m,
                                        Age = Utilities.GetAge1MinusAge0(y, m, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                        BF = 0,
                                        InjStatus = Utilities.GetInjStatus(m, y, pitcher.MlbId, db),
                                        MonthFrac = Utilities.GetGamesFrac(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y, db),
                                        LevelId = MISSING_LEVEL,
                                        ParkHRFactor = 1,
                                        ParkRunFactor = 1,
                                        WOBARatio = 1,
                                        HRPercRatio = 1,
                                        BBPercRatio = 1,
                                        KPercRatio = 1,
                                        GBPercRatio = 1,
                                        FIPRatio = 1,
                                        ERARatio = 1
                                    });

                                m++;
                                if (m > 9)
                                {
                                    m = 4;
                                    y++;
                                }
                            }
                        }

                        progressBar.Tick();
                    }
                    db.SaveChanges();
                }

                return true;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ModelMonthStats");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
