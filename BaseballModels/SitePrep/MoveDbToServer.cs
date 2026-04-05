namespace SitePrep
{
    internal class MoveDbToServer
    {
        public static void Update()
        {
            try {
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
