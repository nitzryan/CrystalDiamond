using Microsoft.EntityFrameworkCore;

namespace ModelDb
{
    public class Connection
    {
        public static readonly DbContextOptions<ModelDbContext> MODELDB_READONLY_OPTIONS = new DbContextOptionsBuilder<ModelDbContext>()
                .UseSqlite("Data Source=../../../../ModelDb/Model.db;Mode=ReadOnly;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;
    }
}
