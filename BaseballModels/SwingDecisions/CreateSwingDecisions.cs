using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;
using SwingDecisionsDb;

namespace SwingDecisions
{
    internal class CreateSwingDecisions
    {
        private record RemKey(int LeagueId, int CountBalls, int CountStrikes, Db.DbEnums.PitchResult Result);
        public static void Update(int year, bool forceRefresh)
        {
            using SwingDecisionsDbContext swingDb = new(SwingDecisionsDb.Connection.DB_OPTIONS);
            if (forceRefresh)
            {
                swingDb.SwingDecision.Where(sd => sd.Year == year).ExecuteDelete();
            }
            else if (swingDb.SwingDecision.Any(sd => sd.Year == year))
            {
                return;
            }
            using SqliteDbContext db = new(Db.Connection.DB_READONLY_OPTIONS);
            using PitchDbContext pitchDb = new(PitchDb.Connection.PITCHDB_READONLY_OPTIONS);
            var remLookup = LoadRunExpectancyMatrix(db, year);
            List<int> gameIds = GetDistinctGameIds(pitchDb, year);

            using var transaction = swingDb.Database.BeginTransaction();

            const int BatchSize = 100;
            var batches = Batch(gameIds, BatchSize).ToList();

            using (ProgressBar progressBar = new(batches.Count, $"Building swing decisions for {year}"))
            {
                foreach (var gameIdBatch in batches)
                {
                    var opvaRecords = LoadOpvaForGames(pitchDb, gameIdBatch);
                    var psLookup = LoadPitchStatcastLookup(db, gameIdBatch);
                    var decisions = BuildSwingDecisions(opvaRecords, psLookup, remLookup);
                    SaveSwingDecisions(swingDb, decisions);
                    progressBar.Tick();
                }
            }

            transaction.Commit();
        }
        // -- Data access stubs --
        private static Dictionary<RemKey, float> LoadRunExpectancyMatrix(SqliteDbContext db, int year)
        {
            return db.RunExpectancyMatrix
                .Where(r => r.Year == year)
                .ToDictionary(
                    r => new RemKey(r.LeagueId, r.CountBalls, r.CountStrikes, r.Result),
                    r => r.DeltaRuns
                );
        }
        private static List<int> GetDistinctGameIds(PitchDbContext pitchDb, int year)
        {
            return pitchDb.Output_PitchValueAggregation
                .Where(o => o.Year == year && o.Model == 1)
                .Select(o => o.GameId)
                .Distinct()
                .ToList();
        }
        private static List<PitchDb.Output_PitchValueAggregation> LoadOpvaForGames(
            PitchDbContext pitchDb, List<int> gameIds)
        {
            return pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .Where(o => o.Model == 1 && gameIds.Contains(o.GameId))
                .ToList();
        }
        private static Dictionary<(int GameId, int PitchId), Db.PitchStatcast> LoadPitchStatcastLookup(
            SqliteDbContext db, List<int> gameIds)
        {
            return db.PitchStatcast
                .AsNoTracking()
                .Where(ps => gameIds.Contains(ps.GameId))
                .ToDictionary(ps => (ps.GameId, ps.PitchId));
        }
        private static void SaveSwingDecisions(
            SwingDecisionsDbContext swingDb, List<SwingDecisionsDb.SwingDecision> decisions)
        {
            swingDb.BulkInsert(decisions);
        }
        // -- Batch utility --
        private static IEnumerable<List<T>> Batch<T>(List<T> source, int batchSize)
        {
            for (int i = 0; i < source.Count; i += batchSize)
            {
                yield return source.GetRange(i, Math.Min(batchSize, source.Count - i));
            }
        }
        // -- Decision building --
        private static List<SwingDecisionsDb.SwingDecision> BuildSwingDecisions(
            List<PitchDb.Output_PitchValueAggregation> opvaRecords,
            Dictionary<(int GameId, int PitchId), Db.PitchStatcast> psLookup,
            Dictionary<RemKey, float> remLookup)
        {
            List<SwingDecisionsDb.SwingDecision> decisions = new(opvaRecords.Count);
            foreach (var opva in opvaRecords)
            {
                var ps = psLookup[(opva.GameId, opva.PitchId)];
                decisions.Add(CreateSwingDecision(opva, ps, remLookup));
            }
            return decisions;
        }
        private static SwingDecisionsDb.SwingDecision CreateSwingDecision(
            PitchDb.Output_PitchValueAggregation opva,
            Db.PitchStatcast ps,
            Dictionary<RemKey, float> remLookup)
        {
            float probSwing = opva.CombinedSwing;
            float valueSwing = ComputeValueSwing(opva, ps, remLookup);
            float valueNoSwing = ComputeValueNoSwing(opva, ps, remLookup);
            // When the model gives P(Swing) = 1, the no-swing branch is undefined.
            // Treat swing and no-swing as equal so the decision value is 0,
            // effectively punting on this edge case.
            if (float.IsNaN(valueNoSwing))
                valueNoSwing = valueSwing;
            bool didSwing = ps.HadSwing;
            float value = ComputeDecisionValue(didSwing, probSwing, valueSwing, valueNoSwing);
            return new SwingDecisionsDb.SwingDecision
            {
                GameId = opva.GameId,
                PitchId = opva.PitchId,
                Year = ps.Year,
                Month = ps.Month,
                HitterId = ps.HitterId,
                PitcherId = ps.PitcherId,
                LevelId = ps.LevelId,
                PitchType = ps.PitchType,
                CountBalls = ps.CountBalls,
                CountStrikes = ps.CountStrike,
                Outs = ps.Outs,
                BaseOccupancy = ps.BaseOccupancy,
                DidSwing = didSwing,
                ProbSwing = probSwing,
                ValueSwing = valueSwing,
                ValueNoSwing = valueNoSwing,
                Value = value,
            };
        }
        // -- Value computation stubs --
        private static float ComputeValueSwing(
            PitchDb.Output_PitchValueAggregation opva,
            Db.PitchStatcast ps,
            Dictionary<RemKey, float> remLookup)
        {
            float strikeValue = remLookup[new RemKey(
                ps.LeagueId, ps.CountBalls, ps.CountStrike, Db.DbEnums.PitchResult.CalledStrike)];
            // A foul with 2 strikes cannot change the count, so its run-value delta is 0.
            // With fewer than 2 strikes a foul acts as a strike.
            float foulValue = ps.CountStrike < 2 ? strikeValue : 0f;
            // Tier-2 probabilities (Whiff, Foul, InPlay) are conditional on swing and sum to 1.
            // InPlay uses the model's expected value directly rather than REM,
            // since CombinedInPlayExpected is already adjusted for the current count.
            return (opva.CombinedWhiff * strikeValue)
                 + (opva.CombinedFoul * foulValue)
                 + (opva.CombinedInPlay * opva.CombinedInPlayExpected);
        }
        private static float ComputeValueNoSwing(
            PitchDb.Output_PitchValueAggregation opva,
            Db.PitchStatcast ps,
            Dictionary<RemKey, float> remLookup)
        {
            float noSwingProb = 1f - opva.CombinedSwing;
            // When the model predicts a swing is certain, the conditional
            // no-swing probabilities are undefined. Return a NaN so the
            // caller can handle this gracefully.
            if (noSwingProb <= 0f)
                return float.NaN;
            float csValue = remLookup[new RemKey(
                ps.LeagueId, ps.CountBalls, ps.CountStrike, Db.DbEnums.PitchResult.CalledStrike)];
            float ballValue = remLookup[new RemKey(
                ps.LeagueId, ps.CountBalls, ps.CountStrike, Db.DbEnums.PitchResult.Ball)];
            float hbpValue = remLookup[new RemKey(
                ps.LeagueId, ps.CountBalls, ps.CountStrike, Db.DbEnums.PitchResult.HBP)];
            // Tier-1 probabilities (CalledStrike, Ball, HBP) are unconditional,
            // so dividing by (1 - P(Swing)) converts them to conditional on no swing.
            return ((opva.CombinedCalledStrike * csValue)
                  + (opva.CombinedBall * ballValue)
                  + (opva.CombinedHBP * hbpValue)) / noSwingProb;
        }
        private static float ComputeDecisionValue(
            bool didSwing, float probSwing, float valueSwing, float valueNoSwing)
        {
            // The expected value is the probability-weighted blend of the two branches.
            // The decision value is how much the actual choice beat (or trailed) that baseline.
            float expectedValue = (probSwing * valueSwing) + ((1f - probSwing) * valueNoSwing);
            return (didSwing ? valueSwing : valueNoSwing) - expectedValue;
        }
    }
}
