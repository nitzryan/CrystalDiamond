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
                for (int year = 2005; year <= 2006; year++)
                {
                    //var result = await PlayerUpdate.Main(db, year);
                    //result = await GameLogUpdate.Main(db, year, 3, 10);
                    //var res = ParkFactorUpdate.Main(db, year);
                    for (int month = 4; month <= 9; month++)
                    {
                        //res = CalculateLevelStats.Main(db, year, month);
                        var res = CalculateMonthStats.Main(db, year, month);
                    }
                    //res = CalculateLevelStats.Main(db, year, 4);
                    //res = CalculateLevelStats.Main(db, year, 5);
                    //res = CalculateLevelStats.Main(db, year, 6);
                    //res = CalculateLevelStats.Main(db, year, 7);
                    //res = CalculateLevelStats.Main(db, year, 8);
                    //res = CalculateLevelStats.Main(db, year, 9);
                    
                }
            }
        }
    }
}
