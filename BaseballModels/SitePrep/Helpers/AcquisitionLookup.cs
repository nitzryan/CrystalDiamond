using Db;

namespace SitePrep.Helpers
{
    using AcqType = SiteDb.DbEnums.AcquisitionType;

    public class AcquisitionLookup
    {
        // Data for when/how a player arrived to an org
        private sealed record Arrival(int Year, int Month, int Day, int OrgId, Db.DbEnums.TransactionType Type);
        
        // Set of transaction types that are considered as traded for
        private static readonly HashSet<Db.DbEnums.TransactionType> TRADE_TYPES = [Db.DbEnums.TransactionType.Trade];
        
        //Drafted players and their pick
        public Dictionary<int, int> DraftPicks { get; }
        
        private readonly Dictionary<int, List<Arrival>> arrivals;
        private readonly Dictionary<(int, int, int, int), AcqType> cache = new();

        public AcquisitionLookup(SqliteDbContext db)
        {
            // Drafted players and their pick
            DraftPicks = db.Player
                .Where(f => f.DraftPick != null)
                .Select(f => new { f.MlbId, Pick = f.DraftPick!.Value })
                .ToDictionary(f => f.MlbId, f => f.Pick);

            // Track how player arrived to each organization each time they arrived
            arrivals = db.Transaction_Log
                .Where(f => f.ParentOrgId != 0
                         && (f.PrevParentOrgId == null || f.PrevParentOrgId != f.ParentOrgId))
                .Select(f => new { f.MlbId, f.Year, f.Month, f.Day, f.ParentOrgId, f.TransactionType })
                .ToList()
                .GroupBy(f => f.MlbId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => new Arrival(f.Year, f.Month, f.Day, f.ParentOrgId, f.TransactionType))
                          .OrderBy(f => f.Year).ThenBy(f => f.Month).ThenBy(f => f.Day)
                          .ToList());
        }

        // Get how a player was aquired for a given team on a given date
        public AcqType Get(int mlbId, int teamId, int year, int month)
        {
            if (cache.TryGetValue((mlbId, teamId, year, month), out AcqType cached))
                return cached;

            AcqType result = DraftPicks.ContainsKey(mlbId) ? AcqType.Draft : AcqType.Signed;
            if (arrivals.TryGetValue(mlbId, out var events))
            {
                // Most recent arrival at this org on or before the end of the snapshot month
                Arrival? latest = null;
                foreach (var e in events) // ascending by date
                {
                    if (e.Year > year || (e.Year == year && e.Month > month))
                        break;
                    if (e.OrgId == teamId)
                        latest = e;
                }
                if (latest != null && TRADE_TYPES.Contains(latest.Type))
                    result = AcqType.Traded;
            }
            cache[(mlbId, teamId, year, month)] = result;
            return result;
        }


    }
}
