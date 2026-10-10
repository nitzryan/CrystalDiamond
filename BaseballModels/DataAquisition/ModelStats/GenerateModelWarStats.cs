using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace DataAquisition.ModelStats
{
    internal class GenerateModelWarStats
    {
        internal readonly record struct PlayerYearMonth(int MlbId, int Year, int Month);
        /// Number of future seasons (after the current one) to predict.
        private const int FUTURE_SEASONS = 6;

        /// Last month of a season. A year is "completed" once this month exists in Player_MonthlyWar.
        private const int SEASON_END_MONTH = 9;
        private const int COVID_YEAR = 2020;

        public static void CalculateHitterWar()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            HashSet<int> completedYears = GetCompletedYears(db);

            // (mlbId, year, month) -> stats, only for completed years
            Dictionary<PlayerYearMonth, HitterTotals> monthly = db.Player_MonthlyWar
                .AsNoTracking()
                .Select(p => new { p.MlbId, p.Year, p.Month, p.PA, p.OFF, p.DRAA, p.DEF, p.BSR, p.WAR_h })
                .AsEnumerable()
                .Where(p => completedYears.Contains(p.Year))
                .ToDictionary(
                    p => new PlayerYearMonth(p.MlbId, p.Year, p.Month),
                    p => new HitterTotals
                    {
                        PA = p.PA,
                        OFF = p.OFF,
                        DRAA = p.DRAA,
                        DEF = p.DEF,
                        BSR = p.BSR,
                        WAR = p.WAR_h
                    });

            List<PlayerYearMonth> sources = db.Model_HitterStats
                .AsNoTracking()
                .Select(s => new { s.MlbId, s.Year, s.Month })
                .AsEnumerable()
                .Where(s => completedYears.Contains(s.Year))
                .Select(s => new PlayerYearMonth(s.MlbId, s.Year, s.Month))
                .ToList();

            var rows = new List<Model_HitterWarValues>();

            foreach (var s in sources)
            {
                foreach (var target in GetTargets(s.Year, s.Month, completedYears))
                {
                    HitterTotals t = SumHitterRange(monthly, s.MlbId, target.TargetYear, target.FromMonth, target.ToMonth);

                    rows.Add(new Model_HitterWarValues
                    {
                        MlbId = s.MlbId,
                        Year = s.Year,
                        Month = s.Month,
                        Offset = target.Offset,
                        PA = t.PA,
                        OFF = t.OFF,
                        DRAA = t.DRAA,
                        DEF = t.DEF,
                        BSR = t.BSR,
                        WAR = t.WAR
                    });
                }
            }

            db.Model_HitterWarValues.ExecuteDelete();
            if (rows.Count > 0)
                db.BulkInsert(rows);
        }

        public static void CalculatePitcherWar()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            HashSet<int> completedYears = GetCompletedYears(db);

            Dictionary<PlayerYearMonth, PitcherTotals> monthly = db.Player_MonthlyWar
                .AsNoTracking()
                .Select(p => new { p.MlbId, p.Year, p.Month, p.IP_SP, p.IP_RP, p.WAR_s, p.WAR_r })
                .AsEnumerable()
                .Where(p => completedYears.Contains(p.Year))
                .ToDictionary(
                    p => new PlayerYearMonth(p.MlbId, p.Year, p.Month),
                    p => new PitcherTotals
                    {
                        OutsSP = InningsToOuts(p.IP_SP),
                        OutsRP = InningsToOuts(p.IP_RP),
                        WarSP = p.WAR_s,
                        WarRP = p.WAR_r
                    });

            List<PlayerYearMonth> sources = db.Model_PitcherStats
                .AsNoTracking()
                .Select(s => new { s.MlbId, s.Year, s.Month })
                .AsEnumerable()
                .Where(s => completedYears.Contains(s.Year))
                .Select(s => new PlayerYearMonth(s.MlbId, s.Year, s.Month))
                .ToList();

            var rows = new List<Model_PitcherWarValues>();

            foreach (var s in sources)
            {
                foreach (var target in GetTargets(s.Year, s.Month, completedYears))
                {
                    PitcherTotals t = SumPitcherRange(monthly, s.MlbId, target.TargetYear, target.FromMonth, target.ToMonth);

                    rows.Add(new Model_PitcherWarValues
                    {
                        MlbId = s.MlbId,
                        Year = s.Year,
                        Month = s.Month,
                        Offset = target.Offset,
                        OutsSP = t.OutsSP,
                        OutsRP = t.OutsRP,
                        WarSP = t.WarSP,
                        WarRP = t.WarRP
                    });
                }
            }

            db.Model_PitcherWarValues.ExecuteDelete();
            if (rows.Count > 0)
                db.BulkInsert(rows);
        }

        // ---------------- Helpers ----------------

        /// Years that contain the season-end month somewhere in Player_MonthlyWar.
        private static HashSet<int> GetCompletedYears(SqliteDbContext db)
        {
            return new HashSet<int>(
                db.Player_MonthlyWar
                    .Where(p => p.Month == SEASON_END_MONTH
                            && p.Year != COVID_YEAR)
                    .Select(p => p.Year)
                    .Distinct()
                    .ToList());
        }

        /// Produces the prediction windows for a source (year, month).
        /// FutureYear == year means "rest of the current season".
        private static IEnumerable<(int Offset, int TargetYear, int FromMonth, int ToMonth)> GetTargets(
            int year, int month, HashSet<int> completedYears)
        {
            if (month < SEASON_END_MONTH)
            {
                // Offset 0 is always "rest of the current season".
                yield return (0, year, month + 1, SEASON_END_MONTH);

                for (int k = 1; k <= FUTURE_SEASONS; k++)
                {
                    int targetYear = year + k;
                    if (completedYears.Contains(targetYear))
                        yield return (k, targetYear, 1, 12);
                }
            }
            else
            {
                // No "rest of season" slot, so future seasons shift down to start at 0.
                for (int k = 1; k <= FUTURE_SEASONS; k++)
                {
                    int targetYear = year + k;
                    if (completedYears.Contains(targetYear))
                        yield return (k - 1, targetYear, 1, 12);
                }
            }
        }

        private static HitterTotals SumHitterRange(
            Dictionary<PlayerYearMonth, HitterTotals> data,
            int mlbId, int year, int fromMonth, int toMonth)
        {
            var total = new HitterTotals();
            for (int m = fromMonth; m <= toMonth; m++)
            {
                if (data.TryGetValue(new PlayerYearMonth(mlbId, year, m), out var month))
                    total.Add(month);
            }
            return total;
        }

        private static PitcherTotals SumPitcherRange(
            Dictionary<PlayerYearMonth, PitcherTotals> data,
            int mlbId, int year, int fromMonth, int toMonth)
        {
            var total = new PitcherTotals();
            for (int m = fromMonth; m <= toMonth; m++)
            {
                if (data.TryGetValue(new PlayerYearMonth(mlbId, year, m), out var month))
                    total.Add(month);
            }
            return total;
        }

        /// Converts decimal innings (e.g. 6.333) to outs (e.g. 19).
        private static int InningsToOuts(double innings)
        {
            return (int)Math.Round(innings * 3, MidpointRounding.AwayFromZero);
        }

        // ---------------- Accumulators ----------------

        private sealed class HitterTotals
        {
            public int PA;
            public float OFF;
            public float DRAA;
            public float DEF;
            public float BSR;
            public float WAR;

            public void Add(HitterTotals o)
            {
                PA += o.PA;
                OFF += o.OFF;
                DRAA += o.DRAA;
                DEF += o.DEF;
                BSR += o.BSR;
                WAR += o.WAR;
            }
        }

        private sealed class PitcherTotals
        {
            public int OutsSP;
            public int OutsRP;
            public float WarSP;
            public float WarRP;

            public void Add(PitcherTotals o)
            {
                OutsSP += o.OutsSP;
                OutsRP += o.OutsRP;
                WarSP += o.WarSP;
                WarRP += o.WarRP;
            }
        }
    }
}
