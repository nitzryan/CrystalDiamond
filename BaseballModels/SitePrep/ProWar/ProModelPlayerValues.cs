using Db;
using EFCore.BulkExtensions;
using MathNet.Numerics;
using Microsoft.EntityFrameworkCore;
using ModelDb;
using ScottPlot.TickGenerators.TimeUnits;
using SiteDb;
using System.Collections.Generic;

namespace SitePrep.ProWar
{
    internal static class ProModelPlayerValues
    {
        private record PlayerModelKey(int MlbId, int ModelId, bool IsHitter);
        private record Date(int Year, int Month) : IComparable<Date>
        {
            private int Key => (Year * 100) + Month;

            public int CompareTo(Date? other) =>
                other is null ? 1 : Key.CompareTo(other.Key);

            public static bool operator <(Date left, Date right) => left.CompareTo(right) < 0;
            public static bool operator >(Date left, Date right) => left.CompareTo(right) > 0;
            public static bool operator <=(Date left, Date right) => left.CompareTo(right) <= 0;
            public static bool operator >=(Date left, Date right) => left.CompareTo(right) >= 0;
        }
        private record MlbModelPoint(Date date, float[] War);
        private record AccumulatedWarValue(Date date, float HitterWar, float PitcherWar);
        private record PlayerValuePoint(PlayerModelKey Key, float ProspectWar, float MlbWar);

        private const int YEARS_TO_PLOT = 5;

        public static void Create()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // Get end values for prospect
            Dictionary<PlayerModelKey, (float War, Date date)> ProspectValueDict = modelDb.Output_PlayerWarAggregation
                .Select(f => new { f.MlbId, f.ModelId, f.IsHitter, f.Year, f.Month, f.War })
                .Where(f => f.Month != 0) // Ignore initialization step, will not have mlb projection data
                .GroupBy(f => new { f.MlbId, f.ModelId, f.IsHitter })
                .Select(g => g
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .First())
                .ToDictionary(
                    f => new PlayerModelKey(f.MlbId, f.ModelId, f.IsHitter),
                    f => (f.War, new Date(f.Year, f.Month)));

            // Get MLB model estimates
            Dictionary<PlayerModelKey, List<MlbModelPoint>> mlbModelDict = modelDb.Output_HitterMlbWarAggregation
                .Select(f => new { f.MlbId, f.ModelId, f.Year, f.Month, f.War0, f.War1, f.War2, f.War3, f.War4, f.War5, f.War6 })
                .GroupBy(f => new { f.MlbId, f.ModelId })
                .ToDictionary(f => new PlayerModelKey(f.Key.MlbId, f.Key.ModelId, true),
                    g => g.Select(f => new MlbModelPoint(new Date(f.Year, f.Month), [f.War0, f.War1, f.War2, f.War3, f.War4, f.War5, f.War6]))
                    .ToList());

            Dictionary<PlayerModelKey, List<MlbModelPoint>> pitcherMlbModelDict = modelDb.Output_PitcherMlbWarAggregation
                .Select(f => new { f.MlbId, f.ModelId, f.Year, f.Month, 
                    War0=f.WarRP0+f.WarSP0, War1 = f.WarRP1 + f.WarSP1, 
                    War2 = f.WarRP2 + f.WarSP2, War3 = f.WarRP3 + f.WarSP3, 
                    War4 = f.WarRP4 + f.WarSP4, War5 = f.WarRP5 + f.WarSP5, 
                    War6 = f.WarRP6 + f.WarSP6, })
                .GroupBy(f => new { f.MlbId, f.ModelId })
                .ToDictionary(f => new PlayerModelKey(f.Key.MlbId, f.Key.ModelId, false),
                    g => g.Select(f => new MlbModelPoint(new Date(f.Year, f.Month), [f.War0, f.War1, f.War2, f.War3, f.War4, f.War5, f.War6]))
                    .ToList());

            mlbModelDict = mlbModelDict
                .Concat(pitcherMlbModelDict)
                .ToDictionary();

            // Filter out values from Prospect and mlb value dicts where they have the same end date
            List<PlayerModelKey> keysToRemove = new();
            foreach (var key in ProspectValueDict.Keys)
            {
                Date lastProspectDate = ProspectValueDict[key].date;
                Date maxPlayerDate = mlbModelDict[key].DefaultIfEmpty(null).Max(f => f?.date)
                    ?? throw new InvalidOperationException($"No entries found for key {key}");

                if (lastProspectDate >= maxPlayerDate)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                ProspectValueDict.Remove(key);
                mlbModelDict.Remove(key);
            }

            // Get Accumulated values
            Dictionary<int, List<AccumulatedWarValue>> accWar = db.Player_MonthlyWar
                .Select(f => new { f.MlbId, warValue = new AccumulatedWarValue(new Date(f.Year, f.Month), f.WAR_h, f.WAR_s + f.WAR_r) })
                .GroupBy(f => f.MlbId)
                .ToDictionary(f => f.Key, f => f.Select(g => g.warValue).ToList());

            #pragma warning disable CS8600
            Date maxDate = accWar.Values.Max(f => f.Max(g => g.date)) ?? throw new Exception("No dates in Player_MonthlyWar");
            #pragma warning restore CS8600

            // Generate plots N years into the future
            Date prospectCutoffDate = new Date(maxDate.Year - YEARS_TO_PLOT - 1, maxDate.Month);
            Dictionary<PlayerModelKey, (float War, Date date)> PlotProspectValueDict = ProspectValueDict
                .Where(f => f.Value.date <= prospectCutoffDate)
                .ToDictionary();

            for (int year = 0; year <= YEARS_TO_PLOT; year++)
            {
                List<PlayerValuePoint> plotPoints = new(PlotProspectValueDict.Count);
                foreach (var item in PlotProspectValueDict)
                {
                    plotPoints.Add(GetPlayerValuePoint(
                        item.Key,
                        new Date(item.Value.date.Year + year, item.Value.date.Month),
                        PlotProspectValueDict,
                        mlbModelDict,
                        accWar) ?? throw new Exception($"{item.Key.MlbId} included in graphs but had null value"));
                }

                PlotByModel(plotPoints, "../../../ProWar/Plots/", $"Year {year}");
            }

            // Generate Post-Prospect Valuation for all timesteps in DB
            List<PostProspectPlayerModel> postProspectList = new(mlbModelDict.Values.Sum(f => f.Count)); // Will overestimate, but guaranteed to have enough space without resize
            foreach (var item in ProspectValueDict)
            {
                var dates = mlbModelDict[item.Key]
                    .Where(f => f.date >= item.Value.date)
                    .Select(f => f.date);
                foreach (var date in dates)
                {
                    PlayerValuePoint? point = GetPlayerValuePoint(
                        item.Key,
                        date,
                        ProspectValueDict,
                        mlbModelDict,
                        accWar);
                    if (point == null) // Post Team-Control, stop logging
                        break;

                    postProspectList.Add(new PostProspectPlayerModel
                    {
                        MlbId = item.Key.MlbId,
                        ModelId = item.Key.ModelId,
                        IsHitter = item.Key.IsHitter,
                        Year = date.Year,
                        Month = date.Month,
                        War = point.MlbWar
                    });
                }
            }

            siteDb.PostProspectPlayerModel.ExecuteDelete();
            siteDb.BulkInsert(postProspectList);
        }

        // Get all the WAR a player has already obtained by a given date
        private static float GetAccumulatedValue(PlayerModelKey key, Date date, IReadOnlyDictionary<int, List<AccumulatedWarValue>> accWar)
        {
            if (!accWar.TryGetValue(key.MlbId, out var values))
                return 0; // No MLB data, equivalent to 0

            return key.IsHitter
                ? values.Where(f => f.date <= date).Sum(f => f.HitterWar)
                : values.Where(f => f.date <= date).Sum(f => f.PitcherWar);
        }

        // Get the expected amount of WAR a player will produce under his remaining team control that has not yet occured
        private static float? GetMlbValueProjectionWar(Date prospectEndDate, Date evalDate, IReadOnlyList<MlbModelPoint> mlbProjections)
        {
            MlbModelPoint currentEval = mlbProjections
                .Where(f => f.date <= evalDate)
                .OrderByDescending(f => f.date)
                .First()
                ?? throw new Exception("No MLB Projection found");

            // If player loses eligility in April they will only have 6 years
            // If in September, they get 6 more, but september MLB projections start at next year
            // Otherwise, get rest of current year and 6 more
            int totalEligibilityYears = prospectEndDate.Month == 4 || prospectEndDate.Month == 9 ?
                6 : 7;

            int yearsPassed = evalDate.Year - prospectEndDate.Year;
            int yearsToTake = totalEligibilityYears - yearsPassed;
            if (yearsToTake < 0) // Post Team Control
                return null;

            return currentEval.War.Take(yearsToTake).Sum();
        }

        // Get the end prospect WAR as well as the mlb model estimate (MLB projection + already accumulated)
        private static PlayerValuePoint? GetPlayerValuePoint(
            PlayerModelKey key,
            Date evalDate,
            IReadOnlyDictionary<PlayerModelKey, (float War, Date date)> prospectProjections,
            IReadOnlyDictionary<PlayerModelKey, List<MlbModelPoint>> mlbProjections,
            IReadOnlyDictionary<int, List<AccumulatedWarValue>> accWar)
        {
            (float prospectWar, Date endProspectDate) = prospectProjections[key];
            float accumulatedWar = GetAccumulatedValue(key, evalDate, accWar);
            float? mlbProjectionWar = GetMlbValueProjectionWar(endProspectDate, evalDate, mlbProjections[key]);

            if (mlbProjectionWar == null)
                return null;

            if (prospectWar > 20)
                prospectWar += 0;

            return new PlayerValuePoint(key, prospectWar, accumulatedWar + mlbProjectionWar.Value);
        }

        private static void PlotByModel(IEnumerable<PlayerValuePoint> points, string outputDir, string title)
        {
            Directory.CreateDirectory(outputDir);

            foreach (var group in points
                .GroupBy(p => (p.Key.ModelId, p.Key.IsHitter))
                .OrderBy(g => g.Key.ModelId).ThenBy(g => g.Key.IsHitter))
            {
                string role = group.Key.IsHitter ? "Hitters" : "Pitchers";
                double[] xs = group.Select(p => (double)p.ProspectWar).ToArray();
                double[] ys = group.Select(p => (double)p.MlbWar).ToArray();
                if (xs.Length < 2)
                    throw new Exception($"Less than 2 points found in plot ${title}");

                var (intercept, slope) = Fit.Line(xs, ys);
                double r2 = GoodnessOfFit.RSquared(xs.Select(x => intercept + (slope * x)), ys);
                double min = Math.Min(xs.Min(), ys.Min());
                double max = Math.Max(xs.Max(), ys.Max());

                var plot = new ScottPlot.Plot();

                var scatter = plot.Add.ScatterPoints(xs, ys);
                scatter.LegendText = $"n = {xs.Length}";

                var fitLine = plot.Add.Line(min, intercept + (slope * min), max, intercept + (slope * max));
                fitLine.LineWidth = 2;
                fitLine.LegendText = $"y = {slope:0.000}x + {intercept:0.000} (R² = {r2:0.000})";

                var identity = plot.Add.Line(min, min, max, max);
                identity.LinePattern = ScottPlot.LinePattern.Dashed;
                identity.LegendText = "y = x";

                plot.Title($"{title} – Model {group.Key.ModelId} {role}");
                plot.XLabel("Prospect model WAR");
                plot.YLabel("MLB model WAR + accumulated WAR");
                plot.ShowLegend(ScottPlot.Alignment.UpperLeft);

                plot.SavePng(Path.Combine(outputDir, $"{title}_model{group.Key.ModelId}_{role}.png"), 900, 700);
            }
        }
    }
}
    