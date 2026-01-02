using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using System;
using System.Threading.Tasks;

namespace DataAquisition
{
    internal class ModelMonthStats
    {
        private const int NUM_THREADS = 24;
        private static int progress_bar_thread = 0;
        private static List<int> thread_counts;

        private static int endYear = 0;
        private static int endMonth = 0;

        private static List<Model_HitterStats> HitterStatsThreadFunction(int[] ids, int thread_idx, ProgressBar progressBar, int progressSum)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            List<Model_HitterStats> output = new(ids.Length * 70);
            IProgress<float> progress = progressBar.AsProgress<float>();

            // Create dictionary to store what levels have data
            Dictionary<(int, int, int), bool> GamesAtLevelDict = new();
            Dictionary<(int, int, int), float> GamesFracDict = new();
            var allLevels = db.Level_GameCounts.Select(f => f.LevelId).Distinct();
            var allYears = db.Level_GameCounts.Select(f => f.Year).Distinct();
            var allMonths = db.Level_GameCounts.Select(f => f.Month).Distinct();
            foreach (int month in allMonths)
                foreach (int level in allLevels)
                    foreach (int year in allYears)
                    {
                        GamesAtLevelDict.Add((month, level, year), Utilities.GamesAtLevel(month, level, year, db));
                        GamesFracDict.Add((month, level, year), Utilities.GetGamesFrac(month, level, year, db));
                    }

            foreach (int id in ids)
            {
                var hitter = db.Model_Players.Where(f => f.MlbId == id).Single();

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
                    TrainMask = -1,
                    MonthFrac = -1,
                    ParkHRFactor = -1,
                    ParkRunFactor = -1,
                    Year = -1,
                    Month = -1,
                    LevelId = -1,
                    AVGRatio = -1,
                    OBPRatio = -1,
                    ISORatio = -1,
                    WRC = 100,
                    CrWAR = -100000,
                    CrBSR = -100000,
                    CrDRAA = -100000,
                    CrDPOS = -100000,
                    CrOFF = -100000,
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
                    PercDH = -1,
                    Hit1B = -1,
                    Hit2B = -1,
                    Hit3B = -1,
                    HitHR = -1,
                    BB = -1,
                    HBP = -1,
                    K = -1,
                    SB = -1,
                    CS = -1,
                };

                // Generate Hitter Stats
                foreach (var r in ratios)
                {
                    int level = r.LevelId == 1 ? 1 : r.LevelId - 9;
                    var stat = db.Player_Hitter_MonthStats.Where(f => f.MlbId == r.MlbId && f.Year == r.Year && f.Month == r.Month && f.LeagueId == r.LeagueId)
                        .Select(f => new { f.H, f.Hit2B, f.Hit3B, f.HR, f.PA, f.BB, f.HBP, f.K, f.SB, f.CS, f.AB, f.ParkHRFactor, f.ParkRunFactor }).Single();

                    float model_month = r.Month + 1;
                    League_HitterStats lhs = db.League_HitterStats.Where(f => f.Year == r.Year && f.Month == r.Month && f.LeagueId == r.LeagueId).Single();
                    float hit1BRatio = Utilities.SafeDivide((stat.H - stat.Hit2B - stat.Hit3B - stat.HR) / lhs.Hit1B, stat.PA);
                    float hit2BRatio = Utilities.SafeDivide(stat.Hit2B, stat.PA * lhs.Hit2B);
                    float hit3BRatio = Utilities.SafeDivide(stat.Hit3B, stat.PA * lhs.Hit3B);
                    float hitHRRatio = Utilities.SafeDivide(stat.HR, stat.PA * lhs.HitHR);
                    float hitBBRatio = Utilities.SafeDivide(stat.BB, stat.PA * lhs.BB);
                    float hitHBPRatio = Utilities.SafeDivide(stat.HBP, stat.PA * lhs.HBP);
                    float hitKRatio = Utilities.SafeDivide(stat.K, stat.PA * lhs.K);
                    float hitSBRatio = Utilities.SafeDivide(stat.SB, stat.PA * lhs.SB);
                    float hitCSRatio = Utilities.SafeDivide(stat.CS, stat.PA * lhs.CS);

                    if (r.Year == prevYear && r.Month == prevMonth)
                    {
                        int PA = stat.AB + stat.BB + stat.HBP;
                        float prop = (currentData.PA + PA) > 0 ? (float)(PA) / (currentData.PA + PA) : 0.5f;
                        float invProp = 1 - prop;

                        currentData.PA += PA;
                        currentData.MonthFrac = (GamesFracDict[(prevMonth, Utilities.ModelLevelToMlbLevel(level), prevYear)] * prop) + (currentData.MonthFrac * invProp);
                        currentData.ParkHRFactor = (stat.ParkHRFactor * prop) + (currentData.ParkHRFactor * invProp);
                        currentData.ParkRunFactor = (stat.ParkRunFactor * prop) + (currentData.ParkRunFactor * invProp);
                        currentData.LevelId = (level * prop) + (currentData.LevelId * invProp);
                        currentData.AVGRatio = (r.AVGRatio * prop) + (currentData.AVGRatio * invProp);
                        currentData.OBPRatio = (r.OBPRatio * prop) + (currentData.OBPRatio * invProp);
                        currentData.ISORatio = (r.ISORatio * prop) + (currentData.ISORatio * invProp);
                        currentData.WRC = (r.WRC * prop) + (currentData.WRC * invProp);
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
                        currentData.Hit1B = (hit1BRatio * prop) + (currentData.Hit1B * invProp);
                        currentData.Hit2B = (hit2BRatio * prop) + (currentData.Hit2B * invProp);
                        currentData.Hit3B = (hit3BRatio * prop) + (currentData.Hit3B * invProp);
                        currentData.HitHR = (hitHRRatio * prop) + (currentData.HitHR * invProp);
                        currentData.BB = (hitBBRatio * prop) + (currentData.BB * invProp);
                        currentData.HBP = (hitHBPRatio * prop) + (currentData.HBP * invProp);
                        currentData.K = (hitKRatio * prop) + (currentData.K * invProp);
                        currentData.SB = (hitSBRatio * prop) + (currentData.SB * invProp);
                        currentData.CS = (hitCSRatio * prop) + (currentData.CS * invProp);
                    }
                    else // New Month
                    {
                        // Add Previous
                        if (currentData.Age > 0)
                        {
                            currentData.WRC = Utilities.ClampWRC(currentData.WRC);
                            output.Add(currentData.Clone());
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
                                if (GamesAtLevelDict[(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear)])
                                    output.Add(new Model_HitterStats
                                    {
                                        MlbId = r.MlbId,
                                        Year = prevYear,
                                        Month = prevMonth,
                                        Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                        PA = 0,
                                        TrainMask = Utilities.GetModelMask(hitter, prevYear, prevMonth),
                                        InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, r.MlbId, db),
                                        MonthFrac = GamesFracDict[(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear)],
                                        LevelId = prevLevelInt,
                                        ParkHRFactor = 1,
                                        ParkRunFactor = 1,
                                        AVGRatio = 1,
                                        OBPRatio = 1,
                                        ISORatio = 1,
                                        WRC = 100,
                                        CrWAR = 0,
                                        CrBSR = 0,
                                        CrDPOS = 0,
                                        CrDRAA = 0,
                                        CrOFF = 0,
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
                                        PercDH = 0,
                                        Hit1B = 1,
                                        Hit2B = 1,
                                        Hit3B = 1,
                                        HitHR = 1,
                                        BB = 1,
                                        HBP = 1,
                                        K = 1,
                                        SB = 1,
                                        CS = 1,
                                    });

                                prevMonth++;
                                if (prevMonth > 9)
                                {
                                    prevMonth = 4;
                                    prevYear++;
                                }
                            }
                        }

                        var phma = db.Player_Hitter_MonthAdvanced.Where(f => f.MlbId == hitter.MlbId && f.Year == r.Year && f.Month == r.Month)
                            .Select(f => new { f.CrWAR, f.CrOFF });
                        var fieldingStats = db.Player_Fielder_MonthStats.Where(f => f.MlbId == hitter.MlbId && f.Year == r.Year && f.Month == r.Month)
                            .Select(f => new { f.ScaledDRAA, f.PosAdjust });

                        currentData.Age = Utilities.GetAge1MinusAge0(r.Year, r.Month, 15, player.BirthYear, player.BirthMonth, player.BirthDate);
                        currentData.PA = stat.AB + stat.BB + stat.HBP;
                        currentData.TrainMask = Utilities.GetModelMask(hitter, r.Year, r.Month);
                        currentData.InjStatus = Utilities.GetInjStatus(r.Month, r.Year, r.MlbId, db);
                        currentData.ParkHRFactor = stat.ParkHRFactor;
                        currentData.ParkRunFactor = stat.ParkRunFactor;
                        currentData.Year = r.Year;
                        currentData.Month = r.Month;
                        currentData.LevelId = level;
                        currentData.MonthFrac = GamesFracDict[(r.Month, Utilities.ModelLevelToMlbLevel(level), r.Year)];
                        currentData.AVGRatio = r.AVGRatio;
                        currentData.OBPRatio = r.OBPRatio;
                        currentData.ISORatio = r.ISORatio;
                        currentData.WRC = r.WRC;
                        currentData.CrWAR = phma.Sum(f => f.CrWAR);
                        currentData.CrOFF = phma.Sum(f => f.CrOFF);
                        currentData.CrDPOS = fieldingStats.Sum(f => f.PosAdjust);
                        currentData.CrDRAA = fieldingStats.Sum(f => f.ScaledDRAA);
                        currentData.CrBSR = db.Player_Hitter_MonthBaserunning.Where(f => f.MlbId == hitter.MlbId && f.Year == r.Year && f.Month == r.Month)
                            .Sum(f => f.RBSR);
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
                        currentData.Hit1B = hit1BRatio;
                        currentData.Hit2B = hit2BRatio;
                        currentData.Hit3B = hit3BRatio;
                        currentData.HitHR = hitHRRatio;
                        currentData.BB = hitBBRatio;
                        currentData.HBP = hitHBPRatio;
                        currentData.K = hitKRatio;
                        currentData.SB = hitSBRatio;
                        currentData.CS = hitCSRatio;

                        prevMonth = r.Month;
                        prevYear = r.Year;
                        prevLevel = level;
                    }
                }

                // Add last result
                if (ratios.Any())
                {
                    currentData.WRC = Utilities.ClampWRC(currentData.WRC);
                    output.Add(currentData.Clone());
                }


                // Make sure that trailing gaps are included
                if (ratios.Any())
                {
                    int lastYear = ratios.Last().Year;
                    int pLevelInt = Convert.ToInt32(Math.Floor(prevLevel));

                    prevMonth++;
                    if (prevMonth > 9)
                    {
                        prevMonth = 4;
                        prevYear++;
                    }
                    while (prevYear <= Math.Min(lastYear + 2, endYear))
                    {
                        // Fill Hitter Gaps
                        if (GamesAtLevelDict[(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear)])
                            output.Add(new Model_HitterStats
                            {
                                MlbId = hitter.MlbId,
                                Year = prevYear,
                                Month = prevMonth,
                                Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                PA = 0,
                                TrainMask = Utilities.GetModelMask(hitter, prevYear, prevMonth),
                                InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, hitter.MlbId, db),
                                MonthFrac = GamesFracDict[(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear)],
                                LevelId = pLevelInt,
                                ParkHRFactor = 1,
                                ParkRunFactor = 1,
                                AVGRatio = 1,
                                OBPRatio = 1,
                                ISORatio = 1,
                                WRC = 100,
                                CrWAR = 0,
                                CrBSR = 0,
                                CrDPOS = 0,
                                CrDRAA = 0,
                                CrOFF = 0,
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
                                PercDH = 0,
                                Hit1B = 1,
                                Hit2B = 1,
                                Hit3B = 1,
                                HitHR = 1,
                                BB = 1,
                                HBP = 1,
                                K = 1,
                                SB = 1,
                                CS = 1,
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
                        if (GamesAtLevelDict[(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y)])
                            output.Add(new Model_HitterStats
                            {
                                MlbId = hitter.MlbId,
                                Year = y,
                                Month = m,
                                Age = Utilities.GetAge1MinusAge0(y, m, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                PA = 0,
                                TrainMask = Utilities.GetModelMask(hitter, y, m),
                                InjStatus = Utilities.GetInjStatus(m, y, hitter.MlbId, db),
                                MonthFrac = GamesFracDict[(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y)],
                                LevelId = MISSING_LEVEL,
                                ParkHRFactor = 1,
                                ParkRunFactor = 1,
                                AVGRatio = 1,
                                OBPRatio = 1,
                                ISORatio = 1,
                                WRC = 100,
                                CrWAR = 0,
                                CrBSR = 0,
                                CrDPOS = 0,
                                CrDRAA = 0,
                                CrOFF = 0,
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
                                PercDH = 0,
                                Hit1B = 1,
                                Hit2B = 1,
                                Hit3B = 1,
                                HitHR = 1,
                                BB = 1,
                                HBP = 1,
                                K = 1,
                                SB = 1,
                                CS = 1,
                            });

                        m++;
                        if (m > 9)
                        {
                            m = 4;
                            y++;
                        }
                    }
                }

                thread_counts[thread_idx]++; // Allows for tracking progress

                if (thread_idx == progress_bar_thread) // Only update from single thread
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / progressSum);
                }
            }

            return output;
        }

        public static List<Model_PitcherStats> PitcherStatsThreadFunction(int[] ids, int thread_idx, ProgressBar progressBar, int progressSum)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            List<Model_PitcherStats> output = new(ids.Length * 70);
            IProgress<float> progress = progressBar.AsProgress<float>();

            // Create dictionary to store what levels have data
            Dictionary<(int, int, int), bool> GamesAtLevelDict = new();
            Dictionary<(int, int, int), float> GamesFracDict = new();
            var allLevels = db.Level_GameCounts.Select(f => f.LevelId).Distinct();
            var allYears = db.Level_GameCounts.Select(f => f.Year).Distinct();
            var allMonths = db.Level_GameCounts.Select(f => f.Month).Distinct();
            foreach (int month in allMonths)
                foreach (int level in allLevels)
                    foreach (int year in allYears)
                    {
                        GamesAtLevelDict.Add((month, level, year), Utilities.GamesAtLevel(month, level, year, db));
                        GamesFracDict.Add((month, level, year), Utilities.GetGamesFrac(month, level, year, db));
                    }

            foreach (int id in ids)
            {
                Model_Players pitcher = db.Model_Players.Where(f => f.MlbId == id).Single();

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
                    TrainMask = -1,
                    InjStatus = -1,
                    MonthFrac = -1,
                    ParkHRFactor = -1,
                    ParkRunFactor = -1,
                    SpPerc = -1,
                    Year = -1,
                    Month = -1,
                    LevelId = -1,
                    WOBARatio = -1,
                    HRPercRatio = -1,
                    BBPercRatio = -1,
                    KPercRatio = -1,
                    GBPercRatio = -1,
                    ERARatio = -1,
                    FIPRatio = -1,
                    CrWAR = -100000,
                };

                // Generate Hitter Stats
                foreach (var r in ratios)
                {
                    int level = r.LevelId == 1 ? 1 : r.LevelId - 9;
                    var stat = db.Player_Pitcher_MonthStats.Where(f => f.MlbId == r.MlbId && f.Year == r.Year && f.Month == r.Month && f.LeagueId == r.LeagueId).Single();
                    if (r.Year == prevYear && r.Month == prevMonth)
                    {

                        float prop = (currentData.BF + stat.BattersFaced) > 0 ? (float)(stat.BattersFaced) / (currentData.BF + stat.BattersFaced) : 0.5f;
                        float invProp = 1 - prop;

                        currentData.BF += stat.BattersFaced;
                        currentData.MonthFrac = (GamesFracDict[(prevMonth, Utilities.ModelLevelToMlbLevel(level), prevYear)] * prop) + (currentData.MonthFrac * invProp);
                        currentData.ParkHRFactor = (stat.ParkHRFactor * prop) + (currentData.ParkHRFactor * invProp);
                        currentData.ParkRunFactor = (stat.ParkRunFactor * prop) + (currentData.ParkRunFactor * invProp);
                        currentData.SpPerc = (stat.SPPerc * prop) + (currentData.SpPerc * invProp);
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
                            output.Add(currentData.Clone());
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

                                if (GamesAtLevelDict[(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear)])
                                    output.Add(new Model_PitcherStats
                                    {
                                        MlbId = r.MlbId,
                                        Year = prevYear,
                                        Month = prevMonth,
                                        Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                        BF = 0,
                                        TrainMask = Utilities.GetModelMask(pitcher, prevYear, prevMonth),
                                        InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, r.MlbId, db),
                                        MonthFrac = GamesFracDict[(prevMonth, Utilities.ModelLevelToMlbLevel(prevLevelInt), prevYear)],
                                        LevelId = prevLevelInt,
                                        ParkHRFactor = 1,
                                        ParkRunFactor = 1,
                                        SpPerc = 0.5f,
                                        WOBARatio = 1,
                                        HRPercRatio = 1,
                                        BBPercRatio = 1,
                                        KPercRatio = 1,
                                        GBPercRatio = 1,
                                        FIPRatio = 1,
                                        ERARatio = 1,
                                        CrWAR = 0,
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
                        currentData.TrainMask = Utilities.GetModelMask(pitcher, r.Year, r.Month);
                        currentData.InjStatus = Utilities.GetInjStatus(r.Month, r.Year, r.MlbId, db);
                        currentData.ParkHRFactor = stat.ParkHRFactor;
                        currentData.ParkRunFactor = stat.ParkRunFactor;
                        currentData.SpPerc = stat.SPPerc;
                        currentData.Year = r.Year;
                        currentData.Month = r.Month;
                        currentData.LevelId = level;
                        currentData.MonthFrac = GamesFracDict[(r.Month, Utilities.ModelLevelToMlbLevel(level), r.Year)];
                        currentData.WOBARatio = r.WOBARatio;
                        currentData.HRPercRatio = r.HRPercRatio;
                        currentData.BBPercRatio = r.BBPercRatio;
                        currentData.KPercRatio = r.KPercRatio;
                        currentData.GBPercRatio = r.GBPercRatio;
                        currentData.ERARatio = r.ERARatio;
                        currentData.FIPRatio = r.FIPRatio;
                        currentData.CrWAR = db.Player_Pitcher_MonthAdvanced.Where(f => f.MlbId == pitcher.MlbId && f.Year == r.Year && f.Month == r.Month).Select(f => f.CrWAR).Sum();

                        prevMonth = r.Month;
                        prevYear = r.Year;
                        prevLevel = level;
                    }
                }

                // Add last result
                if (ratios.Any())
                    output.Add(currentData.Clone());

                // Make sure that trailing gaps are included
                if (ratios.Any())
                {
                    int lastYear = ratios.Last().Year;
                    int pLevelInt = Convert.ToInt32(Math.Floor(prevLevel));

                    prevMonth++;
                    if (prevMonth > 9)
                    {
                        prevMonth = 4;
                        prevYear++;
                    }

                    while (prevYear <= Math.Min(lastYear + 2, endYear))
                    {
                        // Fill Hitter Gaps
                        //Console.WriteLine($"{prevMonth}, {level}, {prevYear}");

                        if (GamesAtLevelDict[(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear)])
                            output.Add(new Model_PitcherStats
                            {
                                MlbId = pitcher.MlbId,
                                Year = prevYear,
                                Month = prevMonth,
                                Age = Utilities.GetAge1MinusAge0(prevYear, prevMonth, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                BF = 0,
                                TrainMask = Utilities.GetModelMask(pitcher, prevYear, prevMonth),
                                InjStatus = Utilities.GetInjStatus(prevMonth, prevYear, pitcher.MlbId, db),
                                MonthFrac = GamesFracDict[(prevMonth, Utilities.ModelLevelToMlbLevel(pLevelInt), prevYear)],
                                LevelId = pLevelInt,
                                ParkHRFactor = 1,
                                ParkRunFactor = 1,
                                SpPerc = 0.5f,
                                WOBARatio = 1,
                                HRPercRatio = 1,
                                BBPercRatio = 1,
                                KPercRatio = 1,
                                GBPercRatio = 1,
                                FIPRatio = 1,
                                ERARatio = 1,
                                CrWAR = 0,
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
                        if (GamesAtLevelDict[(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y)])
                            output.Add(new Model_PitcherStats
                            {
                                MlbId = pitcher.MlbId,
                                Year = y,
                                Month = m,
                                Age = Utilities.GetAge1MinusAge0(y, m, 15, player.BirthYear, player.BirthMonth, player.BirthDate),
                                BF = 0,
                                TrainMask = Utilities.GetModelMask(pitcher, y, m),
                                InjStatus = Utilities.GetInjStatus(m, y, pitcher.MlbId, db),
                                MonthFrac = GamesFracDict[(m, Utilities.ModelLevelToMlbLevel(MISSING_LEVEL), y)],
                                LevelId = MISSING_LEVEL,
                                SpPerc = 0.5f,
                                ParkHRFactor = 1,
                                ParkRunFactor = 1,
                                WOBARatio = 1,
                                HRPercRatio = 1,
                                BBPercRatio = 1,
                                KPercRatio = 1,
                                GBPercRatio = 1,
                                FIPRatio = 1,
                                ERARatio = 1,
                                CrWAR = 0,
                            });

                        m++;
                        if (m > 9)
                        {
                            m = 4;
                            y++;
                        }
                    }
                }

                thread_counts[thread_idx]++; // Allows for tracking progress

                if (thread_idx == progress_bar_thread) // Only update from single thread
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / progressSum);
                }
            }

            return output;
        }

        public static async Task<bool> Main(int EndYear, int EndMonth)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Model_HitterStats.ExecuteDelete();
                db.Model_PitcherStats.ExecuteDelete();

                endYear = EndYear;
                endMonth = EndMonth;

                // Get Hitter Stats
                int[] ids = db.Model_Players.Where(f => f.IsHitter == 1).Select(f => f.MlbId).ToArray();
                int j = 0;
                List<IEnumerable<int>> id_partitions = (from item in ids
                                                              group item by j++ % NUM_THREADS into part
                                                              select part.AsEnumerable()).ToList();

                List<Task<List<Model_HitterStats>>> hitterTasks = new(NUM_THREADS);
                using (ProgressBar progressBar = new(ids.Count(), "Calculating Model Hitter Stats"))
                {
                    progress_bar_thread = 0;
                    thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

                    for (int i = 0; i < NUM_THREADS; i++)
                    {
                        int idx = i;
                        hitterTasks.Add(Task.Run(() => HitterStatsThreadFunction(id_partitions.ElementAt(idx).ToArray(), idx, progressBar, ids.Count())));
                    }

                    List<Model_HitterStats> output = new(ids.Count() * 70);
                    foreach (var task in hitterTasks)
                    {
                        var mhs = await task;
                        output.AddRange(mhs);
                        progress_bar_thread++;
                    }

                    db.BulkInsert(output);
                }

                // Get Pitcher Stats
                ids = db.Model_Players.Where(f => f.IsPitcher == 1).Select(f => f.MlbId).ToArray();
                j = 0;
                id_partitions = (from item in ids
                                group item by j++ % NUM_THREADS into part
                                select part.AsEnumerable()).ToList();

                List<Task<List<Model_PitcherStats>>> pitcherTasks = new(NUM_THREADS);
                using (ProgressBar progressBar = new(ids.Count(), "Calculating Model Pitcher Stats"))
                {
                    progress_bar_thread = 0;
                    thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

                    for (int i = 0; i < NUM_THREADS; i++)
                    {
                        int idx = i;
                        pitcherTasks.Add(Task.Run(() => PitcherStatsThreadFunction(id_partitions.ElementAt(idx).ToArray(), idx, progressBar, ids.Count())));
                    }

                    List<Model_PitcherStats> output = new(ids.Count() * 70);
                    foreach (var task in pitcherTasks)
                    {
                        var mhs = await task;
                        output.AddRange(mhs);
                        progress_bar_thread++;
                    }

                    db.BulkInsert(output);
                }

                // Set ignore players train masks to 0
                var ignore_pcs = db.Player_CareerStatus.Where(f => f.IgnorePlayer != null);
                foreach (var pcs in ignore_pcs)
                {
                    foreach (var item in db.Model_HitterStats.Where(f => f.MlbId == pcs.MlbId))
                    {
                        item.TrainMask = 0;
                    }
                    foreach (var item in db.Model_PitcherStats.Where(f => f.MlbId == pcs.MlbId))
                    {
                        item.TrainMask = 0;
                    }
                }
                db.SaveChanges();

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
