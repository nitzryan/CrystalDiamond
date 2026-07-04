using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition.College
{
    internal class ModelStats
    {
        public static bool CreateHitterModelStats()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.Model_College_HitterYear.ExecuteDelete();

            // Construct dictionary to lookup conf stats
            Dictionary<(int confId, int year), College_ConfHitterAvg> confLookup = new();
            foreach (var c in db.College_ConfHitterAvg)
            {
                confLookup[(c.ConfId, c.Year)] = c;
            }

            // Construct dictionary of Conference Strengths
            Dictionary<(int confId, int year), float> confRankLookup = new();
            foreach (var c in db.College_ConferenceRank)
            {
                confRankLookup[(c.ConfId, c.Year)] = c.AvgRPI;
            }

            // Construct dictionary to lookup park factors
            Dictionary<(int teamId, int year), float> parkLookup = new();
            foreach (var pf in db.College_ParkFactors)
            {
                parkLookup[(pf.TeamId, pf.Year)] = pf.RunFactor;
            }

            // Iterate through all players
            List<Model_College_HitterYear> modelStats = new();
            modelStats.Capacity = db.College_HitterStats.Count();
            using (ProgressBar progressBar = new(db.College_HitterStats.Count(), $"Calculating College Hitter Model Stats"))
            {
                foreach (var stat in db.College_HitterStats)
                {
                    progressBar.Tick();

                    if (stat.PA == 0)
                        continue;

                    // Skip covid year
                    if (stat.Year == 2020)
                        continue;

                    College_ConfHitterAvg confAvg = confLookup[(stat.ConfId, stat.Year)];


                    float parkFactor = 1;
                    try
                    {
                        parkFactor = parkLookup[(stat.TeamId, stat.Year)];
                    }
                    catch (Exception)
                    {
                        // Doesn't exist
                    }
                    float confRank = confRankLookup[(stat.ConfId, stat.Year)];

                    // Cap invalid datapoints
                    int height = stat.Height > 0 ? stat.Height : ColUtilities.DEFAULT_HEIGHT;
                    float age = stat.Age;
                    if (stat.Age > 100 || stat.Age < 16 + stat.ExpYears)
                        age = ColUtilities.UKNOWN_START_AGE + stat.ExpYears;

                    modelStats.Add(new Model_College_HitterYear
                    {
                        TBCId = stat.TBCId,
                        Level = stat.Level,
                        Year = stat.Year,
                        ExpYears = stat.ExpYears,
                        ParkRunFactor = parkFactor,
                        ConfScore = confRank,
                        PA = stat.PA,
                        H = ColUtilities.NormalizeStat(stat.H, stat.AB, confAvg.H),
                        H2B = ColUtilities.NormalizeStat(stat.H2B, stat.AB, confAvg.H2B),
                        H3B = ColUtilities.NormalizeStat(stat.H3B, stat.AB, confAvg.H3B),
                        HR = ColUtilities.NormalizeStat(stat.HR, stat.AB, confAvg.HR),
                        SB = ColUtilities.NormalizeStat(stat.SB, stat.PA, confAvg.SB),
                        CS = ColUtilities.NormalizeStat(stat.CS, stat.PA, confAvg.CS),
                        BB = ColUtilities.NormalizeStat(stat.BB, stat.PA, confAvg.BB),
                        K = ColUtilities.NormalizeStat(stat.K, stat.PA, confAvg.K),
                        HBP = ColUtilities.NormalizeStat(stat.HBP, stat.PA, confAvg.HBP),
                        AVG = stat.AVG / confAvg.AVG,
                        OBP = stat.OBP / confAvg.OBP,
                        SLG = stat.SLG / confAvg.SLG,
                        OPS = stat.OPS / confAvg.OPS,
                        Age = stat.Age,
                        Pos = stat.Pos,
                        Height = height,
                        Weight = stat.Weight,
                    });
                }
            }

            db.Model_College_HitterYear.AddRange(modelStats);
            db.SaveChanges();

            return true;
        }

        public static bool CreatePitcherModelStats()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.Model_College_PitcherYear.ExecuteDelete();

            // Construct dictionary to lookup conf stats
            Dictionary<(int confId, int year), College_ConfPitcherAvg> confLookup = new();
            foreach (var c in db.College_ConfPitcherAvg)
            {
                confLookup[(c.ConfId, c.Year)] = c;
            }

            // Construct dictionary of Conference Strengths
            Dictionary<(int confId, int year), float> confRankLookup = new();
            foreach (var c in db.College_ConferenceRank)
            {
                confRankLookup[(c.ConfId, c.Year)] = c.AvgRPI;
            }

            // Construct dictionary to lookup park factors
            Dictionary<(int teamId, int year), float> parkLookup = new();
            foreach (var pf in db.College_ParkFactors)
            {
                parkLookup[(pf.TeamId, pf.Year)] = pf.RunFactor;
            }

            // Iterate through all players
            List<Model_College_PitcherYear> modelStats = new();
            modelStats.Capacity = db.College_PitcherStats.Count();
            using (ProgressBar progressBar = new(db.College_PitcherStats.Count(), $"Calculating College Pitcher Model Stats"))
            {
                foreach (var stat in db.College_PitcherStats)
                {
                    progressBar.Tick();

                    if (stat.G == 0)
                        continue;

                    // Skip covid year
                    if (stat.Year == 2020)
                        continue;

                    College_ConfPitcherAvg confAvg = confLookup[(stat.ConfId, stat.Year)];

                    float parkFactor = 1;
                    try
                    {
                        parkFactor = parkLookup[(stat.TeamId, stat.Year)];
                    }
                    catch (Exception)
                    {
                        // Doesn't exist
                    }
                    float confRank = confRankLookup[(stat.ConfId, stat.Year)];

                    // Cap invalid datapoints
                    int height = stat.Height > 0 ? stat.Height : ColUtilities.DEFAULT_HEIGHT;
                    float age = stat.Age;
                    if (stat.Age > 100 || stat.Age < 16 + stat.ExpYears)
                        age = ColUtilities.UKNOWN_START_AGE + stat.ExpYears;

                    modelStats.Add(new Model_College_PitcherYear
                    {
                        TBCId = stat.TBCId,
                        Level = stat.Level,
                        Year = stat.Year,
                        ExpYears = stat.ExpYears,
                        ParkRunFactor = parkFactor,
                        ConfScore = confRank,
                        G = stat.G,
                        GS = stat.GS,
                        Outs = stat.Outs,
                        ERA = Utilities.SafeDivide(stat.ERA, confAvg.ERA),
                        H9 = Utilities.SafeDivide(stat.H9, confAvg.H9),
                        HR9 = Utilities.SafeDivide(stat.HR9, confAvg.HR9),
                        BB9 = Utilities.SafeDivide(stat.BB9, confAvg.BB9),
                        K9 = Utilities.SafeDivide(stat.K9, confAvg.K9),
                        WHIP = Utilities.SafeDivide(stat.WHIP, confAvg.WHIP),
                        Age = stat.Age,
                        Height = height,
                        Weight = stat.Weight,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_PitcherYear.AddRange(modelStats);
            db.SaveChanges();

            return true;
        }

        public static bool CreatePlayerGaps()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            // Hitter
            var hitters = db.Model_College_HitterYear.GroupBy(f => f.TBCId);
            foreach (var hitter in hitters)
            {
                var orderedStats = hitter.OrderBy(f => f.Year);
                int year = orderedStats.First().Year;
                int expYear = orderedStats.First().ExpYears;
                float age = orderedStats.First().Age;

                foreach (var stat in orderedStats)
                {
                    while (stat.Year - 1 > year)
                    {
                        year++;
                        expYear++;
                        age += 1;

                        // No COVID stats
                        if (year == 2020)
                        {
                            expYear--;
                            continue;
                        }

                        int height = stat.Height > 0 ? stat.Height : ColUtilities.DEFAULT_HEIGHT;

                        db.Model_College_HitterYear.Add(new Model_College_HitterYear
                        {
                            TBCId = stat.TBCId,
                            Level = stat.Level,
                            Year = year,
                            ExpYears = expYear,
                            ParkRunFactor = 1,
                            ConfScore = 1,
                            PA = 0,
                            H = 1,
                            H2B = 1,
                            H3B = 1,
                            HR = 1,
                            SB = 1,
                            CS = 1,
                            BB = 1,
                            K = 1,
                            HBP = 1,
                            AVG = 1,
                            OBP = 1,
                            SLG = 1,
                            OPS = 1,
                            Age = age,
                            Pos = stat.Pos,
                            Height = height,
                            Weight = stat.Weight,
                        });
                    }

                    year = stat.Year;
                    expYear = stat.ExpYears;
                    age = stat.Age;
                }
            }

            // Pitcher
            var pitchers = db.Model_College_PitcherYear.GroupBy(f => f.TBCId);
            foreach (var pitcher in pitchers)
            {
                var orderedStats = pitcher.OrderBy(f => f.Year);
                int year = orderedStats.First().Year;
                int expYear = orderedStats.First().ExpYears;
                float age = orderedStats.First().Age;

                foreach (var stat in orderedStats)
                {
                    while (stat.Year - 1 > year)
                    {
                        year++;
                        expYear++;
                        age += 1;

                        // No COVID stats
                        if (year == 2020)
                        {
                            expYear--;
                            continue;
                        }


                        int height = stat.Height > 0 ? stat.Height : ColUtilities.DEFAULT_HEIGHT;

                        db.Model_College_PitcherYear.Add(new Model_College_PitcherYear
                        {
                            TBCId = stat.TBCId,
                            Level = stat.Level,
                            Year = year,
                            ExpYears = expYear,
                            ParkRunFactor = 1,
                            ConfScore = 1,
                            G = 0,
                            GS = 0,
                            Outs = 0,
                            ERA = 1,
                            H9 = 1,
                            HR9 = 1,
                            BB9 = 1,
                            K9 = 1,
                            WHIP = 1,
                            Age = age,
                            Height = height,
                            Weight = stat.Weight,
                        });
                    }

                    year = stat.Year;
                    expYear = stat.ExpYears;
                    age = stat.Age;
                }
            }

            db.SaveChanges();

            return true;
        }

        
    }
}
