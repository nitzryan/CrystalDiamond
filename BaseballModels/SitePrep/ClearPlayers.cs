using Db;
using ShellProgressBar;

namespace SitePrep
{
    internal class ClearPlayers
    {
        public static bool Main()
        {
            try {
                string path = Constants.SITE_ASSET_FOLDER + "/player/";
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ClearPlayers");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
