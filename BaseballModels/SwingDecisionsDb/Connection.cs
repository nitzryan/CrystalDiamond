using Microsoft.EntityFrameworkCore;

namespace SwingDecisionsDb
{
    public class Connection
    {
        public static readonly DbContextOptions<SwingDecisionsDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SwingDecisionsDbContext>()
                        .UseSqlite("Data Source=../../../../SwingDecisionsDb/SwingDecisions.db;")
                        .EnableSensitiveDataLogging()
                        .Options;

        public static readonly DbContextOptions<SwingDecisionsDbContext> DB_READONLY_OPTIONS = new DbContextOptionsBuilder<SwingDecisionsDbContext>()
                    .UseSqlite("Data Source=../../../../SwingDecisionsDb/SwingDecisions.db;Mode=ReadOnly")
                    .Options;
    }
}
