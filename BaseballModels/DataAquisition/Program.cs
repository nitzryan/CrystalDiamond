using Db;
using Microsoft.EntityFrameworkCore;

namespace DataAquisition
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db")
                .Options;

            using (var db = new SqliteDbContext(options))
            {
                //var result = await PlayerUpdate.Main(db, Constants.START_YEAR);
                //var result = await GameLogUpdate.Main(db, 2005, 4, 5);
                //var res = ParkFactorUpdate.Main(db, 2005);
                var res = CalculateLevelStats.Main(db, 2005, 4);
                res = CalculateLevelStats.Main(db, 2005, 5);
            }
        }
    }
}
