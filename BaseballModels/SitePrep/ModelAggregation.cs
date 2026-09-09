using ModelDb;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace SitePrep
{
    internal class ModelAggregation
    {
        private static void PlayerWar()
        {
            using ModelDbContext db = new(Constants.MODELDB_OPTIONS);
            db.Output_PlayerWarAggregation.ExecuteDelete();
            db.Output_College_HitterAggregation.ExecuteDelete();
            db.Output_College_PitcherAggregation.ExecuteDelete();

            List<Output_PlayerWarAggregation> items = new();

            var opws = db.Output_PlayerWar.GroupBy(f => new { f.MlbId, f.ModelId, f.IsHitter, f.Year, f.Month });
            int count = opws.Count();
                
            items.Capacity = count;
            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating WarBucket Model Results"))
            {
                foreach (var o in opws)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    Output_PlayerWarAggregation owa = new()
                    {
                        MlbId = o.Key.MlbId,
                        ModelId = o.Key.ModelId,
                        IsHitter = o.Key.IsHitter,
                        Year = o.Key.Year,
                        Month = o.Key.Month,
                        War0 = o.Average(result => result.War0),
                        War1 = o.Average(result => result.War1),
                        War2 = o.Average(result => result.War2),
                        War3 = o.Average(result => result.War3),
                        War4 = o.Average(result => result.War4),
                        War5 = o.Average(result => result.War5),
                        War6 = o.Average(result => result.War6),
                        War = o.Average(result => result.War),
                    };

                    items.Add(owa);
                    progressBar.Tick();
                }
            }

            // College Hitters
            List<Output_College_HitterAggregation> collegeHitterItems = new();
            var ohcd = db.Output_College_Hitter.GroupBy(f => new { f.TbcId, f.ModelId, f.Year });
            count = ohcd.Count();
            collegeHitterItems.Capacity = count;

            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating College Hitter Model Results"))
            {
                foreach (var o in ohcd)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    Output_College_HitterAggregation oca = new()
                    {
                        TbcId = o.Key.TbcId,
                        ModelId = o.Key.ModelId,
                        Year = o.Key.Year,
                        Draft0 = o.Average(result => result.Draft0),
                        Draft1 = o.Average(result => result.Draft1),
                        Draft2 = o.Average(result => result.Draft2),
                        Draft3 = o.Average(result => result.Draft3),
                        Draft4 = o.Average(result => result.Draft4),
                        Draft5 = o.Average(result => result.Draft5),
                        Draft6 = o.Average(result => result.Draft6),
                        Draft = o.Average(result => result.Draft),

                        Off0 = o.Average(result => result.Off0),
                        Off1 = o.Average(result => result.Off1),
                        Off2 = o.Average(result => result.Off2),
                        Off3 = o.Average(result => result.Off3),
                        Off4 = o.Average(result => result.Off4),
                        Off5 = o.Average(result => result.Off5),
                        Off6 = o.Average(result => result.Off6),
                        OffNone = o.Average(result => result.OffNone),

                        Def0 = o.Average(result => result.Def0),
                        Def1 = o.Average(result => result.Def1),
                        Def2 = o.Average(result => result.Def2),
                        Def3 = o.Average(result => result.Def3),
                        Def4 = o.Average(result => result.Def4),
                        Def5 = o.Average(result => result.Def5),
                        Def6 = o.Average(result => result.Def6),
                        DefNone = o.Average(result => result.DefNone),

                        Pa0 = o.Average(result => result.Pa0),
                        Pa1 = o.Average(result => result.Pa1),
                        Pa2 = o.Average(result => result.Pa2),
                        Pa3 = o.Average(result => result.Pa3),
                        Pa4 = o.Average(result => result.Pa4),
                        Pa5 = o.Average(result => result.Pa5),
                        Pa6 = o.Average(result => result.Pa6),

                        War0 = o.Average(result => result.War0),
                        War1 = o.Average(result => result.War1),
                        War2 = o.Average(result => result.War2),
                        War3 = o.Average(result => result.War3),
                        War4 = o.Average(result => result.War4),
                        War5 = o.Average(result => result.War5),
                        War6 = o.Average(result => result.War6),
                        War = o.Average(result => result.War),

                        ProbC = o.Average(result => result.ProbC),
                        Prob1B = o.Average(result => result.Prob1B),
                        Prob2B = o.Average(result => result.Prob2B),
                        Prob3B = o.Average(result => result.Prob3B),
                        ProbSS = o.Average(result => result.ProbSS),
                        ProbLF = o.Average(result => result.ProbLF),
                        ProbCF = o.Average(result => result.ProbCF),
                        ProbRF = o.Average(result => result.ProbRF),
                        ProbDH = o.Average(result => result.ProbDH),
                    };

                    collegeHitterItems.Add(oca);
                    progressBar.Tick();
                }
            }

            // College Pitchers
            List<Output_College_PitcherAggregation> collegePitcherItems = new();
            var opcd = db.Output_College_Pitcher.GroupBy(f => new { f.TbcId, f.ModelId, f.Year });
            count = opcd.Count();
            collegePitcherItems.Capacity = count;

            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating College Pitcher Model Results"))
            {
                foreach (var o in opcd)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    Output_College_PitcherAggregation oca = new()
                    {
                        TbcId = o.Key.TbcId,
                        ModelId = o.Key.ModelId,
                        Year = o.Key.Year,
                        Draft0 = o.Average(result => result.Draft0),
                        Draft1 = o.Average(result => result.Draft1),
                        Draft2 = o.Average(result => result.Draft2),
                        Draft3 = o.Average(result => result.Draft3),
                        Draft4 = o.Average(result => result.Draft4),
                        Draft5 = o.Average(result => result.Draft5),
                        Draft6 = o.Average(result => result.Draft6),
                        Draft = o.Average(result => result.Draft),

                        War0 = o.Average(result => result.War0),
                        War1 = o.Average(result => result.War1),
                        War2 = o.Average(result => result.War2),
                        War3 = o.Average(result => result.War3),
                        War4 = o.Average(result => result.War4),
                        War5 = o.Average(result => result.War5),
                        War6 = o.Average(result => result.War6),
                        War = o.Average(result => result.War),

                        ProbSP = o.Average(result => result.ProbSP),
                        ProbRP = o.Average(result => result.ProbRP),
                    };

                    collegePitcherItems.Add(oca);
                    progressBar.Tick();
                }
            }

            db.BulkInsert(items);
            db.BulkInsert(collegeHitterItems);
            db.BulkInsert(collegePitcherItems);
        }

        private static void PlayerLevel()
        {
            using ModelDbContext db = new(Constants.MODELDB_OPTIONS);
            db.Output_PlayerHighestLevelAggregation.ExecuteDelete();

            List<Output_PlayerHighestLevelAggregation> items = new();

            var opws = db.Output_PlayerHighestLevel.GroupBy(f => new { f.MlbId, f.ModelId, f.IsHitter, f.Year, f.Month });
            int count = opws.Count();

            items.Capacity = count;
            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Highest Level Model Results"))
            {
                foreach (var o in opws)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    Output_PlayerHighestLevelAggregation ophla = new()
                    {
                        MlbId = o.Key.MlbId,
                        ModelId = o.Key.ModelId,
                        IsHitter = o.Key.IsHitter,
                        Year = o.Key.Year,
                        Month = o.Key.Month,
                        DSL = o.Average(result => result.DSL),
                        CPX = o.Average(result => result.CPX),
                        A_LOW = o.Average(result => result.A_LOW),
                        A = o.Average(result => result.A),
                        A_HIGH = o.Average(result => result.A_HIGH),
                        AA = o.Average(result => result.AA),
                        AAA = o.Average(result => result.AAA),
                        MLB = o.Average(result => result.MLB),
                    };

                    items.Add(ophla);
                    progressBar.Tick();
                }
            }

            db.BulkInsert(items);
        }

        private static void HitterStats()
        {
            using ModelDbContext db = new(Constants.MODELDB_OPTIONS);
            db.Output_HitterStatsAggregation.ExecuteDelete();

            List<Output_HitterStatsAggregation> items = new();

            var ohs = db.Output_HitterStats.GroupBy(f => new { f.MlbId, f.ModelId, f.LevelId, f.Year, f.Month });
            int count = ohs.Count();
            items.Capacity = count;
            int maxCount = db.Output_HitterStats.Max(f => f.ModelRun);

            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Hitter Stats"))
            {
                foreach (var o in ohs)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    // If some models had under the minimum, throw out
                    if (size < maxCount)
                    {
                        progressBar.Tick();
                        continue;
                    }

                    Output_HitterStatsAggregation ohsa = new()
                    {
                        MlbId = o.Key.MlbId,
                        ModelId = o.Key.ModelId,
                        Year = o.Key.Year,
                        Month = o.Key.Month,
                        LevelId = o.Key.LevelId,
                        Pa = o.Average(f => f.Pa),
                        Hit1B = o.Average(result => result.Hit1B),
                        Hit2B = o.Average(result => result.Hit2B),
                        Hit3B = o.Average(result => result.Hit3B),
                        HitHR = o.Average(result => result.HitHR),
                        BB = o.Average(result => result.BB),
                        HBP = o.Average(result => result.HBP),
                        K = o.Average(result => result.K),
                        SB = o.Average(result => result.SB),
                        CS = o.Average(result => result.CS),
                        BSR = o.Average(result => result.BSR),
                        DRAA = o.Average(result => result.DRAA),
                        ParkRunFactor = o.Average(result => result.ParkRunFactor),
                        PercC = o.Average(result => result.PercC),
                        Perc1B = o.Average(result => result.Perc1B),
                        Perc2B = o.Average(result => result.Perc2B),
                        Perc3B = o.Average(result => result.Perc3B),
                        PercSS = o.Average(result => result.PercSS),
                        PercLF = o.Average(result => result.PercLF),
                        PercCF = o.Average(result => result.PercCF),
                        PercRF = o.Average(result => result.PercRF),
                        PercDH = o.Average(result => result.PercDH),
                    };

                    items.Add(ohsa);
                    progressBar.Tick();
                }
            }

            db.BulkInsert(items);
        }

        private static void PitcherStats()
        {
            using ModelDbContext db = new(Constants.MODELDB_OPTIONS);
            db.Output_PitcherStatsAggregation.ExecuteDelete();

            List<Output_PitcherStatsAggregation> items = new();

            var ops = db.Output_PitcherStats.GroupBy(f => new { f.MlbId, f.ModelId, f.LevelId, f.Year, f.Month });
            int count = ops.Count();
            items.Capacity = count;
            int maxCount = db.Output_PitcherStats.Max(f => f.ModelRun);
            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Pitcher Stats"))
            {
                foreach (var o in ops)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    if (size < maxCount)
                    {
                        progressBar.Tick();
                        continue;
                    }

                    Output_PitcherStatsAggregation opsa = new()
                    {
                        MlbId = o.Key.MlbId,
                        ModelId = o.Key.ModelId,
                        Year = o.Key.Year,
                        Month = o.Key.Month,
                        LevelId = o.Key.LevelId,
                        Outs_SP = o.Average(result => result.Outs_SP),
                        Outs_RP = o.Average(result => result.Outs_RP),
                        GS = o.Average(result => result.GS),
                        GR = o.Average(result => result.GR),
                        ERA = o.Average(result => result.ERA),
                        FIP = o.Average(result => result.FIP),
                        HR = o.Average(result => result.HR),
                        BB = o.Average(result => result.BB),
                        HBP = o.Average(result => result.HBP),
                        K = o.Average(result => result.K),
                        ParkRunFactor = o.Average(result => result.ParkRunFactor),
                        SP_Perc = o.Average(result => result.SP_Perc),
                        RP_Perc = o.Average(result => result.RP_Perc),
                    };

                    items.Add(opsa);
                    progressBar.Tick();
                }
            }

            db.BulkInsert(items);
        }

        public static void Update()
        {
            try
            {
                ModelAggregation.PlayerWar();
                ModelAggregation.PlayerLevel();
                ModelAggregation.HitterStats();
                ModelAggregation.PitcherStats();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ModelAggregation");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
