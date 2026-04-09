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

            var opws = db.Output_PlayerWar.GroupBy(f => new { f.MlbId, f.Model, f.IsHitter, f.Year, f.Month });
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
                        Model = o.Key.Model,
                        IsHitter = o.Key.IsHitter,
                        Year = o.Key.Year,
                        Month = o.Key.Month,
                        War0 = 0,
                        War1 = 0,
                        War2 = 0,
                        War3 = 0,
                        War4 = 0,
                        War5 = 0,
                        War6 = 0,
                        War = 0,
                    };

                    foreach (var result in o)
                    {
                        owa.War0 += result.War0 / size;
                        owa.War1 += result.War1 / size;
                        owa.War2 += result.War2 / size;
                        owa.War3 += result.War3 / size;
                        owa.War4 += result.War4 / size;
                        owa.War5 += result.War5 / size;
                        owa.War6 += result.War6 / size;
                        owa.War += result.War / size;
                    }

                    items.Add(owa);
                    progressBar.Tick();
                }
            }

            // College Hitters
            List<Output_College_HitterAggregation> collegeHitterItems = new();
            var ohcd = db.Output_College_Hitter.GroupBy(f => new { f.TbcId, f.Model, f.Year });
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
                        Model = o.Key.Model,
                        Year = o.Key.Year,
                        Draft0 = 0,
                        Draft1 = 0,
                        Draft2 = 0,
                        Draft3 = 0,
                        Draft4 = 0,
                        Draft5 = 0,
                        Draft6 = 0,
                        Draft = 0,

                        War0 = 0,
                        War1 = 0,
                        War2 = 0,
                        War3 = 0,
                        War4 = 0,
                        War5 = 0,
                        War6 = 0,
                        War = 0,

                        Off0 = 0,
                        Off1 = 0,
                        Off2 = 0,
                        Off3 = 0,
                        Off4 = 0,
                        Off5 = 0,
                        Off6 = 0,
                        OffNone = 0,

                        Def0 = 0,
                        Def1 = 0,
                        Def2 = 0,
                        Def3 = 0,
                        Def4 = 0,
                        Def5 = 0,
                        Def6 = 0,
                        DefNone = 0,

                        Pa0 = 0,
                        Pa1 = 0,
                        Pa2 = 0,
                        Pa3 = 0,
                        Pa4 = 0,
                        Pa5 = 0,
                        Pa6 = 0,

                        ProbC = 0,
                        Prob1B = 0,
                        Prob2B = 0,
                        Prob3B = 0,
                        ProbSS = 0,
                        ProbLF = 0,
                        ProbCF = 0,
                        ProbRF = 0,
                        ProbDH = 0,
                    };

                    foreach (var result in o)
                    {
                        oca.Draft0 += result.Draft0 / size;
                        oca.Draft1 += result.Draft1 / size;
                        oca.Draft2 += result.Draft2 / size;
                        oca.Draft3 += result.Draft3 / size;
                        oca.Draft4 += result.Draft4 / size;
                        oca.Draft5 += result.Draft5 / size;
                        oca.Draft6 += result.Draft6 / size;
                        oca.Draft += result.Draft / size;

                        oca.Off0 += result.Off0 / size;
                        oca.Off1 += result.Off1 / size;
                        oca.Off2 += result.Off2 / size;
                        oca.Off3 += result.Off3 / size;
                        oca.Off4 += result.Off4 / size;
                        oca.Off5 += result.Off5 / size;
                        oca.Off6 += result.Off6 / size;
                        oca.OffNone += result.OffNone / size;

                        oca.Def0 += result.Def0 / size;
                        oca.Def1 += result.Def1 / size;
                        oca.Def2 += result.Def2 / size;
                        oca.Def3 += result.Def3 / size;
                        oca.Def4 += result.Def4 / size;
                        oca.Def5 += result.Def5 / size;
                        oca.Def6 += result.Def6 / size;
                        oca.DefNone += result.DefNone / size;

                        oca.Pa0 += result.Pa0 / size;
                        oca.Pa1 += result.Pa1 / size;
                        oca.Pa2 += result.Pa2 / size;
                        oca.Pa3 += result.Pa3 / size;
                        oca.Pa4 += result.Pa4 / size;
                        oca.Pa5 += result.Pa5 / size;
                        oca.Pa6 += result.Pa6 / size;

                        oca.War0 += result.War0 / size;
                        oca.War1 += result.War1 / size;
                        oca.War2 += result.War2 / size;
                        oca.War3 += result.War3 / size;
                        oca.War4 += result.War4 / size;
                        oca.War5 += result.War5 / size;
                        oca.War6 += result.War6 / size;
                        oca.War += result.War / size;

                        oca.ProbC += result.ProbC / size;
                        oca.Prob1B += result.Prob1B / size;
                        oca.Prob2B += result.Prob2B / size;
                        oca.Prob3B += result.Prob3B / size;
                        oca.ProbSS += result.ProbSS / size;
                        oca.ProbLF += result.ProbLF / size;
                        oca.ProbCF += result.ProbCF / size;
                        oca.ProbRF += result.ProbRF / size;
                        oca.ProbDH += result.ProbDH / size;
                    }

                    collegeHitterItems.Add(oca);
                    progressBar.Tick();
                }
            }

            // College Pitchers
            List<Output_College_PitcherAggregation> collegePitcherItems = new();
            var opcd = db.Output_College_Pitcher.GroupBy(f => new { f.TbcId, f.Model, f.Year });
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
                        Model = o.Key.Model,
                        Year = o.Key.Year,
                        Draft0 = 0,
                        Draft1 = 0,
                        Draft2 = 0,
                        Draft3 = 0,
                        Draft4 = 0,
                        Draft5 = 0,
                        Draft6 = 0,
                        Draft = 0,

                        War0 = 0,
                        War1 = 0,
                        War2 = 0,
                        War3 = 0,
                        War4 = 0,
                        War5 = 0,
                        War6 = 0,
                        War = 0,

                        ProbSP = 0,
                        ProbRP = 0,
                    };

                    foreach (var result in o)
                    {
                        oca.Draft0 += result.Draft0 / size;
                        oca.Draft1 += result.Draft1 / size;
                        oca.Draft2 += result.Draft2 / size;
                        oca.Draft3 += result.Draft3 / size;
                        oca.Draft4 += result.Draft4 / size;
                        oca.Draft5 += result.Draft5 / size;
                        oca.Draft6 += result.Draft6 / size;
                        oca.Draft += result.Draft / size;

                        oca.War0 += result.War0 / size;
                        oca.War1 += result.War1 / size;
                        oca.War2 += result.War2 / size;
                        oca.War3 += result.War3 / size;
                        oca.War4 += result.War4 / size;
                        oca.War5 += result.War5 / size;
                        oca.War6 += result.War6 / size;
                        oca.War += result.War / size;

                        oca.ProbSP += result.ProbSP / size;
                        oca.ProbRP += result.ProbRP / size;
                    }

                    collegePitcherItems.Add(oca);
                    progressBar.Tick();
                }
            }

            db.BulkInsert(items);
            db.BulkInsert(collegeHitterItems);
            db.BulkInsert(collegePitcherItems);
        }

        private static void HitterStats()
        {
            using ModelDbContext db = new(Constants.MODELDB_OPTIONS);
            db.Output_HitterStatsAggregation.ExecuteDelete();

            List<Output_HitterStatsAggregation> items = new();

            var ohs = db.Output_HitterStats.GroupBy(f => new { f.MlbId, f.Model, f.LevelId, f.Year, f.Month });
            int count = ohs.Count();
            items.Capacity = count;
            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Hitter Stats"))
            {
                foreach (var o in ohs)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    Output_HitterStatsAggregation ohsa = new()
                    {
                        MlbId = o.Key.MlbId,
                        Model = o.Key.Model,
                        Year = o.Key.Year,
                        Month = o.Key.Month,
                        LevelId = o.Key.LevelId,
                        Pa = 0,
                        Hit1B = 0,
                        Hit2B = 0,
                        Hit3B = 0,
                        HitHR = 0,
                        BB = 0,
                        HBP = 0,
                        K = 0,
                        SB = 0,
                        CS = 0,
                        BSR = 0,
                        DRAA = 0,
                        ParkRunFactor = 0,
                        PercC = 0,
                        Perc1B = 0,
                        Perc2B = 0,
                        Perc3B = 0,
                        PercSS = 0,
                        PercLF = 0,
                        PercCF = 0,
                        PercRF = 0,
                        PercDH = 0,
                    };

                    foreach (var result in o)
                    {
                        ohsa.Pa += result.Pa / size;
                        ohsa.Hit1B += result.Hit1B / size;
                        ohsa.Hit2B += result.Hit2B / size;
                        ohsa.Hit3B += result.Hit3B / size;
                        ohsa.HitHR += result.HitHR / size;
                        ohsa.BB += result.BB / size;
                        ohsa.HBP += result.HBP / size;
                        ohsa.K += result.K / size;
                        ohsa.SB += result.SB / size;
                        ohsa.CS += result.CS / size;
                        ohsa.BSR += result.BSR / size;
                        ohsa.DRAA += result.DRAA / size;
                        ohsa.ParkRunFactor += result.ParkRunFactor / size;
                        ohsa.PercC += result.PercC / size;
                        ohsa.Perc1B += result.Perc1B / size;
                        ohsa.Perc2B += result.Perc2B / size;
                        ohsa.Perc3B += result.Perc3B / size;
                        ohsa.PercSS += result.PercSS / size;
                        ohsa.PercLF += result.PercLF / size;
                        ohsa.PercCF += result.PercCF / size;
                        ohsa.PercRF += result.PercRF / size;
                        ohsa.PercDH += result.PercDH / size;
                    }

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

            var ops = db.Output_PitcherStats.GroupBy(f => new { f.MlbId, f.Model, f.LevelId, f.Year, f.Month });
            int count = ops.Count();
            items.Capacity = count;
            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Pitcher Stats"))
            {
                foreach (var o in ops)
                {
                    int size = o.Count();
                    if (size == 0)
                        throw new Exception("No elements in model_results, should not happen");

                    Output_PitcherStatsAggregation opsa = new()
                    {
                        MlbId = o.Key.MlbId,
                        Model = o.Key.Model,
                        Year = o.Key.Year,
                        Month = o.Key.Month,
                        LevelId = o.Key.LevelId,
                        Outs_SP = 0,
                        Outs_RP = 0,
                        GS = 0,
                        GR = 0,
                        ERA = 0,
                        FIP = 0,
                        HR = 0,
                        BB = 0,
                        HBP = 0,
                        K = 0,
                        ParkRunFactor = 0,
                        SP_Perc = 0,
                        RP_Perc = 0,
                    };

                    foreach (var result in o)
                    {
                        opsa.Outs_SP += result.Outs_SP / size;
                        opsa.Outs_RP += result.Outs_RP / size;
                        opsa.GS += result.GS / size;
                        opsa.GR += result.GR / size;
                        opsa.ERA += result.ERA / size;
                        opsa.FIP += result.FIP / size;
                        opsa.HR += result.HR / size;
                        opsa.BB += result.BB / size;
                        opsa.HBP += result.HBP / size;
                        opsa.K += result.K / size;
                        opsa.ParkRunFactor += result.ParkRunFactor / size;
                        opsa.SP_Perc += result.SP_Perc / size;
                        opsa.RP_Perc += result.RP_Perc / size;
                    }

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
