using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition
{
    internal class UpdatePlayerOrgMap
    { 
        internal class DateTeam
        {
            public required int TeamId;
            public required int Year;
            public required int Month;
            public required int Day;
        }

        public static bool Main()
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
                            .AsNoTracking()
                            .Select(f => new DateTeam {
                                TeamId = f.ParentOrgId,
                                Year = f.Year,
                                Month = f.Month,
                                Day = f.Day
                            }).ToList();

                        var hitterGames = db.Player_Hitter_GameLog.Where(f => f.MlbId == id)
                            .AsNoTracking()
                            .Select(f => new DateTeam
                            {
                                TeamId = f.TeamId,
                                Year = f.Year,
                                Month = f.Month,
                                Day = f.Day
                            }).AsEnumerable();

                        var pitcherGames = db.Player_Pitcher_GameLog.Where(f => f.MlbId == id)
                            .AsNoTracking()
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
                        foreach (var t in transactions)
                        {
                            if (!teamMap.ContainsKey((t.TeamId, t.Year)))
                            {
                                int tId = Utilities.GetParentOrgId(t.TeamId, t.Year, db);
                                teamMap.Add((t.TeamId, t.Year), tId > 0 ? tId : 0);
                            }
                        }

                        // Sort
                        transactions = transactions.OrderBy(f => f.Year)
                            .ThenBy(f => f.Month)
                            .ThenBy(f => f.Day)
                            .ThenByDescending(f => f.TeamId) // makes it so release/sign gives new team first below team 0
                            .ToList();

                        // Iterate through transactions, searching for team changes
                        int currentTeam = 0;

                        // Can have multiple transactions in a day, so take first
                        int prevYear = 0;
                        int prevMonth = 0;
                        int prevDay = 0;
                        foreach (var t in transactions)
                        {
                            int teamId = teamMap[(t.TeamId, t.Year)];
                            if (teamId != currentTeam && (t.Year != prevYear || t.Month != prevMonth || t.Day != prevDay))
                            {
                                //Console.WriteLine($"{id} {t.Year} {t.Month} {t.Day} {t.TeamId}");
                                db.Player_OrgMap.Add(new Player_OrgMap
                                {
                                    MlbId = id,
                                    Year = t.Year,
                                    Month = t.Month,
                                    Day = t.Day,
                                    ParentOrgId = teamId
                                });

                                currentTeam = teamId;
                                prevYear = t.Year;
                                prevDay = t.Day;
                                prevMonth = t.Month;
                            }
                        }

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in UpdatePlayerOrgMap");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
