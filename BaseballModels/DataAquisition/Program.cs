using Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                var result = await PlayerUpdate.Main(db, Constants.START_YEAR);
            }
        }
    }
}
