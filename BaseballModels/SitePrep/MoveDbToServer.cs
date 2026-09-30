using Microsoft.EntityFrameworkCore;
using SiteDb;

namespace SitePrep
{
    internal class MoveDbToServer
    {
        public static void Update()
        {
            try {
                using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

                siteDb.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint(TRUNCATE);");
                siteDb.Database.ExecuteSqlRaw("VACUUM;");
                siteDb.Dispose();

                File.Copy("../../../../SiteDb/Site.db", "../../../../Site/server/assets/Site.db", true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in MoveDbToServer");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
