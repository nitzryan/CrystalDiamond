using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using SiteDb;

namespace SitePrep.Draft
{
    internal class TeamDraftPlayerGen
    {
        private sealed record DraftEntry(
            int ModelId,
            int MlbId,
            bool IsHitter,
            int Year,
            int? DraftPick,
            int? DraftTeamid,
            string? Name);

        private sealed record WarRow(
            int ModelId,
            int MlbId,
            bool IsHitter,
            int Year,
            int Month,
            float War);

        private sealed record StatRow(int MlbId, int Year, int Month);

        // Year and month combined into one comparable key, e.g. 2022-09 => 202209
        private static int MonthKey(int year, int month) => (year * 100) + month;

        private static Dictionary<int, List<int>> ToStatKeys(IEnumerable<StatRow> rows) =>
            rows
                .GroupBy(r => r.MlbId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => MonthKey(r.Year, r.Month)).OrderBy(k => k).ToList());

        public static void Calculate()
        {
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            siteDb.TeamDraftPlayer.ExecuteDelete();

            var draftEntries = siteDb.DraftRank
                .Where(d => d.DraftTeamid != null)
                .Select(d => new DraftEntry(d.ModelId, d.MlbId, d.IsHitter, d.Year, d.DraftPick, d.DraftTeamid, d.Name))
                .ToList();

            var mlbIds = draftEntries.Select(d => d.MlbId).Distinct().ToList();
            var modelIds = draftEntries.Select(d => d.ModelId).Distinct().ToList();

            // Filter to drafted players in SQL, then to exact (model, mlbId, isHitter) in memory
            var playerModelHistory = siteDb.PlayerModel
                .Where(p => modelIds.Contains(p.ModelId) && mlbIds.Contains(p.MlbId))
                .Select(p => new WarRow(p.ModelId, p.MlbId, p.IsHitter, p.Year, p.Month, p.War))
                .AsEnumerable()
                .GroupBy(r => (r.ModelId, r.MlbId, r.IsHitter))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(r => r.Year).ThenBy(r => r.Month).ToList());

            var pickSet = siteDb.ModelDraftPickValues
                .Select(p => p.Pick)
                .ToList()
                .ToHashSet();

            var hitterKeys = ToStatKeys(db.Model_HitterStats
                .Where(h => mlbIds.Contains(h.MlbId))
                .Select(h => new StatRow(h.MlbId, h.Year, h.Month))
                .AsEnumerable()
                );

            var pitcherKeys = ToStatKeys(db.Model_PitcherStats
                .Where(p => mlbIds.Contains(p.MlbId))
                .Select(p => new StatRow(p.MlbId, p.Year, p.Month))
                .AsEnumerable()
                );

            var maxYear = siteDb.PlayerModel.Max(p => p.Year);

            var rows = new List<TeamDraftPlayer>(draftEntries.Count);
            using (var pbar = new ProgressBar(draftEntries.Count, "Calculating TeamDraftPlayer"))
            {
                foreach (var draft in draftEntries)
                {
                    pbar.Tick(draft.Name ?? $"mlbId {draft.MlbId}");
                    rows.Add(BuildRow(draft, playerModelHistory, pickSet, maxYear, hitterKeys, pitcherKeys));
                }
            }

            siteDb.BulkInsert(rows);
        }

        private static TeamDraftPlayer BuildRow(
            DraftEntry draft,
            IReadOnlyDictionary<(int ModelId, int MlbId, bool IsHitter), List<WarRow>> playerModelHistory,
            HashSet<int> pickSet,
            int maxYear,
            IReadOnlyDictionary<int, List<int>> hitterKeys,
            IReadOnlyDictionary<int, List<int>> pitcherKeys)
        {
            var name = draft.Name ?? $"mlbId {draft.MlbId}";

            var pick = draft.DraftPick
                ?? throw new InvalidDataException($"{name}: drafted but DraftPick is null");
            var teamId = draft.DraftTeamid
                ?? throw new InvalidDataException($"{name}: drafted but DraftTeamid is null");

            if (!pickSet.Contains(pick))
                throw new InvalidDataException($"{name}: no ModelDraftPickValues entry for pick {pick}");

            if (draft.Year > maxYear)
                throw new InvalidDataException($"{name}: draft year {draft.Year} is after latest data year {maxYear}");

            if (!playerModelHistory.TryGetValue((draft.ModelId, draft.MlbId, draft.IsHitter), out var warRows))
                throw new InvalidDataException($"{name}: no PlayerModel rows for model {draft.ModelId}");

            var initial = warRows.SingleOrDefault(r => r.Year == 0 && r.Month == 0)
                ?? throw new InvalidDataException($"{name}: no initial PlayerModel row");

            var ranks = warRows.Where(r => !(r.Year == 0 && r.Month == 0)).ToList();

            // Most recent data at or before the year, falling back to the initial value
            float WarAsOf(int year) => ranks.LastOrDefault(r => r.Year <= year)?.War ?? initial.War;

            var last = ranks.Count > 0 ? ranks[^1] : initial;
            int? postEligible = null;
            if (ranks.Count > 0)
                postEligible = PostEligibleYear(
                    draft.IsHitter ? hitterKeys : pitcherKeys,
                    draft.MlbId,
                    MonthKey(last.Year, last.Month));

            // Year n is shown only when it is strictly before the latest data year
            float? YearWar(int n)
            {
                var y = draft.Year + n;
                return y < maxYear ? (float?)WarAsOf(y) : null;
            }

            return new TeamDraftPlayer
            {
                ModelId = draft.ModelId,
                MlbId = draft.MlbId,
                IsHitter = draft.IsHitter,
                DraftYear = draft.Year,
                DraftPick = pick,
                DraftTeamId = teamId,
                InitialWar = initial.War,
                WarYear1 = YearWar(1),
                WarYear2 = YearWar(2),
                WarYear3 = YearWar(3),
                WarYear4 = YearWar(4),
                WarYear5 = YearWar(5),
                WarYear6 = YearWar(6),
                CurrentWar = WarAsOf(maxYear),
                PostEligibleYear = postEligible,
            };
        }

        private static int? PostEligibleYear(
            IReadOnlyDictionary<int, List<int>> statKeys,
            int mlbId,
            int lastDataKey)
        {
            if (!statKeys.TryGetValue(mlbId, out var keys))
                throw new Exception($"No statKeys found for {mlbId}");

            // First stats datapoint after the player's last PlayerModel month
            foreach (var key in keys)
            {
                if (key > lastDataKey)
                    return key / 100;
            }

            return null;
        }
    }
}