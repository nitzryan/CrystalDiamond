using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition
{
    internal class PitchHitterZones
    {
        public static void Update(int year, bool forceRefresh)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            if (!forceRefresh && db.HitterYearZoneData.Any(f => f.Year == year))
                return;

            if (forceRefresh)
                db.HitterYearZoneData
                    .Where(f => f.Year == year)
                    .ExecuteDelete();

            #pragma warning disable CS8629 // Filtered out
            var pitchStatcastHitters = db.PitchStatcast
                .Where(f => f.Year == year && f.ZoneTop != null && f.ZoneBot != null)
                .Select(f => new { f.HitterId, f.HadSwing, ZoneTop=f.ZoneTop.Value, ZoneBot=f.ZoneBot.Value })
                .GroupBy(f => f.HitterId);
            #pragma warning restore CS8629

            int count = pitchStatcastHitters.Count();
            List<HitterYearZoneData> dbData = new(count);
            using (ProgressBar progressBar = new(count, $"Generating Hitter Zone Data for {year}"))
            {
                foreach (var hitter in pitchStatcastHitters)
                {
                    var notSwingingPitches = hitter.Where(f => !f.HadSwing);
                    
                    // Log a swing if no non-swinging pitches exist
                    if (!notSwingingPitches.Any())
                    {
                        dbData.Add(new HitterYearZoneData
                        {
                            MlbId = hitter.Key,
                            Year = year,
                            ZoneTop = hitter.First().ZoneTop,
                            ZoneBot = hitter.First().ZoneBot
                        });
                    }
                    else // Take average of non-swinging pitches
                    {
                        dbData.Add(new HitterYearZoneData
                        {
                            MlbId = hitter.Key,
                            Year = year,
                            ZoneTop = hitter.Average(f => f.ZoneTop),
                            ZoneBot = hitter.Average(f => f.ZoneBot)
                        });
                    }

                    progressBar.Tick();
                }
            }

            db.BulkInsert(dbData);
        }
    }
}
