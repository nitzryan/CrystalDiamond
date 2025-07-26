using Db;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataAquisition
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db")
                .Options;

            using (var context = new SqliteDbContext(options))
            {
                context.Database.EnsureCreated();
                //context.League_Factors.Add(new League_Factors { Leagueid = 1, Year = 2888, Runfactor = 1.01f, Hrfactor = 0.99f, });
                //context.League_Factors.Add(new League_Factors { Leagueid = 1, Year = 2889, Runfactor = 0.99f, Hrfactor = 0.99f, });
                //context.League_Factors.Add(new League_Factors { Leagueid = 2, Year = 2888, Runfactor = 1.0f, Hrfactor = 0.99f, });
                //context.League_Factors.Add(new League_Factors { Leagueid = 2, Year = 2889, Runfactor = 0.99f, Hrfactor = 1.02f, });
                //context.SaveChanges();

                var testFactors = context.League_Factors.Where(f => f.Runfactor < 1.0f);
                foreach (var tf in testFactors)
                {
                    Console.WriteLine($"Year: {tf.Year}, LeagueId: {tf.Leagueid}, RF: {tf.Runfactor}, HR: {tf.Hrfactor}");
                }
            }
        }
    }
}
