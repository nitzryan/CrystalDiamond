using Db;
using System.Numerics;

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

        public static Func<List<int>, Player_Hitter_GameLog, List<int>> PositionAggregation = (l, gl) =>
        {
            List<int> gamesAtPosition = [l[0], l[1], l[2], l[3], l[4], l[5], l[6], l[7], l[8], l[9], l[10], l[11]];
            gamesAtPosition[gl.Position]++;
            return gamesAtPosition;
        };

        public static string GetPosition(SqliteDbContext db, int mlbId, int year)
        {
            string position = "";
            var games = db.Player_Hitter_GameLog.Where(f => f.MlbId == mlbId && (f.Year == year || f.Year == year - 1) && f.Position < 10)
                .AsEnumerable();

            if (!games.Any())
                return "DH";

            List<int> initList = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            var gamesAtPosition = games.Aggregate(initList, (a, b) => PositionAggregation(a, b));
            int totalGames = games.Count();
            List<float> gamesProp = [.. gamesAtPosition.Select(f => (float)f / totalGames)];

            const float SINGLE_POSITION_CUTOFF = 0.8f;
            const float MULTI_POSITION_CUTOFF = 0.2f;

            List<bool> sp = [.. gamesProp.Select(f => f > SINGLE_POSITION_CUTOFF)];
            List<bool> mps = [.. gamesProp.Select(f => f > MULTI_POSITION_CUTOFF)];
            // Single Position
            if (sp[2])
                position = "C";
            else if (sp[3])
                position = "1B";
            else if (sp[4])
                position = "2B";
            else if (sp[5])
                position = "3B";
            else if (sp[6])
                position = "SS";
            else if (sp[7])
                position = "LF";
            else if (sp[8])
                position = "CF";
            else if (sp[9])
                position = "RF";
            else if (gamesProp[7] + gamesProp[8] + gamesProp[9] > SINGLE_POSITION_CUTOFF)
                position = "OF";
            else
            {
                if (mps[2])
                    position += "C/";
                if (mps[3])
                    position += "1B/";
                if (mps[4])
                    position += "2B/";
                if (mps[5])
                    position += "3B/";
                if (mps[6])
                    position += "SS/";
                if (mps[7])
                    position += "LF/";
                if (mps[8])
                    position += "CF/";
                if (mps[9])
                    position += "RF/";

                if (position.Length > 0)
                    position = position.Substring(0, position.Length - 1);
                else
                    position = "UTIL";
            }

            return position;
        }
    }
}
