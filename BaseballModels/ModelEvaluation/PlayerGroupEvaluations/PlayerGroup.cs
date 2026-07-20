using Db;
using ModelDb;

namespace ModelEvaluation.PlayerGroupEvaluations
{
    public class PlayerGroup
    {
        public class UnifiedPlayer
        {
            public required Player Player { get; init; }
            public required Model_Players ModelPlayer { get; init; }
            public College_Player? CollegePlayer { get; init; }
        }

        public record PlayerWarRequest(int MlbId, bool IsHitter, int Year, int Month);
        public record PlayerModelKey(int MlbId, bool IsHitter);
        public record YearMonth(int Year, int Month)
        {
            public int TotalMonths => (Year * 12) + Month;
        }

        public class PlayerWarComparison
        {
            public required PlayerWarRequest Request { get; init; }
            public required UnifiedPlayer Player { get; init; }
            public required YearMonth StartDate { get; init; }
            public required float StartWar { get; init; }
            public required YearMonth EndDate { get; init; }
            public required float EndWar { get; init; }
        }

        public static Dictionary<TKey, List<PlayerWarComparison>> GetGroupWarPerformance<TKey>(
            IEnumerable<PlayerWarRequest> requests,
            Func<UnifiedPlayer, TKey> groupKeySelector,
            int yearsAfterStart,
            int modelId)
            where TKey : notnull
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // List of players is small, so just preload all of them
            Dictionary<int, Player> playerDict = db.Player.ToDictionary(f => f.MlbId);
            Dictionary<int, Model_Players> mpDict = db.Model_Players.ToDictionary(f => f.MlbId);
            Dictionary<int, Player_CareerStatus> pcsDict = db.Player_CareerStatus.ToDictionary(f => f.MlbId);
            Dictionary<int, College_Player> colDict = db.College_Player
                .Where(f => f.MlbId > 0)
                .ToDictionary(f => f.MlbId);

            // Preload OPWA dict
            var opwaDict = GetOpwaDict(modelDb, modelId);

            // Iterate through all requests
            List<PlayerWarRequest> requestList = requests.Distinct().ToList();
            List<PlayerWarComparison> pwcList = new(requestList.Count);
            foreach (var request in requestList)
            {
                // Get DB types
                Player player = playerDict[request.MlbId];
                Model_Players mp = mpDict[request.MlbId];
                Player_CareerStatus pcs = pcsDict[request.MlbId];
                College_Player? cp = colDict.GetValueOrDefault(request.MlbId);

                // Two-way player check
                if (mp.IsHitter && mp.IsPitcher)
                    continue;

                // Ignore player check
                if (pcs.IgnorePlayer != null)
                    continue;

                UnifiedPlayer up = new UnifiedPlayer
                {
                    Player = player,
                    ModelPlayer = mp,
                    CollegePlayer = cp
                };

                // Get Pre/Post WAR values
                OPWA_Key opwaKey = new OPWA_Key(request.MlbId, request.IsHitter);
                YearMonth requestDate = new YearMonth(request.Year, request.Month);
                float startWar = GetDateWar(opwaDict, opwaKey, requestDate);

                YearMonth startDate = GetPlayerStartDate(opwaDict, opwaKey, requestDate);
                YearMonth endDate = new YearMonth(startDate.Year + yearsAfterStart, startDate.Month);
                float endWar = GetDateWar(opwaDict, opwaKey, endDate);

                // Aggregate all values
                pwcList.Add(new PlayerWarComparison
                {
                    Request=request,
                    Player=up,
                    StartDate=requestDate,
                    StartWar=startWar,
                    EndDate=endDate,
                    EndWar=endWar,
                });
            }

            return pwcList
                .GroupBy(f => groupKeySelector(f.Player))
                .ToDictionary(
                    f => f.Key,
                    f => f.ToList()
                );
        }

        private record OPWA_Key(int mlbId, bool isHitter);
        private record OPWA_Value(YearMonth date, float war);
        private static Dictionary<OPWA_Key, List<OPWA_Value>> GetOpwaDict(ModelDbContext modelDb, int modelId)
        {
            return modelDb.Output_PlayerWarAggregation
                .Where(f => f.ModelId == modelId)
                .GroupBy(f => new OPWA_Key(f.MlbId, f.IsHitter))
                .ToDictionary(
                    keySelector: f => f.Key,
                    elementSelector: f => f.Select(
                        g => new OPWA_Value(new YearMonth(g.Year, g.Month), g.War)
                        ).ToList()
                );
        }

        private static float GetDateWar(Dictionary<OPWA_Key, List<OPWA_Value>> dict, OPWA_Key key, YearMonth date)
        {
            var items = dict[key];
            return items
                .Where(f => f.date.TotalMonths <= date.TotalMonths)
                .OrderByDescending(f => f.date.TotalMonths)
                .First()
                .war;
        }

        private static YearMonth GetPlayerStartDate(Dictionary<OPWA_Key, List<OPWA_Value>> dict, OPWA_Key key, YearMonth date)
        {
            if (date.Year != 0)
                return date;

            return dict[key]
                .Where(f => f.date.Year != 0)
                .OrderBy(f => f.date.TotalMonths).First().date;
        }
    }
}
