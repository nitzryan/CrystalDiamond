using Db;
using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using ModelDb;
using ShellProgressBar;
using SiteDb;

namespace SitePrep.Draft
{
    internal class TeamDraftOverviews
    {
        private const int MAX_DRAFT_RANK_PROJECTION_YEARS = 5;

        private static void CreateTeamDraftOverviews(
            int draftYear,
            List<int> evaluationYears,
            List<int> modelIds,
            SqliteDbContext db,
            SiteDbContext siteDb,
            ModelDbContext modelDb,
            ProgressBar progressBar)
        {
            // Descending so each iteration can filter opwaRows in place
            evaluationYears = evaluationYears.OrderDescending().ToList();
            int maxEvaluationYear = evaluationYears[0];

            // Players selected and signed in the draft year
            List<Draft_Results> signedPicks = db.Draft_Results
                .Where(f => f.Year == draftYear && f.Signed == 1)
                .Join(db.Player_CareerStatus, f => f.MlbId, f => f.MlbId, (dr, pcs) => new {dr, pcs})
                .Where(f => !(f.pcs.IgnorePlayer > 0))
                .Select(f => f.dr)
                .ToList();

            Dictionary<int, DraftPickValues> pickValues = db.DraftPickValues
                .ToDictionary(f => f.Pick);

            HashSet<int> draftedMlbIds = signedPicks.Select(f => f.MlbId).ToHashSet();
            List<int> teamIds = signedPicks.Select(f => f.TeamId).Distinct().ToList();

            // All OPWA rows for drafted players up to the evaluation year
            var opwaRows = modelDb.Output_PlayerWarAggregation
                .Where(f => draftedMlbIds.Contains(f.MlbId) && f.Year <= maxEvaluationYear)
                .Select(f => new
                {
                    f.MlbId,
                    f.ModelId,
                    f.IsHitter,
                    f.Year,
                    f.Month,
                    f.War
                })
                .ToList();

            foreach (int evaluationYear in evaluationYears)
            {
                if (evaluationYear != evaluationYears.First())
                    opwaRows = opwaRows.Where(f => f.Year <= evaluationYear).ToList();

                // Get last model results of year for player
                var latestModelResults = opwaRows
                    .GroupBy(f => (f.MlbId, f.ModelId))
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            int maxYear = g.Max(x => x.Year);
                            int maxMonth = g.Where(x => x.Year == maxYear).Max(x => x.Month);
                            return g.Where(x => x.Year == maxYear && x.Month == maxMonth)
                                    .OrderByDescending(x => x.War)
                                    .First();
                        });

                foreach (int modelId in modelIds)
                {
                    foreach (int teamId in teamIds)
                    {
                        float hitterCapital = 0, pitcherCapital = 0, hitterValue = 0, pitcherValue = 0;

                        var teamPicks = signedPicks.Where(f => f.TeamId == teamId);
                        foreach (Draft_Results pick in teamPicks)
                        {
                            if (!latestModelResults.TryGetValue((pick.MlbId, modelId), out var modelResults))
                                throw new Exception($"No valid OPWA entry for MlbId={pick.MlbId}, ModelId={modelId}, DraftYear={draftYear}, EvaluationYear={evaluationYear}");
                            if (!pickValues.TryGetValue(pick.Pick, out DraftPickValues? value))
                                throw new Exception($"No DraftPickValues entry for Pick={pick.Pick}");

                            if (modelResults.IsHitter)
                            {
                                hitterCapital += value.WarHitter;
                                hitterValue += modelResults.War;
                            }
                            else
                            {
                                pitcherCapital += value.WarPitcher;
                                pitcherValue += modelResults.War;
                            }
                        }

                        siteDb.TeamDraftOverview.Add(new TeamDraftOverview
                        {
                            ModelId = modelId,
                            DraftYear = draftYear,
                            EvaluationYear = evaluationYear,
                            TeamId = teamId,
                            HitterCapital = hitterCapital,
                            PitcherCapital = pitcherCapital,
                            HitterValue = hitterValue,
                            PitcherValue = pitcherValue
                        });
                    }

                    progressBar.Tick($"DraftYear={draftYear} EvaluationYear={evaluationYear} ModelId={modelId}");
                }
            }
        }

        public static void RegenerateTeamDraftOverviews()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            siteDb.TeamDraftOverview.ExecuteDelete();

            int maxTeamRankYear = siteDb.TeamRank.Max(f => f.Year);
            List<int> draftYears = siteDb.DraftRank
                .Select(f => f.Year)
                .Distinct()
                .Order()
                .ToList();
            List<int> modelIds = siteDb.Models
                .Select(f => f.ModelId)
                .ToList();

            // Evaluation years are draftYear through draftYear + MAX_DRAFT_RANK_PROJECTION_YEARS, not past the latest TeamRank year
            Dictionary<int, List<int>> evaluationYearsByDraftYear = new();
            foreach (int draftYear in draftYears)
            {
                List<int> evaluationYears = Enumerable.Range(draftYear, MAX_DRAFT_RANK_PROJECTION_YEARS + 1)
                    .Where(year => year <= maxTeamRankYear)
                    .ToList();
                if (evaluationYears.Count > 0)
                    evaluationYearsByDraftYear[draftYear] = evaluationYears;
            }

            int totalTicks = evaluationYearsByDraftYear.Values.Sum(years => years.Count) * modelIds.Count;
            using ProgressBar progressBar = new(totalTicks, "Regenerating team draft overviews");
            foreach (var (draftYear, evaluationYears) in evaluationYearsByDraftYear)
            {
                CreateTeamDraftOverviews(draftYear, evaluationYears, modelIds, db, siteDb, modelDb, progressBar);
            }

            siteDb.SaveChanges();
        }

        ///////////// Calibration ///////////////////
        public static void CheckValueCalibration()
        {
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

            List<TeamDraftOverview> overviews = siteDb.TeamDraftOverview
                .AsNoTracking()
                .ToList();

            Console.WriteLine("=== Hitter Calibration ===");
            foreach (var group in overviews.GroupBy(f => f.EvaluationYear - f.DraftYear).OrderBy(g => g.Key))
            {
                List<double> capital = group.Select(f => (double)f.HitterCapital).ToList();
                List<double> value = group.Select(f => (double)f.HitterValue).ToList();
                PrintCalibration("Hitter", group.Key, capital, value);
            }

            Console.WriteLine("=== Pitcher Calibration ===");
            foreach (var group in overviews.GroupBy(f => f.EvaluationYear - f.DraftYear).OrderBy(g => g.Key))
            {
                List<double> capital = group.Select(f => (double)f.PitcherCapital).ToList();
                List<double> value = group.Select(f => (double)f.PitcherValue).ToList();
                PrintCalibration("Pitcher", group.Key, capital, value);
            }
        }

        private static void PrintCalibration(string label, int yearsAfterDraft, List<double> capital, List<double> value)
        {
            int count = capital.Count;
            double totalCapital = capital.Sum();
            double totalValue = value.Sum();
            double ratio = totalCapital == 0 ? double.NaN : totalValue / totalCapital;
            double correlation = MathNet.Numerics.Statistics.Correlation.Pearson(capital, value);

            Console.WriteLine($"{label} Year+{yearsAfterDraft}: N={count}, TotalCapital={totalCapital:F2}, TotalValue={totalValue:F2}, Value/Capital={ratio:F3}, Correlation={correlation:F3}");
        }

        public static void CheckPickBinCalibration()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // Fixed population: only draft years that have data for every horizon
            int maxTeamRankYear = siteDb.TeamRank.Max(f => f.Year);
            int maxHorizon = MAX_DRAFT_RANK_PROJECTION_YEARS;
            int cutoffDraftYear = maxTeamRankYear - maxHorizon;

            (string Label, int Min, int Max)[] bins =
            [
                ("1-10", 1, 10),
                ("11-50", 11, 50),
                ("51-100", 51, 100),
                ("101-200", 101, 200),
                ("201+", 201, int.MaxValue)
            ];

            List<Draft_Results> picks = db.Draft_Results
                .AsNoTracking()
                .Where(f => f.Signed == 1 && f.Year <= cutoffDraftYear && f.Year >= 2005)
                .Join(db.Player_CareerStatus, f => f.MlbId, f => f.MlbId, (dr, pcs) => new { dr, pcs })
                .Where(f => !(f.pcs.IgnorePlayer > 0))
                .Select(f => f.dr)
                .ToList();

            Dictionary<int, ModelDraftPickValues> pickValues = siteDb.ModelDraftPickValues
                .AsNoTracking()
                .ToDictionary(f => f.Pick);

            List<int> modelIds = siteDb.Models
                .Select(f => f.ModelId)
                .ToList();

            HashSet<int> mlbIds = picks.Select(f => f.MlbId).ToHashSet();
            Dictionary<(int MlbId, int ModelId), List<(int Year, int Month, bool IsHitter, float War)>> opwaByPlayerModel = modelDb.Output_PlayerWarAggregation
                .AsNoTracking()
                .Where(f => mlbIds.Contains(f.MlbId) && f.Year <= maxTeamRankYear)
                .Select(f => new { f.MlbId, f.ModelId, f.Year, f.Month, f.IsHitter, f.War })
                .ToList()
                .GroupBy(f => (f.MlbId, f.ModelId))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (x.Year, x.Month, x.IsHitter, x.War)).ToList());

            // One record per pick/model/horizon
            var records = new List<(int Horizon, bool IsHitter, string Bin, double Capital, double Value)>();
            foreach (Draft_Results pick in picks)
            {
                if (!pickValues.TryGetValue(pick.Pick, out ModelDraftPickValues? pv))
                    throw new Exception($"No DraftPickValues entry for Pick={pick.Pick}");
                string bin = bins.First(b => pick.Pick >= b.Min && pick.Pick <= b.Max).Label;

                foreach (int horizon in Enumerable.Range(0, maxHorizon + 1))
                {
                    int evaluationYear = pick.Year + horizon;
                    foreach (int modelId in modelIds)
                    {
                        if (!opwaByPlayerModel.TryGetValue((pick.MlbId, modelId), out var rows))
                            throw new Exception($"No OPWA entries for MlbId={pick.MlbId}, ModelId={modelId}");

                        var eligible = rows.Where(r => r.Year <= evaluationYear).ToList();
                        if (eligible.Count == 0)
                            throw new Exception($"No valid OPWA entry for MlbId={pick.MlbId}, ModelId={modelId}, EvaluationYear={evaluationYear}");

                        int maxYear = eligible.Max(r => r.Year);
                        int maxMonth = eligible.Where(r => r.Year == maxYear).Max(r => r.Month);
                        var latest = eligible
                            .Where(r => r.Year == maxYear && r.Month == maxMonth)
                            .OrderByDescending(r => r.War)
                            .First();

                        double capital = latest.IsHitter ? pv.WarHitter : pv.WarPitcher;
                        records.Add((horizon, latest.IsHitter, bin, capital, latest.War));
                    }
                }
            }

            foreach (bool isHitter in new[] { true, false })
            {
                Console.WriteLine(isHitter ? "=== Hitter Pick-Bin Calibration ===" : "=== Pitcher Pick-Bin Calibration ===");
                string label = isHitter ? "Hitter" : "Pitcher";

                foreach (var bin in bins)
                {
                    Console.WriteLine($"=== Bin {bin.Label} ===");
                    foreach (int horizon in Enumerable.Range(0, maxHorizon + 1))
                    {
                        List<(int Horizon, bool IsHitter, string Bin, double Capital, double Value)> subset = records
                            .Where(r => r.IsHitter == isHitter && r.Horizon == horizon && r.Bin == bin.Label)
                            .ToList();
                        if (subset.Count < 2)
                            continue;

                        double totalCapital = subset.Sum(r => r.Capital);
                        double totalValue = subset.Sum(r => r.Value);
                        double ratio = totalCapital == 0 ? double.NaN : totalValue / totalCapital;
                        double correlation = Correlation.Pearson(
                            subset.Select(r => r.Capital),
                            subset.Select(r => r.Value));

                        Console.WriteLine($"{label} Year+{horizon} Picks {bin.Label}: N={subset.Count}, TotalCapital={totalCapital:F2}, TotalValue={totalValue:F2}, Value/Capital={ratio:F3}, Correlation={correlation:F3}");
                    }
                }
            }
        }
    }
}
