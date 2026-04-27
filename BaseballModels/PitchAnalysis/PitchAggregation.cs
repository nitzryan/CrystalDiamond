using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class PitchAggregation
    {
        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);

            pitchDb.Output_PitchValueAggregation.ExecuteDelete();

            List<Output_PitchValueAggregation> pitchList = new();

            var pitchOutputs = pitchDb.Output_PitchValue
                .GroupBy(f => new { f.PitchId, f.GameId, f.Model });
            int count = pitchOutputs.Count();
            pitchList.Capacity = count;

            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Pitch Model Results"))
            {
                foreach (var pitch in pitchOutputs)
                {
                    int size = pitch.Count();

                    Output_PitchValueAggregation opva = new()
                    {
                        Model = pitch.Key.Model,
                        GameId = pitch.Key.GameId,
                        PitchId = pitch.Key.PitchId,
                        AbsValue = 0,
                        StuffOnly = 0,
                        LocationOnly = 0,
                        Combined = 0,
                    };

                    foreach (var p in pitch)
                    {
                        opva.AbsValue += p.AbsValue / size;
                        opva.LocationOnly += p.LocationOnly / size;
                        opva.StuffOnly += p.StuffOnly / size;
                        opva.Combined += p.Combined / size;
                    }

                    pitchList.Add(opva);
                    progressBar.Tick();
                }
            }

            pitchDb.BulkInsert(pitchList);
        }
    }
}
