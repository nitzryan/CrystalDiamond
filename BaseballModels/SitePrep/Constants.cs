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
    }
}
