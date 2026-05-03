using Microsoft.EntityFrameworkCore;

namespace PitchDb
{
    public class Connection
    {
        public static readonly DbContextOptions<PitchDbContext> PITCHDB_READONLY_OPTIONS = new DbContextOptionsBuilder<PitchDbContext>()
                .UseSqlite("Data Source=../../../../PitchDb/Pitch.db;Mode=ReadOnly")
                .Options;
    }
}
