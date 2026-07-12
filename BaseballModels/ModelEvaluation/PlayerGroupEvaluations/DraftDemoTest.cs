using Db;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using ScottPlot;
using static ModelEvaluation.PlayerGroupEvaluations.PlayerGroup;

namespace ModelEvaluation.PlayerGroupEvaluations
{
    internal class DraftDemoTest
    {
        public record DraftTestGroupKey(bool IsHitter, int DraftPickBucket, bool HasCollegeEntry, bool IsEligible);
        public record PowerFitResult(double Offset, double Scale, double Exponent, double Correlation);

        private static int GetBucketIndex(int pick, int[] bucketUpperBounds)
        {
            for (int i = 0; i < bucketUpperBounds.Length; i++)
            {
                if (pick <= bucketUpperBounds[i])
                    return i;
            }
            return bucketUpperBounds.Length;
        }
        public enum WarSource
        {
            ModelPre,
            ModelPost,
            Actual,
        }

        private static Func<UnifiedPlayer, DraftTestGroupKey> CreateDraftKeyFunc(
            int[] bucketUpperBounds, bool isDraftPick)
        {
            return up => new DraftTestGroupKey(
                up.ModelPlayer.IsHitter,
                GetBucketIndex(
                    isDraftPick ? up.ModelPlayer.DraftPick : up.ModelPlayer.DraftSignRank,
                    bucketUpperBounds),
                up.CollegePlayer != null,
                up.ModelPlayer.IsEligible
            );
        }

        private static readonly int[] DRAFT_BUCKET_BOUNDS = [
            3, 6, 10,
            15, 20, 25, 30,
            .. Enumerable.Range(0, 57).Select(f => (f * 10) + 40),
            int.MaxValue
        ];

        public static void RunDraftPickBucketTest(int model, int yearsAfterStart, DraftPlotOptions options)
        {
            List<PlayerWarRequest> requests;
            using (SqliteDbContext db = new(Constants.DB_OPTIONS))
            {
                requests = db.Player
                    .Where(f => f.SigningYear < 2023 && f.SigningYear >= 2005 && f.DraftPick != null)
                    .Join(db.Model_Players, f => f.MlbId, f => f.MlbId, (p, mp) => new{ p, mp})
                    .Select(f => new PlayerWarRequest(f.p.MlbId, f.mp.IsHitter, 0, 0))
                    .ToList();
            }

            // Draft Pick
            var keyFunc = CreateDraftKeyFunc(DRAFT_BUCKET_BOUNDS, true);
            var draftPerfDict = GetGroupWarPerformance(requests, keyFunc, yearsAfterStart, model);
            PlotDraftValidation(draftPerfDict, DRAFT_BUCKET_BOUNDS, options, true);

            // Draft Bonus
            keyFunc = CreateDraftKeyFunc(DRAFT_BUCKET_BOUNDS, false);
            draftPerfDict = GetGroupWarPerformance(requests, keyFunc, yearsAfterStart, model);
            PlotDraftValidation(draftPerfDict, DRAFT_BUCKET_BOUNDS, options, false);
        }

        [Flags]
        public enum DraftPlotOptions
        {
            None = 0,
            LogX = 2,
            LogY = 4,
            IsDraftPick = 8,
        }

        public record Point(double X, double Y);
        public record GroupingKey(bool isCollege, bool preCutoff, bool isHitter, WarSource warSource);

        private static void PlotDraftValidation(
            Dictionary<DraftTestGroupKey, List<PlayerWarComparison>> draftPerfDict, 
            int[] bucketUpperBounds,
            DraftPlotOptions options,
            bool isDraftPick)
        {
            // Get configurable options
            bool useLogX = options.HasFlag(DraftPlotOptions.LogX);
            bool useLogY = options.HasFlag(DraftPlotOptions.LogY);

            Dictionary<GroupingKey, List<Point>> movingAveragePointDict = new();

            // Get distinct series
            ScottPlot.Multiplot multiplot = new();
            multiplot.AddPlots(4);
            multiplot.Layout = new ScottPlot.MultiplotLayouts.Grid(
                rows: 2,
                columns: 2
            );

            int plotId = 0;
            foreach (bool isHitter in new[] { true, false })
            {
                foreach (bool isEligible in new[]{true, false})
                {
                    Plot plot = multiplot.GetPlot(plotId);
                    plotId++;

                    // Title
                    string hitTitle = isHitter ? "Hitter" : "Pitcher";
                    string eligibleTitle = isEligible ? "Pre Cutoff" : "Post Cutoff";
                    plot.Title(hitTitle + " " + eligibleTitle);

                    // Axis labels
                    if (isDraftPick)
                        plot.XLabel("Mean Draft Pick");
                    else
                        plot.XLabel("Mean Draft Bonus Rank");
                    plot.YLabel("Mean WAR");

                    foreach (WarSource warSource in new[] { WarSource.ModelPre, WarSource.ModelPost, WarSource.Actual })
                    {
                        if (warSource == WarSource.Actual && !isEligible)
                            continue;

                        foreach (bool isCollege in new[] {true, false})
                        {
                            // Track values for series
                            List<double> seriesDraft = [];
                            List<double> seriesWar = [];

                            int numBuckets = bucketUpperBounds.Length + 1;
                            for (int bucketIndex = 0; bucketIndex < numBuckets; bucketIndex++)
                            {
                                // Get War and draft pick mean for group as a whole
                                DraftTestGroupKey key = new DraftTestGroupKey(isHitter, bucketIndex, isCollege, isEligible);
                                if (!draftPerfDict.TryGetValue(key, out var values) || values.Count == 0)
                                    continue;

                                double draftPickMean = isDraftPick ?
                                    values.Average(f => f.Player.ModelPlayer.DraftPick) :
                                    values.Average(f => f.Player.ModelPlayer.DraftSignRank);

                                double warValue = warSource switch
                                {
                                    WarSource.ModelPre => values.Average(f => f.StartWar),
                                    WarSource.ModelPost => values.Average(f => f.EndWar),
                                    WarSource.Actual => values.Average(f => isHitter ?
                                        f.Player.ModelPlayer.WarHitter : f.Player.ModelPlayer.WarPitcher),
                                    _ => throw new InvalidOperationException()
                                };

                                seriesDraft.Add(draftPickMean);
                                seriesWar.Add(warValue);
                            }

                            double[] xs = seriesDraft.ToArray();
                            double[] ys = seriesWar.ToArray();

                            // Calculate moving average
                            List<Point> movingAveragePoints = [];
                            for (int i = 0; i < seriesWar.Count; i++)
                            {
                                double x = seriesDraft[i];
                                int windowSize = 1;
                                if (x > 300)
                                    windowSize = 5;
                                else if (x > 200)
                                    windowSize = 4;
                                else if (x > 100)
                                    windowSize = 3;
                                else if (x > 50)
                                    windowSize = 2;
                                

                                int entryCount = 0;
                                double y = 0;
                                
                                for (int j = i - windowSize; j <= i + windowSize; j++)
                                {
                                    if (j >= 0 && j < seriesWar.Count)
                                    {
                                        entryCount++;
                                        y += seriesWar[j];
                                    }
                                }
                                y /= entryCount;

                                movingAveragePoints.Add(new Point(x, y));
                            }
                            movingAveragePointDict.Add(new GroupingKey(isCollege, isEligible, isHitter, warSource),
                                movingAveragePoints);

                            // Log-transform
                            if (useLogX)
                            {
                                xs = xs.Select(Math.Log10).ToArray();
                            }
                            if (useLogY)
                            {
                                ys = ys.Select(Math.Log10).ToArray();
                            }

                            // Color/Marking per combination
                            Color seriesColor = (isCollege, warSource) switch
                            {
                                // College — blue family
                                (true, WarSource.ModelPre) => Colors.LightSkyBlue,
                                (true, WarSource.ModelPost) => Colors.DodgerBlue,
                                (true, WarSource.Actual) => Colors.Navy,
                                // High School — orange/red family
                                (false, WarSource.ModelPre) => Colors.GoldenRod,
                                (false, WarSource.ModelPost) => Colors.OrangeRed,
                                (false, WarSource.Actual) => Colors.DarkRed,
                                _ => Colors.Black,
                            };

                            MarkerShape markerShape = warSource switch
                            {
                                WarSource.ModelPre => MarkerShape.OpenCircle,
                                WarSource.ModelPost => MarkerShape.OpenSquare,
                                WarSource.Actual => MarkerShape.FilledCircle,
                                _ => MarkerShape.FilledCircle,
                            };
                            LinePattern fitPattern = warSource switch
                            {
                                WarSource.ModelPre => LinePattern.Dashed,
                                WarSource.ModelPost => LinePattern.Dotted,
                                WarSource.Actual => LinePattern.Solid,
                                _ => LinePattern.Solid,
                            };

                            // Legend Names
                            string sourceLabel = warSource switch
                            {
                                WarSource.ModelPre => "Pre",
                                WarSource.ModelPost => "Post",
                                WarSource.Actual => "Actual",
                                _ => throw new InvalidOperationException()
                            };
                            string colHs = isCollege ? "COL" : "HS";

                            string legendText = $"{sourceLabel} {colHs}";

                            // Plot Datapoints
                            var dataScatter = plot.Add.Scatter(xs, ys);
                            dataScatter.Color = seriesColor;
                            dataScatter.MarkerSize = 7;
                            dataScatter.MarkerShape = markerShape;
                            dataScatter.LineStyle = LineStyle.None;
                            dataScatter.LegendText = legendText;
                        }
                    }

                    

                    if (useLogX)
                    {
                        ScottPlot.TickGenerators.LogMinorTickGenerator minorGen = new();
                        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic()
                        {
                            MinorTickGenerator = minorGen,
                            IntegerTicksOnly = true,
                            LabelFormatter = pos => Math.Pow(10, pos).ToString("N0")
                        };
                    }
                    
                    if (useLogY)
                    {
                        ScottPlot.TickGenerators.LogMinorTickGenerator leftMinorGen = new();
                        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic()
                        {
                            MinorTickGenerator = leftMinorGen,
                            IntegerTicksOnly = true,
                            LabelFormatter = pos => Math.Pow(10, pos).ToString("0.##")
                        };
                    }

                    

                    // Axis Configuration
                    plot.Axes.AutoScale();
                    plot.Axes.Bottom.Min = useLogX ? 0 : 1;
                    plot.Axes.Left.Min = useLogY ? -1 : 0;
                    

                    plot.Legend.IsVisible = true;
                    plot.Legend.Alignment = Alignment.UpperRight;
                    plot.Legend.Orientation = Orientation.Vertical;
                }
            }

            // Make sure the hitter and pitcher graphs share the Y axis values
            for (int groupStart = 0; groupStart < 4; groupStart += 2)
            {
                double sharedMinY = Math.Min(
                    multiplot.GetPlot(groupStart).Axes.Left.Min,
                    multiplot.GetPlot(groupStart + 1).Axes.Left.Min
                );
                double sharedMaxY = Math.Max(
                    multiplot.GetPlot(groupStart).Axes.Left.Max,
                    multiplot.GetPlot(groupStart + 1).Axes.Left.Max
                );
                for (int i = groupStart; i < groupStart + 2; i++)
                {
                    multiplot.GetPlot(i).Axes.Left.Min = sharedMinY;
                    multiplot.GetPlot(i).Axes.Left.Max = sharedMaxY;
                }
            }

            string plotName = isDraftPick ? "Draft_Pick" : "Draft_Bonus";
            multiplot.SavePng($"../../../Output/ModelValidation/{plotName}.png", width: 1200, height: 900);

            PlotComparativeDraft(movingAveragePointDict, plotName);
        }

        private static void PlotComparativeDraft(Dictionary<GroupingKey, List<Point>> movingAveragePointDict, string plotName)
        {
            ScottPlot.Multiplot multiplot = new();
            multiplot.AddPlots(2);
            multiplot.Layout = new ScottPlot.MultiplotLayouts.Grid(
                rows: 2,
                columns: 1
            );

            // Get Same formatting for any GroupingKey on both subplots
            static (Color color, LinePattern pattern, double lineWidth, string legendText)
                GetPlotStyle(bool isCollege, bool isHitter, WarSource warSource)
            {
                ScottPlot.Color color = (isCollege, isHitter) switch
                {
                    (true, true) => Colors.Blue,     // College Hitter
                    (true, false) => Colors.Teal,     // College Pitcher
                    (false, true) => Colors.Orange,   // HS Hitter
                    _ => Colors.Red       // HS Pitcher
                };

                LinePattern pattern = warSource switch
                {
                    WarSource.Actual => LinePattern.Solid,
                    WarSource.ModelPost => LinePattern.Dashed,
                    WarSource.ModelPre => LinePattern.Dotted,
                    _ => LinePattern.Solid
                };

                double lineWidth = warSource switch
                {
                    WarSource.Actual => 2.0,
                    _ => 1.5
                };

                string group = isCollege ? "College" : "HS";
                string role = isHitter ? "Hitter" : "Pitcher";
                string source = warSource.ToString().Replace("Model", "");

                return (color, pattern, lineWidth, $"{group} {role} - {source}");
            }

            // Training Section
            List<Point> colHitterActualPointsPreCutoff = movingAveragePointDict[new GroupingKey(true, true, true, WarSource.Actual)];
            List<Point> colHitterModelPostPostCutoff = movingAveragePointDict[new GroupingKey(true, false, true, WarSource.ModelPost)];

            int plotIdx = 0;
            foreach (bool preCutoff in new[]{true, false})
            {
                Plot plot = multiplot.GetPlot(plotIdx);
                plotIdx++;

                var baseList = preCutoff ? colHitterActualPointsPreCutoff : colHitterModelPostPostCutoff;
                foreach (bool isCollege in new[]{true, false})
                {
                    foreach (bool isHitter in new[] { true, false })
                    {
                        foreach (WarSource warSource in new[] { WarSource.ModelPost, WarSource.Actual })
                        {
                            if (!preCutoff && warSource == WarSource.Actual)
                                continue;

                            if (preCutoff && warSource == WarSource.Actual && isCollege && isHitter)
                                continue;

                            if (!preCutoff && warSource == WarSource.ModelPost && isCollege && isHitter)
                                continue;

                            List<Point> points = movingAveragePointDict[new GroupingKey(isCollege, preCutoff, isHitter, warSource)];
                            List<Point> ratios = GetFractionList(baseList, points);

                            var scatter = plot.Add.Scatter(ratios.Select(f => f.X).ToArray(), ratios.Select(f => f.Y).ToArray());

                            var (color, pattern, lineWidth, legendText) = GetPlotStyle(isCollege, isHitter, warSource);
                            scatter.Color = color;
                            scatter.LineStyle.Pattern = pattern;
                            scatter.LineWidth = (float)lineWidth;
                            scatter.LegendText = legendText;
                        }
                    }
                }

                plot.Axes.AutoScale();
                plot.Axes.Left.Max = 4;
                plot.Axes.Left.Min = 0;
                plot.Axes.Bottom.Max = 600;
                plot.Axes.Bottom.Min = 0;

                var refLine = plot.Add.HorizontalLine(1.0);
                refLine.Color = Colors.Gray;
                refLine.LinePattern = LinePattern.Dashed;
                refLine.LineWidth = 1.0f;

                plot.Legend.IsVisible = true;
                plot.Legend.Orientation = Orientation.Horizontal;
                plot.Legend.Alignment = Alignment.UpperRight;

                plot.YLabel(preCutoff ? "Ratio to College Hitter Actual" : "Ratio to College Hitter ModelPost");
                plot.Title(preCutoff ? "Pre-Cutoff" : "Post-Cutoff");
            }



            multiplot.SavePng($"../../../Output/ModelValidation/{plotName}_Ratio.png", width: 1200, height: 900);
        }

        private static List<Point> GetFractionList(List<Point> baseList, List<Point> testList)
        {
            List<Point> points = new(baseList.Count);
            foreach (Point b in baseList)
            {
                Point? testPoint = testList.Where(f => Math.Abs(f.X - b.X) < 2).SingleOrDefault();
                if (testPoint == null)
                    continue;

                points.Add(new Point(b.X, b.Y / testPoint.Y));
            }

            return points;
        }
    }
}
