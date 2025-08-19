namespace SitePrep
{
    internal class Utilities
    {
        public static void LogException(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            Console.Write(e.StackTrace);
        }
    }
}
