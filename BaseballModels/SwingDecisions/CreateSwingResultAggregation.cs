using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using SwingDecisionsDb;

namespace SwingDecisions
{
    internal class CreateSwingResultAggregation
    {
        public static void Update(int year, int? month, bool forceRefresh)
        {
            using SwingDecisionsDbContext swingDb = new(SwingDecisionsDb.Connection.DB_OPTIONS);
            int? mappedMonth = month is int m ? MapMonth(m) : null;

            // Check for existing data or clear stale records
            if (!forceRefresh)
            {
                bool dataExists = mappedMonth.HasValue
                    ? swingDb.SwingResultAggregation.Any(s => s.Year == year && s.Month == mappedMonth)
                    : swingDb.SwingResultAggregation.Any(s => s.Year == year && s.Month == null);
                if (dataExists)
                    return;
            }

            if (mappedMonth.HasValue)
            {
                swingDb.SwingResultAggregation
                    .Where(s => s.Year == year && (s.Month == mappedMonth || s.Month == null))
                    .ExecuteDelete();
            }
            else
            {
                swingDb.SwingResultAggregation
                    .Where(s => s.Year == year)
                    .ExecuteDelete();
            }

            // Load source data and determine which timeframes to aggregate
            var swingDecisions = swingDb.SwingDecision
                .AsNoTracking()
                .Where(sd => sd.Year == year)
                .ToList();

            var timeframes = GetTimeframes(swingDecisions, mappedMonth);

            // Group pitches by player and level for aggregation
            var byHitter = swingDecisions
                .GroupBy(sd => (sd.HitterId, sd.LevelId))
                .ToDictionary(g => g.Key, g => g.ToList());
            var byPitcher = swingDecisions
                .GroupBy(sd => (sd.PitcherId, sd.LevelId))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Aggregate each player across all timeframes and pitch groups
            var results = new List<SwingDecisionsDb.SwingResultAggregation>();

            string message = mappedMonth.HasValue
                ? $"Aggregating swing decisions for {year} month {mappedMonth}"
                : $"Aggregating swing decisions for {year}";

            using (ProgressBar progressBar = new(byHitter.Count + byPitcher.Count, message))
            {
                foreach (var ((hitterId, levelId), pitches) in byHitter)
                {
                    results.AddRange(AggregateForPlayer(hitterId, null, levelId, year, timeframes, pitches));
                    progressBar.Tick();
                }

                foreach (var ((pitcherId, levelId), pitches) in byPitcher)
                {
                    results.AddRange(AggregateForPlayer(null, pitcherId, levelId, year, timeframes, pitches));
                    progressBar.Tick();
                }
            }

            // Write all aggregations at once
            swingDb.BulkInsert(results);
        }

        private static int MapMonth(int month) => Math.Clamp(month, 4, 9);

        private static List<int?> GetTimeframes(
            List<SwingDecisionsDb.SwingDecision> swingDecisions,
            int? mappedMonth)
        {
            // Full year (null) is always included
            var timeframes = new List<int?> { null };

            if (mappedMonth.HasValue)
            {
                timeframes.Add(mappedMonth.Value);
            }
            else
            {
                foreach (int m in swingDecisions.Select(sd => MapMonth(sd.Month)).Distinct().OrderBy(m => m))
                    timeframes.Add(m);
            }

            return timeframes;
        }

        private static SwingDbEnums.PitchGroup GetPitchGroup(Db.DbEnums.PitchType pitchType)
        {
            return pitchType switch
            {
                Db.DbEnums.PitchType.Fourseam => SwingDbEnums.PitchGroup.Fastball,
                Db.DbEnums.PitchType.Sinker => SwingDbEnums.PitchGroup.Fastball,
                Db.DbEnums.PitchType.Fastball => SwingDbEnums.PitchGroup.Fastball,
                Db.DbEnums.PitchType.Twoseam => SwingDbEnums.PitchGroup.Fastball,

                Db.DbEnums.PitchType.Splitter => SwingDbEnums.PitchGroup.Changeup,
                Db.DbEnums.PitchType.Changeup => SwingDbEnums.PitchGroup.Changeup,
                Db.DbEnums.PitchType.Forkball => SwingDbEnums.PitchGroup.Changeup,

                Db.DbEnums.PitchType.Cutter => SwingDbEnums.PitchGroup.Breaking,
                Db.DbEnums.PitchType.Slider => SwingDbEnums.PitchGroup.Breaking,
                Db.DbEnums.PitchType.Sweeper => SwingDbEnums.PitchGroup.Breaking,
                Db.DbEnums.PitchType.Slurve => SwingDbEnums.PitchGroup.Breaking,
                Db.DbEnums.PitchType.Curveball => SwingDbEnums.PitchGroup.Breaking,
                Db.DbEnums.PitchType.KnuckleCurve => SwingDbEnums.PitchGroup.Breaking,
                Db.DbEnums.PitchType.Screwball => SwingDbEnums.PitchGroup.Breaking,
                Db.DbEnums.PitchType.SlowCurve => SwingDbEnums.PitchGroup.Breaking,

                // Unknown, Knuckleball, Eephus -- only counted in the All group
                _ => SwingDbEnums.PitchGroup.All,
            };
        }

        private static List<SwingDecisionsDb.SwingResultAggregation> AggregateForPlayer(
            int? hitterId, int? pitcherId, int levelId, int year,
            List<int?> timeframes,
            List<SwingDecisionsDb.SwingDecision> pitches)
        {
            var results = new List<SwingDecisionsDb.SwingResultAggregation>();

            foreach (var timeframe in timeframes)
            {
                // For full year, use all pitches; for a specific month, filter to that month
                var timePitches = timeframe.HasValue
                    ? pitches.Where(p => MapMonth(p.Month) == timeframe.Value).ToList()
                    : pitches;

                // Only create entries for timeframes where the player had at least 1 pitch
                if (timePitches.Count == 0)
                    continue;

                foreach (var pitchGroup in Enum.GetValues<SwingDbEnums.PitchGroup>())
                {
                    IEnumerable<SwingDecisionsDb.SwingDecision> groupPitches =
                        pitchGroup == SwingDbEnums.PitchGroup.All
                            ? timePitches
                            : timePitches.Where(p => GetPitchGroup(p.PitchType) == pitchGroup);

                    results.Add(CreateAggregation(
                        hitterId, pitcherId, levelId, year, timeframe, pitchGroup, groupPitches));
                }
            }

            return results;
        }

        private static SwingDecisionsDb.SwingResultAggregation CreateAggregation(
            int? hitterId, int? pitcherId, int levelId, int year, int? month,
            SwingDbEnums.PitchGroup pitchGroup,
            IEnumerable<SwingDecisionsDb.SwingDecision> pitches)
        {
            // Single pass since pitches may be a deferred LINQ chain
            int numSwings = 0;
            float valueSwings = 0f;
            int numNonSwings = 0;
            float valueNonSwings = 0f;

            foreach (var pitch in pitches)
            {
                if (pitch.DidSwing)
                {
                    numSwings++;
                    valueSwings += pitch.Value;
                }
                else
                {
                    numNonSwings++;
                    valueNonSwings += pitch.Value;
                }
            }

            return new SwingDecisionsDb.SwingResultAggregation
            {
                HitterId = hitterId,
                PitcherId = pitcherId,
                LevelId = levelId,
                Year = year,
                Month = month,
                PitchGroup = pitchGroup,
                NumSwings = numSwings,
                ValueSwings = valueSwings,
                ValuePer100Swings = numSwings > 0 ? (valueSwings / numSwings) * 100f : 0f,
                NumNonSwings = numNonSwings,
                ValueNonSwings = valueNonSwings,
                ValuePer100NonSwings = numNonSwings > 0 ? (valueNonSwings / numNonSwings) * 100f : 0f,
            };
        }
    }
}
