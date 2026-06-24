using Db;
using Microsoft.EntityFrameworkCore;
using ModelDb;
using PitchDb;
using SwingDecisionsDb;

namespace ModelEvaluation
{
    internal class Constants
    {
        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db;Mode=ReadOnly;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

        public static readonly DbContextOptions<PitchDbContext> PITCHDB_OPTIONS = new DbContextOptionsBuilder<PitchDbContext>()
                .UseSqlite("Data Source=../../../../PitchDb/Pitch.db;Mode=ReadOnly;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

        public static readonly DbContextOptions<SwingDecisionsDbContext> SWINGDB_OPTIONS = new DbContextOptionsBuilder<SwingDecisionsDbContext>()
                .UseSqlite("Data Source=../../../../SwingDecisionsDb/SwingDecisions.db;Mode=ReadOnly;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

        public static readonly DbContextOptions<ModelDbContext> MODELDB_OPTIONS = new DbContextOptionsBuilder<ModelDbContext>()
                .UseSqlite("Data Source=../../../../ModelDb/Model.db;Mode=ReadOnly;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

        public static readonly List<float> BUCKET_CUTOFFS = [0, 1, 5, 10, 20, 30, float.PositiveInfinity];
        public static int GetBucket(float war)
        {
            for (int i = 0; i < BUCKET_CUTOFFS.Count; i++)
                if (war <= BUCKET_CUTOFFS[i])
                    return i;

            throw new Exception("GetBucket failed to bucketize WAR");
        }
    }
}
