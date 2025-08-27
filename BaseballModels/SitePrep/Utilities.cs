using Db;

namespace SitePrep
{
    internal class Utilities
    {
        private static List<float> WAR_BUCKETS = [0, 0.5f, 3, 7.5f, 15, 25, 35];

        public static void LogException(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            Console.Write(e.StackTrace);
        }

        public static float GetWar(Output_PlayerWarAggregation opwa)
        {
            return (opwa.Prob1 * WAR_BUCKETS[1]) +
                (opwa.Prob2 * WAR_BUCKETS[2]) +
                (opwa.Prob3 * WAR_BUCKETS[3]) +
                (opwa.Prob4 * WAR_BUCKETS[4]) +
                (opwa.Prob5 * WAR_BUCKETS[5]) +
                (opwa.Prob6 * WAR_BUCKETS[6]);
        }
    }
}
