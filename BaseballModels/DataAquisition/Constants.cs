using Db;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DataAquisition
{
    internal class Constants
    {
        public const int START_YEAR = 2005;
        public static ReadOnlyCollection<int> SPORT_IDS = new([1, 11, 12, 13, 14, 15, 16, 17]);
        public static ReadOnlyCollection<string> SPORT_ID_NAMES = new(["MLB", "AAA", "AA", "A+", "A", "A-", "Rk", "DSL"]);
        public const int MEXICAN_LEAGUE_ID = 125;
        public const int DSL_LEAGUE_ID = 130;

        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db")
                .Options;
    }
}
