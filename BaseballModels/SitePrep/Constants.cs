using Db;
using SiteDb;
using Microsoft.EntityFrameworkCore;

namespace SitePrep
{
    internal class Constants
    {
        public static readonly List<float> TOTAL_WAR_BUCKETS = [0.0f, 0.5f, 3.0f, 7.5f, 15.0f, 25.0f, 35.0f];

        public const string SITE_ASSET_FOLDER = "../../../../Site/server/assets/";
        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

        public static readonly DbContextOptions<SqliteDbContext> DB_WRITE_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
        .UseSqlite("Data Source=../../../../Db/BaseballStats.db;")
        .Options;

        public static readonly DbContextOptions<SiteDbContext> SITEDB_OPTIONS = new DbContextOptionsBuilder<SiteDbContext>()
        .UseSqlite("Data Source=../../../../SiteDb/Site.db;")
        .EnableSensitiveDataLogging()
        .Options;

        public static List<int> ModelLevelToMlbLevel = [1, 11, 12, 13, 14, 15, 16, 17];

        // Positional adjustment
        //https://tht.fangraphs.com/re-examining-wars-defensive-spectrum/
        public const float POSITIONAL_ADJUSTMENT_C = 7.75f;
        public const float POSITIONAL_ADJUSTMENT_1B = -9.25f;
        public const float POSITIONAL_ADJUSTMENT_2B = 1.75f;
        public const float POSITIONAL_ADJUSTMENT_3B = 1.75f;
        public const float POSITIONAL_ADJUSTMENT_SS = 4.75f;
        public const float POSITIONAL_ADJUSTMENT_LF = -4.25f;
        public const float POSITIONAL_ADJUSTMENT_CF = 1.75f;
        public const float POSITIONAL_ADJUSTMENT_RF = -4.25f;
        public const float POSITIONAL_ADJUSTMENT_DH = -9.25f;

        // WAR Calculation Constants
        public const float REPLACEMENT_LEVEL_WIN_PERCENTAGE = 1000.0f / 2430;
        public const float HITTER_WAR_PERCENTAGE = 0.57f;
        public const float PITCHER_WAR_PERCENTAGE = 1.0f - HITTER_WAR_PERCENTAGE;
    }
}
