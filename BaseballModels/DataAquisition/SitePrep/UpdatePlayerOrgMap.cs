using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition.SitePrep
{
    internal class UpdatePlayerOrgMap
    { 
        internal class DateTeam
        {
            public required int TeamId;
            public required int Year;
            public required int Month;
            public required int Day;
            public int? PrevTeamId;
            public bool IsTransaction;
        }

        public static void Update()
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Player_OrgMap.RemoveRange(db.Player_OrgMap);
                db.SaveChanges();

                var ids = db.Player.Select(f => f.MlbId);
                using (ProgressBar progressBar = new ProgressBar(ids.Count(), "Updating Player Org Map"))
                {
                    foreach (var id in ids)
                    {
                        // Get all transactions and games
                        List<DateTeam> transactions = db.Transaction_Log.Where(f => f.MlbId == id)
                            .Select(f => new DateTeam {
                                TeamId = f.ParentOrgId,
                                PrevTeamId = f.PrevParentOrgId,
                                Year = f.Year,
                                Month = f.Month,
                                Day = f.Day,
                                IsTransaction = true,
                            }).ToList();

                        var hitterGames = db.Player_Hitter_GameLog.Where(f => f.MlbId == id)
                            .Select(f => new DateTeam
                            {
                                TeamId = f.TeamId,
                                Year = f.Year,
                                Month = f.Month,
                                Day = f.Day
                            }).AsEnumerable();

                        var pitcherGames = db.Player_Pitcher_GameLog.Where(f => f.MlbId == id)
                            .Select(f => new DateTeam
                            {
                                TeamId = f.TeamId,
                                Year = f.Year,
                                Month = f.Month,
                                Day = f.Day
                            }).AsEnumerable();

                        transactions.AddRange(hitterGames);
                        transactions.AddRange(pitcherGames);

                        // Map teamIds to parents
                        Dictionary<(int, int), int> teamMap = new();
                        foreach (var t in transactions.Where(f => !f.IsTransaction))
                        {
                            if (!teamMap.ContainsKey((t.TeamId, t.Year)))
                            {
                                int tId = Utilities.GetParentOrgId(t.TeamId, t.Year, db);
                                teamMap.Add((t.TeamId, t.Year), tId > 0 ? tId : 0);
                            }
                            t.TeamId = teamMap[(t.TeamId, t.Year)];
                        }

                        // Group by day, oldest first
                        var days = transactions
                            .GroupBy(f => (f.Year, f.Month, f.Day))
                            .OrderBy(g => g.Key.Year)
                            .ThenBy(g => g.Key.Month)
                            .ThenBy(g => g.Key.Day);
                        // Iterate through days, searching for team changes
                        int currentTeam = 0;
                        foreach (var day in days)
                        {
                            int teamId = ResolveEndOfDayOrg(day);
                            if (teamId != currentTeam)
                            {
                                db.Player_OrgMap.Add(new Player_OrgMap
                                {
                                    MlbId = id,
                                    Year = day.Key.Year,
                                    Month = day.Key.Month,
                                    Day = day.Key.Day,
                                    ParentOrgId = teamId
                                });
                                currentTeam = teamId;
                            }
                        }
                        progressBar.Tick();
                    }
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in UpdatePlayerOrgMap");
                Utilities.LogException(e);
                throw;
            }
        }

        // Handles multiple transactions on a same day
        private static int ResolveEndOfDayOrg(IEnumerable<DateTeam> day)
        {
            var txns = day.Where(f => f.IsTransaction).ToList();

            // No transactions, player played in a game
            if (txns.Count == 0)
                return day.Max(f => f.TeamId);

            // Only 1 transaction, take that team
            if (txns.Count == 1)
                return txns[0].TeamId;

            // Any org that was departed on this day can't be the final state.
            // Intra-org rows (prev == to) and unknown prevs don't eliminate anything.
            var departed = txns
                .Where(f => f.PrevTeamId.HasValue && f.PrevTeamId.Value != f.TeamId)
                .Select(f => f.PrevTeamId!.Value)
                .ToHashSet();

            var candidates = txns.Where(f => !departed.Contains(f.TeamId)).ToList();
            if (candidates.Count == 0)
                candidates = txns; // inconsistent data (cycle) – fall back to all rows

            // Tie-break when the chain can't fully resolve (e.g. DFA -> 0 and CLW -> B both unrefuted):
            // prefer a real org over no org
            return candidates.Max(f => f.TeamId);
        }
    }
}
