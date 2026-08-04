using Microsoft.EntityFrameworkCore;

namespace PitchTrackingDb
{
    public class Connection
    {
        public static readonly DbContextOptions<PitchTrackingDbContext> PITCHTRACK_DB_OPTIONS = new DbContextOptionsBuilder<PitchTrackingDbContext>()
                        .UseSqlite("Data Source=../../../../PitchTrackingDb/PitchTracking.db;")
                        .EnableSensitiveDataLogging()
                        .Options;

        public static readonly DbContextOptions<PitchTrackingDbContext> PITCHTRACK_DB_READONLY_OPTIONS = new DbContextOptionsBuilder<PitchTrackingDbContext>()
                    .UseSqlite("Data Source=../../../../PitchTrackingDb/PitchTracking.db;Mode=ReadOnly")
                    .Options;
    }
}
