using Db;
using ShellProgressBar;
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

        public static void Update(int year, bool forceRefresh)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            // Check if there are pitches with run values, don't redo if they exist
            var pitches = db.PitchStatcast.Where(f => f.Year == year);
            if (!forceRefresh && pitches.Any(f => f.RunValueHitter > -100))
                return;

            var leaguePitches = pitches.GroupBy(f => f.LeagueId);
            using (ProgressBar progressBar = new(leaguePitches.Count(), $"Generating Statcast Pitch Values for {year}"))
            {
                foreach (var lp in leaguePitches)
                {
                    (var runExpectancyMatrix, var pitchRunExpectancyMatrix) = GetRunExpectancyMatrices(lp);

                    using (ChildProgressBar topChild = progressBar.Spawn(lp.Count(), $"Getting Pitch Run Values for {lp.Key}"))
                    {
                        foreach (var pitch in lp)
                        {
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
