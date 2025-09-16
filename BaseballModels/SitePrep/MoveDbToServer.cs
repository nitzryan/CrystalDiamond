namespace SitePrep
{
    internal class MoveDbToServer
    {
        public static bool Main()
        {
            try {
                File.Copy("../../../../SiteDb/Site.db", "../../../../Site/server/assets/Site.db", true);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in MoveDbToServer");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
