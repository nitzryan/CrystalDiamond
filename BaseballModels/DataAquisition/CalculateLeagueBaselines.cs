using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateLeagueBaselines
    {
        private static bool LeagueHitterStats(SqliteDbContext db, int year, int month)
        {
            db.League_HitterStats.RemoveRange(db.League_HitterStats.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            var leagues = db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month).Select(f => f.LeagueId).Distinct().ToList();
            using (ProgressBar progressBar = new ProgressBar(leagues.Count(), $"Calculating Hitting League Baselines for {year}-{month}"))
            {
                foreach (int leagueId in leagues)
                {
                    var leagueStats = db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month && f.LeagueId == leagueId).AsEnumerable();
                    if (!leagueStats.Any()) // Skip empty
                    {
                        progressBar.Tick();
                        continue;
                    }

                    LeagueStats ls = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId).Single();

                    // Sum all stats at that level
                    Player_Hitter_MonthStats summedStats = leagueStats.Aggregate((a, b) => new Player_Hitter_MonthStats
                    {
                        MlbId = 0,
                        Month = 0,
                        Year = 0,
                        AB = a.AB + b.AB,
                        PA = a.PA + b.PA,
                        H = a.H + b.H,
                        Hit2B = a.Hit2B + b.Hit2B,
                        Hit3B = a.Hit3B + b.Hit3B,
                        HR = a.HR + b.HR,
                        K = a.K + b.K,
                        BB = a.BB + b.BB,
                        SB = a.SB + b.SB,
                        CS = a.CS + b.CS,
                        HBP = a.HBP + b.HBP,
                        LevelId = 0,
                        LeagueId = 0,
                        ParkRunFactor = 0,
                        ParkHRFactor = 0,
                        GamesC = 0,
                        Games1B = 0,
                        Games2B = 0,
                        Games3B = 0,
                        GamesSS = 0,
                        GamesLF = 0,
                        GamesCF = 0,
                        GamesRF = 0,
                        GamesDH = 0
                    });

                    // Transform to get desired stats
                    float avg = (float)summedStats.H / summedStats.AB;
                    int singles = summedStats.H - summedStats.HR - summedStats.Hit2B - summedStats.Hit3B;
                    float iso = (float)(summedStats.Hit2B + (2 * summedStats.Hit3B) + (3 * summedStats.HR)) / summedStats.AB;
                    int pa = summedStats.AB + summedStats.BB + summedStats.HBP;
                    float obp = (float)(summedStats.BB + summedStats.HBP + summedStats.H) / pa;
                    float slg = avg + iso;
                    float woba = Utilities.CalculateWOBA(ls, summedStats.HBP, summedStats.BB, singles, summedStats.Hit2B, summedStats.Hit3B, summedStats.HR, pa);

                    League_HitterStats lhs = new League_HitterStats
                    {
                        LeagueId = leagueId,
                        Year = year,
                        Month = month,
                        AB = summedStats.AB,
                        AVG = avg,
                        OBP = obp,
                        SLG = slg,
                        ISO = iso,
                        WOBA = woba,
                        HRPerc = (float)summedStats.HR / pa,
                        BBPerc = (float)summedStats.BB / pa,
                        KPerc = (float)summedStats.K / pa,
                        SBRate = (float)summedStats.SB / pa,
                        SBPerc = (float)summedStats.SB / (summedStats.SB + summedStats.CS),
                        Hit1B = Utilities.SafeDivide(singles, pa, -1),
                        Hit2B = Utilities.SafeDivide(summedStats.Hit2B, pa, -1),
                        Hit3B = Utilities.SafeDivide(summedStats.Hit3B, pa, -1),
                        HitHR = Utilities.SafeDivide(summedStats.HR, pa, -1),
                        BB = Utilities.SafeDivide(summedStats.BB, pa, -1),
                        HBP = Utilities.SafeDivide(summedStats.HBP, pa, -1),
                        K = Utilities.SafeDivide(summedStats.K, pa, -1),
                        SB = Utilities.SafeDivide(summedStats.SB, pa, -1),
                        CS = Utilities.SafeDivide(summedStats.CS, pa, -1),
                    };
                    db.League_HitterStats.Add(lhs);

                    progressBar.Tick();
                }
            }
            db.SaveChanges();

            return true;
        }

        private static bool LeagueHitterYearStats(SqliteDbContext db, int year, int month)
        {
            db.League_HitterYearStats.RemoveRange(db.League_HitterYearStats.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            var leagues =  year == 2005 ? 
                db.Player_Hitter_MonthStats.Where(f => f.Year == year).Select(f => f.LeagueId).Distinct().ToList() :
                db.Player_Hitter_MonthStats.Where(f => ((f.Year == year && f.Month <= month) || (f.Year == year - 1 && f.Month > month))).Select(f => f.LeagueId).Distinct().ToList();
            using (ProgressBar progressBar = new ProgressBar(leagues.Count(), $"Calculating Hitting League Year Baselines for {year}-{month}"))
            {
                foreach (int leagueId in leagues)
                {
                    var leagueStats = year == 2005 ? // Need full year for 2005 since 2004 doesn't have data, otherwise trailing year
                        db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.LeagueId == leagueId).AsEnumerable() :
                        db.Player_Hitter_MonthStats.Where(f => ((f.Year == year && f.Month <= month) || (f.Year == year - 1 && f.Month > month)) && f.LeagueId == leagueId).AsEnumerable();
                    if (!leagueStats.Any()) // Skip empty
                    {
                        progressBar.Tick();
                        continue;
                    }

                    var ls_check = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId);
                    LeagueStats ls = ls_check.Any() ? ls_check.Single() :
                        db.LeagueStats.Where(f => f.Year == year - 1 && f.LeagueId == leagueId).Single();
                    

                    // Sum all stats at that level
                    Player_Hitter_MonthStats summedStats = leagueStats.Aggregate((a, b) => new Player_Hitter_MonthStats
                    {
                        MlbId = 0,
                        Month = 0,
                        Year = 0,
                        AB = a.AB + b.AB,
                        PA = a.PA + b.PA,
                        H = a.H + b.H,
                        Hit2B = a.Hit2B + b.Hit2B,
                        Hit3B = a.Hit3B + b.Hit3B,
                        HR = a.HR + b.HR,
                        K = a.K + b.K,
                        BB = a.BB + b.BB,
                        SB = a.SB + b.SB,
                        CS = a.CS + b.CS,
                        HBP = a.HBP + b.HBP,
                        LevelId = 0,
                        LeagueId = 0,
                        ParkRunFactor = 0,
                        ParkHRFactor = 0,
                        GamesC = 0,
                        Games1B = 0,
                        Games2B = 0,
                        Games3B = 0,
                        GamesSS = 0,
                        GamesLF = 0,
                        GamesCF = 0,
                        GamesRF = 0,
                        GamesDH = 0
                    });

                    // Transform to get desired stats
                    float avg = (float)summedStats.H / summedStats.AB;
                    int singles = summedStats.H - summedStats.HR - summedStats.Hit2B - summedStats.Hit3B;
                    float iso = (float)(summedStats.Hit2B + (2 * summedStats.Hit3B) + (3 * summedStats.HR)) / summedStats.AB;
                    int pa = summedStats.AB + summedStats.BB + summedStats.HBP;
                    float obp = (float)(summedStats.BB + summedStats.HBP + summedStats.H) / pa;
                    float slg = avg + iso;
                    float woba = Utilities.CalculateWOBA(ls, summedStats.HBP, summedStats.BB, singles, summedStats.Hit2B, summedStats.Hit3B, summedStats.HR, pa);

                    League_HitterYearStats lhs = new League_HitterYearStats
                    {
                        LeagueId = leagueId,
                        Year = year,
                        Month = month,
                        AB = summedStats.AB,
                        AVG = avg,
                        OBP = obp,
                        SLG = slg,
                        ISO = iso,
                        WOBA = woba,
                        HRPerc = (float)summedStats.HR / pa,
                        BBPerc = (float)summedStats.BB / pa,
                        KPerc = (float)summedStats.K / pa,
                        SBRate = (float)summedStats.SB / pa,
                        SBPerc = (float)summedStats.SB / (summedStats.SB + summedStats.CS),
                        Hit1B = Utilities.SafeDivide(singles, pa, -1),
                        Hit2B = Utilities.SafeDivide(summedStats.Hit2B, pa, -1),
                        Hit3B = Utilities.SafeDivide(summedStats.Hit3B, pa, -1),
                        HitHR = Utilities.SafeDivide(summedStats.HR, pa, -1),
                        BB = Utilities.SafeDivide(summedStats.BB, pa, -1),
                        HBP = Utilities.SafeDivide(summedStats.HBP, pa, -1),
                        K = Utilities.SafeDivide(summedStats.K, pa, -1),
                        SB = Utilities.SafeDivide(summedStats.SB, pa, -1),
                        CS = Utilities.SafeDivide(summedStats.CS, pa, -1),
                    };
                    db.League_HitterYearStats.Add(lhs);

                    progressBar.Tick();
                }
            }
            db.SaveChanges();

            return true;
        }

        private static bool LeaguePitcherStats(SqliteDbContext db, int year, int month)
        {
            db.League_PitcherStats.RemoveRange(db.League_PitcherStats.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            var leagues = db.Player_Pitcher_MonthStats.Where(f => f.Year == year && f.Month == month).Select(f => f.LeagueId).Distinct().ToList();
            using (ProgressBar progressBar = new ProgressBar(leagues.Count(), $"Calculating Pitching League Baselines for {year}-{month}"))
            {
                foreach (int leagueId in leagues)
                {
                    var leagueStats = year == 2005 ? // Need full year for 2005 since 2004 doesn't have data, otherwise trailing year
                        db.Player_Pitcher_MonthStats.Where(f => f.Year == year && f.LeagueId == leagueId).AsEnumerable() :
                        db.Player_Pitcher_MonthStats.Where(f => ((f.Year == year && f.Month <= month) || (f.Year == year - 1 && f.Month > month)) && f.LeagueId == leagueId).AsEnumerable();
                    if (!leagueStats.Any()) // Skip empty
                    {
                        progressBar.Tick();
                        continue;
                    }

                    LeagueStats ls = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId).Single();

                    // Sum all stats at that level
                    Player_Pitcher_MonthStats summedStats = leagueStats.Aggregate((a, b) => new Player_Pitcher_MonthStats
                    {
                        MlbId = 0,
                        Month = 0,
                        Year = 0,
                        BattersFaced = a.BattersFaced + b.BattersFaced,
                        H = a.H + b.H,
                        Hit2B = a.Hit2B + b.Hit2B,
                        Hit3B = a.Hit3B + b.Hit3B,
                        HR = a.HR + b.HR,
                        K = a.K + b.K,
                        BB = a.BB + b.BB,
                        HBP = a.HBP + b.HBP,
                        R = a.R + b.R,
                        ER = a.ER + b.ER,
                        Outs = a.Outs + b.Outs,
                        GO = a.GO + b.GO,
                        AO = a.AO + b.AO,
                        LevelId = 0,
                        LeagueId = 0,
                        SPPerc = 0,
                        ParkHRFactor = 0,
                        ParkRunFactor = 0
                    });

                    // Transform to get desired stats
                    int ab = summedStats.BattersFaced - summedStats.BB + summedStats.HBP;
                    float avg = (float)summedStats.H / ab;
                    int singles = summedStats.H - summedStats.HR - summedStats.Hit2B - summedStats.Hit3B;
                    float iso = (float)(summedStats.Hit2B + (2 * summedStats.Hit3B) + (3 * summedStats.HR)) / ab;
                    int pa = summedStats.BattersFaced;
                    float woba = Utilities.CalculateWOBA(ls, summedStats.HBP, summedStats.BB, singles, summedStats.Hit2B, summedStats.Hit3B, summedStats.HR, pa);
                    float era = (float)summedStats.ER / ((float)summedStats.Outs / 27);
                    float ra = (float)summedStats.R / ((float)summedStats.Outs / 27);

                    League_PitcherStats lps = new League_PitcherStats
                    {
                        LeagueId = leagueId,
                        Year = year,
                        Month = month,
                        ERA = era,
                        RA = ra,
                        FipConstant = ls.CFIP,
                        WOBA = woba,
                        HRPerc = (float)summedStats.HR / pa,
                        BBPerc = (float)summedStats.BB / pa,
                        KPerc = (float)summedStats.K / pa,
                        GOPerc = (float)summedStats.GO / (summedStats.GO + summedStats.AO),
                        Avg = avg,
                        Iso = iso
                    };
                    db.League_PitcherStats.Add(lps);

                    progressBar.Tick();
                }
            }
            db.SaveChanges();

            return true;
        }

        private static bool LeaguePitcherYearStats(SqliteDbContext db, int year, int month)
        {
            db.League_PitcherYearStats.RemoveRange(db.League_PitcherYearStats.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            var leagues = year == 2005 ?
                db.Player_Pitcher_MonthStats.Where(f => f.Year == year).Select(f => f.LeagueId).Distinct().ToList() :
                db.Player_Pitcher_MonthStats.Where(f => ((f.Year == year && f.Month <= month) || (f.Year == year - 1 && f.Month > month))).Select(f => f.LeagueId).Distinct().ToList();
            using (ProgressBar progressBar = new ProgressBar(leagues.Count(), $"Calculating Pitching League Year Baselines for {year}-{month}"))
            {
                foreach (int leagueId in leagues)
                {
                    var leagueStats = db.Player_Pitcher_MonthStats.Where(f => f.Year == year && f.Month == month && f.LeagueId == leagueId).AsEnumerable();
                    if (!leagueStats.Any()) // Skip empty
                    {
                        progressBar.Tick();
                        continue;
                    }

                    var ls_check = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId);
                    LeagueStats ls = ls_check.Any() ? ls_check.Single() :
                        db.LeagueStats.Where(f => f.Year == year - 1 && f.LeagueId == leagueId).Single();

                    // Sum all stats at that level
                    Player_Pitcher_MonthStats summedStats = leagueStats.Aggregate((a, b) => new Player_Pitcher_MonthStats
                    {
                        MlbId = 0,
                        Month = 0,
                        Year = 0,
                        BattersFaced = a.BattersFaced + b.BattersFaced,
                        H = a.H + b.H,
                        Hit2B = a.Hit2B + b.Hit2B,
                        Hit3B = a.Hit3B + b.Hit3B,
                        HR = a.HR + b.HR,
                        K = a.K + b.K,
                        BB = a.BB + b.BB,
                        HBP = a.HBP + b.HBP,
                        R = a.R + b.R,
                        ER = a.ER + b.ER,
                        Outs = a.Outs + b.Outs,
                        GO = a.GO + b.GO,
                        AO = a.AO + b.AO,
                        LevelId = 0,
                        LeagueId = 0,
                        SPPerc = 0,
                        ParkHRFactor = 0,
                        ParkRunFactor = 0
                    });

                    // Transform to get desired stats
                    int ab = summedStats.BattersFaced - summedStats.BB + summedStats.HBP;
                    float avg = (float)summedStats.H / ab;
                    int singles = summedStats.H - summedStats.HR - summedStats.Hit2B - summedStats.Hit3B;
                    float iso = (float)(summedStats.Hit2B + (2 * summedStats.Hit3B) + (3 * summedStats.HR)) / ab;
                    int pa = summedStats.BattersFaced;
                    float woba = Utilities.CalculateWOBA(ls, summedStats.HBP, summedStats.BB, singles, summedStats.Hit2B, summedStats.Hit3B, summedStats.HR, pa);
                    float era = (float)summedStats.ER / ((float)summedStats.Outs / 27);
                    float ra = (float)summedStats.R / ((float)summedStats.Outs / 27);

                    League_PitcherYearStats lps = new League_PitcherYearStats
                    {
                        LeagueId = leagueId,
                        Year = year,
                        Month = month,
                        ERA = era,
                        RA = ra,
                        FipConstant = ls.CFIP,
                        WOBA = woba,
                        HRPerc = (float)summedStats.HR / pa,
                        BBPerc = (float)summedStats.BB / pa,
                        KPerc = (float)summedStats.K / pa,
                        GOPerc = (float)summedStats.GO / (summedStats.GO + summedStats.AO),
                        Avg = avg,
                        Iso = iso
                    };
                    db.League_PitcherYearStats.Add(lps);

                    progressBar.Tick();
                }
            }
            db.SaveChanges();

            return true;
        }

        public static bool Main(int year, int month)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            try {
                if (!LeagueHitterStats(db, year, month))
                    return false;

                if (!LeagueHitterYearStats(db, year, month))
                    return false;

                if (!LeaguePitcherStats(db, year, month))
                    return false;

                if (!LeaguePitcherYearStats(db, year, month))
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("failed CalculateLeagueBaselines");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
