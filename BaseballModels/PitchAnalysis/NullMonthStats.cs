using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class NullMonthStats
    {
        public static void Update()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.PitcherStatcastMonth.Where(f => !f.IsValid).ExecuteDelete();

            var modelPitcherStats = db.Model_PitcherStats
                .Select(f => new { f.MlbId, f.Year, f.Month });
            int count = modelPitcherStats.Count();
            List<PitcherStatcastMonth> dbData = new(count);
            using (ProgressBar progressBar = new(count, $"Generating Null PitchStatcastMonth stats"))
            {
                foreach (var mps in modelPitcherStats)
                {
                    progressBar.Tick();

                    if (db.PitcherStatcastMonth.Where(f => f.MlbId == mps.MlbId && f.Month == mps.Month && f.Year == mps.Year).Any())
                        continue;

                    dbData.Add(new PitcherStatcastMonth
                    {
                        MlbId = mps.MlbId,
                        Year = mps.Year,
                        Month = mps.Month,
                        IsValid = false,
                        Stuff = 95,
                        Pitch = 95,
                        Actual = 95,
                        NumPitches = -1,
                        StuffFastball = 95,
                        PitchFastball = 95,
                        ActFastball = 95,
                        NumFastballs = -1,
                        StuffBreaking = 95,
                        PitchBreaking = 95,
                        ActBreaking = 95,
                        NumBreaking = -1,
                        StuffChangeup = 95,
                        PitchChangeup = 95,
                        ActChangeup = 95,
                        NumChangeup = -1,
                    });
                }
            }

            db.BulkInsert(dbData);
        }
    }
}
