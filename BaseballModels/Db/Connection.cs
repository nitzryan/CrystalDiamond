using Microsoft.EntityFrameworkCore;

namespace Db
{
    public class Connection
    {
        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                    .UseSqlite("Data Source=../../../../Db/BaseballStats.db;")
                    .EnableSensitiveDataLogging()
                    .Options;

        public static readonly DbContextOptions<SqliteDbContext> DB_READONLY_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                    .UseSqlite("Data Source=../../../../Db/BaseballStats.db;Mode=ReadOnly")
                    .Options;
    }
}
