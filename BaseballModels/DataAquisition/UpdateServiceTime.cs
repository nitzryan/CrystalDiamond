using CsvHelper;
using Db;
using System.Globalization;

namespace DataAquisition
{
    internal class ServiceTimeCsvData
    {
        public int mlbId { get; set; }
        public int Year { get; set; }
        public int ServiceYears { get; set; }
        public int ServiceDays { get; set; }
    }

    // This is just copying from previous and doesn't incorportate new data
    // However, only needed for training model, not needed for application of it
    internal class UpdateServiceTime
    {
        public static bool Main()
        {
            try {
                // Read CSV File to tmp type
                var reader = new StreamReader(Constants.SERVICE_TIME_FILE);
                var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var data = csv.GetRecords<ServiceTimeCsvData>().ToList();

                // Clear existing data
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Player_ServiceTime.RemoveRange(db.Player_ServiceTime);
                db.SaveChanges();

                // Add entries
                foreach (var d in data)
                {
                    db.Player_ServiceTime.Add(new Player_ServiceTime
                    {
                        MlbId = d.mlbId,
                        Year = d.Year,
                        ServiceYear = d.ServiceYears,
                        ServiceDays = d.ServiceDays
                    });
                }
                db.SaveChanges();

                return true;
            } catch (Exception e)
            {
                Console.WriteLine("Error in UpdateServiceTime");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
