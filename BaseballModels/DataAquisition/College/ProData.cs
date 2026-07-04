using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using static Db.DbEnums;

namespace DataAquisition.College
{
    internal class ProData
    {
        private static readonly int PRO_YEARS_FOR_STATS = 8;

        public static void CreateHittersData(int currentYear)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.Model_College_HitterProStats.ExecuteDelete();

            var hitters = db.College_Player.Where(f => f.IsHitter);
            int hitterCount = hitters.Count();
            List<Model_College_HitterProStats> proStats = new();
            proStats.Capacity = hitterCount;
            using (ProgressBar progressBar = new(hitterCount, $"Calculating College Hitter Pro Stats"))
            {
                foreach (var hitter in hitters)
                {
                    // Get Fielding stats at each position
                    var fieldingStats = db.Player_Fielder_YearStats.Where(f => f.MlbId == hitter.MlbId
                        && f.Year <= hitter.LastYear + PRO_YEARS_FOR_STATS);

                    int outsC = 0;
                    int outs1B = 0;
                    int outs2B = 0;
                    int outs3B = 0;
                    int outsSS = 0;
                    int outsLF = 0;
                    int outsCF = 0;
                    int outsRF = 0;
                    int outsDH = 0;
                    int defOuts = 0;
                    float defRuns = 0;

                    float mlbRuns = 0;
                    int mlbOuts = 0;

                    foreach (var fs in fieldingStats)
                    {
                        int scale = ColUtilities.GetLevelScale(fs.LevelId);

                        int outs = scale * fs.Outs;

                        switch (fs.Position)
                        {
                            case Position.C:
                                outsC += outs;
                                break;
                            case Position.B1:
                                outs1B += outs;
                                break;
                            case Position.B2:
                                outs2B += outs;
                                break;
                            case Position.B3:
                                outs3B += outs;
                                break;
                            case Position.SS:
                                outsSS += outs;
                                break;
                            case Position.LF:
                                outsLF += outs;
                                break;
                            case Position.CF:
                                outsCF += outs;
                                break;
                            case Position.RF:
                                outsRF += outs;
                                break;
                            case Position.DH:
                                outsDH += outs;
                                break;
                            default:
                                break;
                        }

                        defOuts += fs.Outs;
                        defRuns += scale * fs.ScaledDRAA;

                        if (fs.LevelId == 1)
                        {
                            mlbRuns += fs.ScaledDRAA;
                            mlbOuts += fs.Outs;
                        }
                    }

                    int totalOuts = outsC + outs1B + outs2B + outs3B + outsSS + outsLF + outsCF + outsRF + outsDH;

                    Model_Players? mp = db.Model_Players.Where(f => f.MlbId == hitter.MlbId).SingleOrDefault();

                    int mlbPA = 0;
                    float mlbWar = 0;
                    int signingYear = currentYear + 1;
                    float mlbOff = 0;
                    if (mp != null)
                    {
                        mlbPA = mp.TotalPA;
                        mlbWar = mp.WarHitter;
                        signingYear = mp.SigningYear;
                        mlbOff = mp.RateOff * mp.TotalPA;
                    }



                    const float DEFAULT_OUTS = 1.0f / 9.0f;
                    proStats.Add(new Model_College_HitterProStats
                    {
                        TBCId = hitter.TBCId,
                        PercC = Utilities.SafeDivide(outsC, totalOuts, DEFAULT_OUTS),
                        Perc1B = Utilities.SafeDivide(outs1B, totalOuts, DEFAULT_OUTS),
                        Perc2B = Utilities.SafeDivide(outs2B, totalOuts, DEFAULT_OUTS),
                        Perc3B = Utilities.SafeDivide(outs3B, totalOuts, DEFAULT_OUTS),
                        PercSS = Utilities.SafeDivide(outsSS, totalOuts, DEFAULT_OUTS),
                        PercLF = Utilities.SafeDivide(outsLF, totalOuts, DEFAULT_OUTS),
                        PercCF = Utilities.SafeDivide(outsCF, totalOuts, DEFAULT_OUTS),
                        PercRF = Utilities.SafeDivide(outsRF, totalOuts, DEFAULT_OUTS),
                        PercDH = Utilities.SafeDivide(outsDH, totalOuts, DEFAULT_OUTS),
                        DEF = defRuns,
                        MLB_WAR = mlbWar,
                        DefOuts = defOuts,
                        MLB_PA = mlbPA,
                        MLB_DefPer1000IN = Utilities.SafeDivide(mlbRuns * 3000, mlbOuts, 0),
                        MLB_OFFPer600PA = Utilities.SafeDivide(mlbOff * 600, mlbPA, 0),
                        MLB_DefOuts = mlbOuts,
                        YearsSinceDraft = currentYear - signingYear,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_HitterProStats.AddRange(proStats);
            db.SaveChanges();
        }

        public static void CreatePitchersData(int currentYear)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.Model_College_PitcherProStats.ExecuteDelete();

            var pitchers = db.College_Player.Where(f => f.IsPitcher);
            int pitcherCount = pitchers.Count();
            List<Model_College_PitcherProStats> proStats = new();
            proStats.Capacity = pitcherCount;
            using (ProgressBar progressBar = new(pitcherCount, $"Calculating College Pitcher Pro Stats"))
            {
                foreach (var pitcher in pitchers)
                {
                    // Get Starting/relieving stats
                    var pitchingStats = db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == pitcher.MlbId
                        && f.MlbId != 0
                        && f.Year <= pitcher.LastYear + PRO_YEARS_FOR_STATS);

                    float outsSP = 0;
                    float outsRP = 0;

                    foreach (var ps in pitchingStats)
                    {
                        int scale = ColUtilities.GetLevelScale(ps.LevelId);

                        int outs = scale * ps.Outs;
                        outsSP += outs * ps.SPPerc;
                        outsRP += outs * (1 - ps.SPPerc);
                    }

                    float totalOuts = outsSP + outsRP;

                    Model_Players? mp = db.Model_Players.Where(f => f.MlbId == pitcher.MlbId).SingleOrDefault();

                    int mlbOuts = 0;
                    float mlbWar = 0;
                    int signingYear = currentYear + 1;
                    if (mp != null)
                    {
                        mlbOuts = mp.TotalOuts;
                        mlbWar = mp.WarPitcher;
                        signingYear = mp.SigningYear;
                    }

                    proStats.Add(new Model_College_PitcherProStats
                    {
                        TBCId = pitcher.TBCId,
                        PercSP = Utilities.SafeDivide(outsSP, totalOuts, 0.5f),
                        PercRP = Utilities.SafeDivide(outsRP, totalOuts, 0.5f),
                        MLB_WAR = mlbWar,
                        Outs = (int)totalOuts,
                        MLB_Outs = mlbOuts,
                        YearsSinceDraft = currentYear - signingYear,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_PitcherProStats.AddRange(proStats);
            db.SaveChanges();
        }
    }
}
