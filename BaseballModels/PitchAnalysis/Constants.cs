using Db;
using Microsoft.EntityFrameworkCore;
using PitchDb;

namespace PitchAnalysis
{
    internal class Constants
    {
        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                    .UseSqlite("Data Source=../../../../Db/BaseballStats.db;")
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .Options;

        public static readonly DbContextOptions<PitchDbContext> PITCHDB_OPTIONS = new DbContextOptionsBuilder<PitchDbContext>()
                .UseSqlite("Data Source=../../../../PitchDb/Pitch.db;")
                .EnableSensitiveDataLogging()
                .Options;
    }
}
