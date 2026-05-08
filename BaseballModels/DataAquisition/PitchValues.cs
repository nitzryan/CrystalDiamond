using Db;
using ShellProgressBar;
using SQLitePCL;
using static Db.DbEnums;

namespace DataAquisition
{
    internal class PitchValues
    {
        private record struct GamePitchSituation(
            int outs,
            BaseOccupancy baseOccupancy,
            int balls,
            int strikes
        );

        private record struct GameSituation(
            int Outs, 
            BaseOccupancy BaseOccupancy
        );

        private enum ExitVeloBucket
        {
            Unknown,
            _90,
            _90_95,
            _95_100,
            _100_105,
            _105
        };

        private enum LaunchAngleBucket
        {
            Unknown,
            _10,
            _10_20,
            _20_30,
            _30_40,
            _40
        }

        private enum PitchBallStrike
        { 
            Strike,
            Ball,
        }


        private static readonly List<(float, float, ExitVeloBucket)> _exitVeloBuckets = new()
        {
            (0, 90, ExitVeloBucket._90),
            (90, 95, ExitVeloBucket._90_95),
            (95, 100, ExitVeloBucket._95_100),
            (100, 105, ExitVeloBucket._100_105),
            (105, 9999, ExitVeloBucket._105)
        };

        private static readonly List<(float, float, LaunchAngleBucket)> _launchAngleBuckets = new()
        {
            (-10000, 10, LaunchAngleBucket._10),
            (10, 20, LaunchAngleBucket._10_20),
            (20, 30, LaunchAngleBucket._20_30),
            (30, 40, LaunchAngleBucket._30_40),
            (40, 100000, LaunchAngleBucket._40)
        };

        private static ExitVeloBucket GetExitVeloBucket(float launchSpeed)
        {
            foreach (var bucket in _exitVeloBuckets)
            {
                if (launchSpeed >= bucket.Item1 && launchSpeed < bucket.Item2)
                {
                    return bucket.Item3;
                }
            }

            throw new ArgumentOutOfRangeException(
                nameof(launchSpeed),
                $"Launch speed {launchSpeed} does not fall into any defined ExitVeloBucket.");
        }

        private static LaunchAngleBucket GetLaunchAngleBucket(float launchAngle)
        {
            foreach (var bucket in _launchAngleBuckets)
            {
                if (launchAngle >= bucket.Item1 && launchAngle < bucket.Item2)
                {
                    return bucket.Item3;
                }
            }

            throw new ArgumentOutOfRangeException(
                nameof(launchAngle),
                $"Launch angle {launchAngle} does not fall into any defined LaunchAngleBucket.");
        }
        private static (ExitVeloBucket ExitVelo, LaunchAngleBucket LaunchAngle) GetBuckets(PitchStatcast pitch)
        {
            if (pitch.LaunchSpeed == null || pitch.LaunchAngle == null)
            {
                return (ExitVeloBucket.Unknown, LaunchAngleBucket.Unknown);
            }

            var evBucket = GetExitVeloBucket(pitch.LaunchSpeed.Value);
            var laBucket = GetLaunchAngleBucket(pitch.LaunchAngle.Value);

            return (evBucket, laBucket);
        }

        private static (Dictionary<GameSituation, float>, Dictionary<GamePitchSituation, float>) GetRunExpectancyMatrices(IEnumerable<PitchStatcast> pitches)
        {
            // Get runs from situations from empty count
            var startPaPitches = pitches.Where(f => f.CountBalls == 0 && f.CountStrike == 0);
            Dictionary<GameSituation, float> runExpectancyMatrix = new();
            foreach (var situation in startPaPitches.GroupBy(f => new GameSituation (f.Outs, f.BaseOccupancy)))
            {
                int numOpportunities = situation.Count();
                int numRuns = situation.Sum(f => f.PaResultDirectRuns + f.RunsAfterPa);

                float expectedRuns = (float)(numRuns) / numOpportunities;
                runExpectancyMatrix[situation.Key] = expectedRuns;
            }
            runExpectancyMatrix[new GameSituation(3, BaseOccupancy.Empty)] = 0;

            // Get runs for each out/basepath/count
            Dictionary<GamePitchSituation, float> pitchRunExpectancy = new();

            var pitchGroupings = pitches.GroupBy(f => new GamePitchSituation(f.Outs, f.BaseOccupancy, f.CountBalls, f.CountStrike));
            foreach (var situation in pitchGroupings)
            {
                // Get total Runs scored
                int numOpportunities = situation.Count();
                int numRuns = situation.Sum(f => f.PaResultDirectRuns);

                // Get expected runs after PA
                float expectedRunsAfter = 0;
                foreach (var pitch in situation)
                {
                    int outs = pitch.Outs + pitch.PaResultOuts;
                    BaseOccupancy occupancy = pitch.PaResultOccupancy;
                    if (outs >= 3)
                    {
                        occupancy = BaseOccupancy.Empty;
                        outs = 3;
                    }
                        
                    GameSituation gs = new GameSituation(outs, occupancy);

                    expectedRunsAfter += runExpectancyMatrix[gs];
                }

                pitchRunExpectancy[situation.Key] = (numRuns + expectedRunsAfter) / numOpportunities;
            }

            return (runExpectancyMatrix, pitchRunExpectancy);
        }

        private static Dictionary<(ExitVeloBucket, LaunchAngleBucket), float> GetBallInPlayRunExpectancyMatrix(IEnumerable<PitchStatcast> pitches)
        {
            Dictionary<(ExitVeloBucket, LaunchAngleBucket), float> runDict = new();
            pitches = pitches.Where(f => f.Result == PitchResult.InPlay);

            var unknownPitches = pitches.Where(f => f.LaunchSpeed == null || f.LaunchAngle == null);
            runDict[(ExitVeloBucket.Unknown, LaunchAngleBucket.Unknown)] = unknownPitches.Select(f => f.RunValueHitter).Average();

            pitches = pitches.Where(f => f.LaunchAngle != null && f.LaunchSpeed != null);
            foreach (var evBucket in _exitVeloBuckets)
            {
                foreach (var laBucket in _launchAngleBuckets)
                {
                    var bucketPitches = pitches.Where(f =>
                        f.LaunchAngle != null &&
                        f.LaunchSpeed != null &&
                        GetExitVeloBucket(f.LaunchSpeed.Value) == evBucket.Item3 &&
                        GetLaunchAngleBucket(f.LaunchAngle.Value) == laBucket.Item3);
                    runDict[(evBucket.Item3, laBucket.Item3)] = bucketPitches.Select(f => f.RunValueHitter).Average();
                }
            }

            return runDict;
        }

        private static Dictionary<(int, int, PitchBallStrike), float> GetCountRunExpectancyMatrix(IEnumerable<PitchStatcast> pitches)
        {
            Dictionary<(int, int, PitchBallStrike), float> runDict = new();
            pitches = pitches.Where(f => f.Result != PitchResult.InPlay && f.Result != PitchResult.HBP);

            List<int> balls = [0, 1, 2, 3];
            List<int> strikes = [0, 1, 2];
            foreach (var ball in balls)
                foreach (var strike in strikes)
                {
                    var strikePitches = strike == 2 ?
                        pitches.Where(f => f.CountBalls == ball && f.CountStrike == strike &&
                            (f.Result == PitchResult.CalledStrike || f.Result == PitchResult.SwingingStrike))
                            :
                        pitches.Where(f => f.CountBalls == ball && f.CountStrike == strike &&
                            (f.Result == PitchResult.CalledStrike || f.Result == PitchResult.SwingingStrike || f.Result == PitchResult.Foul));

                    var ballPitches = pitches.Where(f => f.CountBalls == ball && f.CountStrike == strike && f.Result == PitchResult.Ball);

                    runDict[(ball, strike, PitchBallStrike.Strike)] = strikePitches.Select(f => f.RunValueHitter).Average();
                    runDict[(ball, strike, PitchBallStrike.Ball)] = ballPitches.Select(f => f.RunValueHitter).Average();
                }

            return runDict;
        }

        private static Dictionary<(int, int), float> GetCountBallInPlayExpectancyMatrix(IEnumerable<PitchStatcast> pitches, Dictionary<(ExitVeloBucket, LaunchAngleBucket), float> bipMatrix)
        {
            Dictionary<(int, int), float> runDict = new();
            pitches = pitches.Where(f => f.Result == PitchResult.InPlay && f.LaunchAngle != null && f.LaunchSpeed != null);

            List<int> balls = [0, 1, 2, 3];
            List<int> strikes = [0, 1, 2];
            foreach (int ball in balls)
                foreach (int strike in strikes)
                {
                    var countPitches = pitches.Where(f => f.CountBalls == ball && f.CountStrike == strike);
                    float avgValue = 0;
                    foreach (var pitch in countPitches)
                    {
                        var (evBucket, laBucket) = GetBuckets(pitch);
                        avgValue += bipMatrix[(evBucket, laBucket)];
                    }

                    runDict[(ball, strike)] = avgValue / countPitches.Count();
                }

            return runDict;
        }

        public static float GetHBPValue(IEnumerable<PitchStatcast> pitches)
        {
            return pitches.Where(f => f.Result == PitchResult.HBP).Select(f => f.RunValueHitter).Average();
        }

        public static void Update(int year, bool forceRefresh)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            // Check if there are pitches with run values, don't redo if they exist
            var pitches = db.PitchStatcast.Where(f => f.Year == year);
            if (!forceRefresh && pitches.Any(f => f.RunValueHitter > -100 & f.RunValueSmoothedHitter > -100))
                return;

            var leaguePitches = pitches.GroupBy(f => f.LeagueId);
            using (ProgressBar progressBar = new(leaguePitches.Count(), $"Generating Statcast Pitch Values for {year}"))
            {
                foreach (var lp in leaguePitches)
                {
                    (var runExpectancyMatrix, var pitchRunExpectancyMatrix) = GetRunExpectancyMatrices(lp);

                    var countBallStrikeRunExpectancyMatrix = GetCountRunExpectancyMatrix(lp);
                    var bipRunExpectancyMatrix = GetBallInPlayRunExpectancyMatrix(lp);
                    var countBIPRunExpectancyMatrix = GetCountBallInPlayExpectancyMatrix(lp, bipRunExpectancyMatrix);
                    float hbpValue = GetHBPValue(lp);

                    using (ChildProgressBar topChild = progressBar.Spawn(lp.Count(), $"Getting Pitch Run Values for {lp.Key}"))
                    {
                        foreach (var pitch in lp)
                        {
                            // Actual Run Values
                            GamePitchSituation gps = new GamePitchSituation(pitch.Outs, pitch.BaseOccupancy, pitch.CountBalls, pitch.CountStrike);
                            float startRunExpectancy = pitchRunExpectancyMatrix[gps];
                            PitchResult pr = pitch.Result;
                            float endRunOccupancy;

                            // Determine if ball is in play, or if it continues the count
                            if (pr == PitchResult.InPlay ||
                                pr == PitchResult.HBP ||
                                ((pr == PitchResult.CalledStrike || pr == PitchResult.SwingingStrike) && pitch.CountStrike == 2) ||
                                (pr == PitchResult.Ball && pitch.CountBalls == 3))
                            {
                                // At bat ended, get end results
                                endRunOccupancy = pitch.PaResultDirectRuns;
                                GameSituation endSituation = new GameSituation(pitch.Outs + pitch.PaResultOuts, pitch.PaResultOccupancy);
                                if (endSituation.Outs >= 3)
                                {
                                    endSituation.BaseOccupancy = BaseOccupancy.Empty;
                                    endSituation.Outs = 3;
                                }
                                    

                                endRunOccupancy += runExpectancyMatrix[endSituation];
                            }
                            else
                            {
                                int strikes = pitch.CountStrike;
                                int balls = pitch.CountBalls;

                                if (pr == PitchResult.SwingingStrike || pr == PitchResult.CalledStrike || pr == PitchResult.Foul)
                                {
                                    if (strikes < 2)
                                        strikes++;
                                }
                                else
                                {
                                    if (balls < 3)
                                        balls++;
                                }
                                GamePitchSituation next = new GamePitchSituation(pitch.Outs, pitch.BaseOccupancy, balls, strikes);
                                endRunOccupancy = pitchRunExpectancyMatrix[next];
                            }

                            pitch.RunValueHitter = endRunOccupancy - startRunExpectancy;

                            // Smoothed run values
                            if (pitch.CountStrike == 2 && pitch.Result == PitchResult.Foul)
                                pitch.RunValueSmoothedHitter = 0;
                            else if (pitch.Result == PitchResult.HBP)
                                pitch.RunValueSmoothedHitter = hbpValue;
                            else if (pitch.Result == PitchResult.InPlay)
                            {
                                (var exitVeloBucket, var launchAngleBucket) = GetBuckets(pitch);
                                float bipExpectedRuns = bipRunExpectancyMatrix[(exitVeloBucket, launchAngleBucket)];
                                float countBipExpectedRuns = countBIPRunExpectancyMatrix[(pitch.CountBalls, pitch.CountStrike)];

                                pitch.RunValueSmoothedHitter = bipExpectedRuns - countBipExpectedRuns;
                            }
                            else
                            {
                                PitchBallStrike pbs = pitch.Result == PitchResult.Ball ? PitchBallStrike.Ball : PitchBallStrike.Strike;
                                pitch.RunValueSmoothedHitter = countBallStrikeRunExpectancyMatrix[(pitch.CountBalls, pitch.CountStrike, pbs)];
                            }

                            topChild.Tick();
                        }
                    }

                    progressBar.Tick();
                }
            }

            db.SaveChanges();
        }
    }
}
