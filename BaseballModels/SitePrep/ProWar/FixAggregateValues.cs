using Microsoft.EntityFrameworkCore;
using ModelDb;
using ShellProgressBar;

namespace SitePrep.ProWar
{
    internal class FixAggregateValues
    {
        public static void Update()
        {
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            var hitterDict = modelDb.SingleYearHitterBucketAverages
                .AsNoTracking()
                .ToDictionary(f => f.Month);

            var pitcherDict = modelDb.SingleYearPitcherBucketAverages
                .AsNoTracking()
                .ToDictionary(f => f.Month);

            var fullYearHitBucket = hitterDict[0];
            var fullYearPitBucket = pitcherDict[0];

            using (ProgressBar progressBar = new ProgressBar(modelDb.Output_HitterMlbWarAggregation.Count(), "Fixing MLB Hitter War Aggregation"))
            {
                foreach (var mlbHit in modelDb.Output_HitterMlbWarAggregation)
                {
                    var month = mlbHit.Month == 9 ? 0 : mlbHit.Month;
                    var year0Bucket = hitterDict[month];
                    mlbHit.War0 = (mlbHit.War_Yr0_Bkt0 * year0Bucket.War0) + (mlbHit.War_Yr0_Bkt1 * year0Bucket.War1) + (mlbHit.War_Yr0_Bkt2 * year0Bucket.War2) + (mlbHit.War_Yr0_Bkt3 * year0Bucket.War3) +
                        (mlbHit.War_Yr0_Bkt4 * year0Bucket.War4) + (mlbHit.War_Yr0_Bkt5 * year0Bucket.War5) + (mlbHit.War_Yr0_Bkt6 * year0Bucket.War6) + (mlbHit.War_Yr0_Bkt7 * year0Bucket.War7);

                    mlbHit.Pa0 = (mlbHit.Pa_Yr0_Bkt0 * year0Bucket.Pa0) + (mlbHit.Pa_Yr0_Bkt1 * year0Bucket.Pa1) + (mlbHit.Pa_Yr0_Bkt2 * year0Bucket.Pa2) + (mlbHit.Pa_Yr0_Bkt3 * year0Bucket.Pa3) +
                        (mlbHit.Pa_Yr0_Bkt4 * year0Bucket.Pa4) + (mlbHit.Pa_Yr0_Bkt5 * year0Bucket.Pa5) + (mlbHit.Pa_Yr0_Bkt6 * year0Bucket.Pa6);

                    mlbHit.War1 = (mlbHit.War_Yr1_Bkt0 * fullYearHitBucket.War0) + (mlbHit.War_Yr1_Bkt1 * fullYearHitBucket.War1) + (mlbHit.War_Yr1_Bkt2 * fullYearHitBucket.War2) + (mlbHit.War_Yr1_Bkt3 * fullYearHitBucket.War3) +
                        (mlbHit.War_Yr1_Bkt4 * fullYearHitBucket.War4) + (mlbHit.War_Yr1_Bkt5 * fullYearHitBucket.War5) + (mlbHit.War_Yr1_Bkt6 * fullYearHitBucket.War6) + (mlbHit.War_Yr1_Bkt7 * fullYearHitBucket.War7);

                    mlbHit.Pa1 = (mlbHit.Pa_Yr1_Bkt0 * fullYearHitBucket.Pa0) + (mlbHit.Pa_Yr1_Bkt1 * fullYearHitBucket.Pa1) + (mlbHit.Pa_Yr1_Bkt2 * fullYearHitBucket.Pa2) + (mlbHit.Pa_Yr1_Bkt3 * fullYearHitBucket.Pa3) +
                        (mlbHit.Pa_Yr1_Bkt4 * fullYearHitBucket.Pa4) + (mlbHit.Pa_Yr1_Bkt5 * fullYearHitBucket.Pa5) + (mlbHit.Pa_Yr1_Bkt6 * fullYearHitBucket.Pa6);

                    mlbHit.War2 = (mlbHit.War_Yr2_Bkt0 * fullYearHitBucket.War0) + (mlbHit.War_Yr2_Bkt1 * fullYearHitBucket.War1) + (mlbHit.War_Yr2_Bkt2 * fullYearHitBucket.War2) + (mlbHit.War_Yr2_Bkt3 * fullYearHitBucket.War3) +
                        (mlbHit.War_Yr2_Bkt4 * fullYearHitBucket.War4) + (mlbHit.War_Yr2_Bkt5 * fullYearHitBucket.War5) + (mlbHit.War_Yr2_Bkt6 * fullYearHitBucket.War6) + (mlbHit.War_Yr2_Bkt7 * fullYearHitBucket.War7);

                    mlbHit.Pa2 = (mlbHit.Pa_Yr2_Bkt0 * fullYearHitBucket.Pa0) + (mlbHit.Pa_Yr2_Bkt1 * fullYearHitBucket.Pa1) + (mlbHit.Pa_Yr2_Bkt2 * fullYearHitBucket.Pa2) + (mlbHit.Pa_Yr2_Bkt3 * fullYearHitBucket.Pa3) +
                        (mlbHit.Pa_Yr2_Bkt4 * fullYearHitBucket.Pa4) + (mlbHit.Pa_Yr2_Bkt5 * fullYearHitBucket.Pa5) + (mlbHit.Pa_Yr2_Bkt6 * fullYearHitBucket.Pa6);

                    mlbHit.War3 = (mlbHit.War_Yr3_Bkt0 * fullYearHitBucket.War0) + (mlbHit.War_Yr3_Bkt1 * fullYearHitBucket.War1) + (mlbHit.War_Yr3_Bkt2 * fullYearHitBucket.War2) + (mlbHit.War_Yr3_Bkt3 * fullYearHitBucket.War3) +
                        (mlbHit.War_Yr3_Bkt4 * fullYearHitBucket.War4) + (mlbHit.War_Yr3_Bkt5 * fullYearHitBucket.War5) + (mlbHit.War_Yr3_Bkt6 * fullYearHitBucket.War6) + (mlbHit.War_Yr3_Bkt7 * fullYearHitBucket.War7);

                    mlbHit.Pa3 = (mlbHit.Pa_Yr3_Bkt0 * fullYearHitBucket.Pa0) + (mlbHit.Pa_Yr3_Bkt1 * fullYearHitBucket.Pa1) + (mlbHit.Pa_Yr3_Bkt2 * fullYearHitBucket.Pa2) + (mlbHit.Pa_Yr3_Bkt3 * fullYearHitBucket.Pa3) +
                        (mlbHit.Pa_Yr3_Bkt4 * fullYearHitBucket.Pa4) + (mlbHit.Pa_Yr3_Bkt5 * fullYearHitBucket.Pa5) + (mlbHit.Pa_Yr3_Bkt6 * fullYearHitBucket.Pa6);

                    mlbHit.War4 = (mlbHit.War_Yr4_Bkt0 * fullYearHitBucket.War0) + (mlbHit.War_Yr4_Bkt1 * fullYearHitBucket.War1) + (mlbHit.War_Yr4_Bkt2 * fullYearHitBucket.War2) + (mlbHit.War_Yr4_Bkt3 * fullYearHitBucket.War3) +
                        (mlbHit.War_Yr4_Bkt4 * fullYearHitBucket.War4) + (mlbHit.War_Yr4_Bkt5 * fullYearHitBucket.War5) + (mlbHit.War_Yr4_Bkt6 * fullYearHitBucket.War6) + (mlbHit.War_Yr4_Bkt7 * fullYearHitBucket.War7);

                    mlbHit.Pa4 = (mlbHit.Pa_Yr4_Bkt0 * fullYearHitBucket.Pa0) + (mlbHit.Pa_Yr4_Bkt1 * fullYearHitBucket.Pa1) + (mlbHit.Pa_Yr4_Bkt2 * fullYearHitBucket.Pa2) + (mlbHit.Pa_Yr4_Bkt3 * fullYearHitBucket.Pa3) +
                        (mlbHit.Pa_Yr4_Bkt4 * fullYearHitBucket.Pa4) + (mlbHit.Pa_Yr4_Bkt5 * fullYearHitBucket.Pa5) + (mlbHit.Pa_Yr4_Bkt6 * fullYearHitBucket.Pa6);

                    mlbHit.War5 = (mlbHit.War_Yr5_Bkt0 * fullYearHitBucket.War0) + (mlbHit.War_Yr5_Bkt1 * fullYearHitBucket.War1) + (mlbHit.War_Yr5_Bkt2 * fullYearHitBucket.War2) + (mlbHit.War_Yr5_Bkt3 * fullYearHitBucket.War3) +
                        (mlbHit.War_Yr5_Bkt4 * fullYearHitBucket.War4) + (mlbHit.War_Yr5_Bkt5 * fullYearHitBucket.War5) + (mlbHit.War_Yr5_Bkt6 * fullYearHitBucket.War6) + (mlbHit.War_Yr5_Bkt7 * fullYearHitBucket.War7);

                    mlbHit.Pa5 = (mlbHit.Pa_Yr5_Bkt0 * fullYearHitBucket.Pa0) + (mlbHit.Pa_Yr5_Bkt1 * fullYearHitBucket.Pa1) + (mlbHit.Pa_Yr5_Bkt2 * fullYearHitBucket.Pa2) + (mlbHit.Pa_Yr5_Bkt3 * fullYearHitBucket.Pa3) +
                        (mlbHit.Pa_Yr5_Bkt4 * fullYearHitBucket.Pa4) + (mlbHit.Pa_Yr5_Bkt5 * fullYearHitBucket.Pa5) + (mlbHit.Pa_Yr5_Bkt6 * fullYearHitBucket.Pa6);

                    mlbHit.War6 = (mlbHit.War_Yr6_Bkt0 * fullYearHitBucket.War0) + (mlbHit.War_Yr6_Bkt1 * fullYearHitBucket.War1) + (mlbHit.War_Yr6_Bkt2 * fullYearHitBucket.War2) + (mlbHit.War_Yr6_Bkt3 * fullYearHitBucket.War3) +
                        (mlbHit.War_Yr6_Bkt4 * fullYearHitBucket.War4) + (mlbHit.War_Yr6_Bkt5 * fullYearHitBucket.War5) + (mlbHit.War_Yr6_Bkt6 * fullYearHitBucket.War6) + (mlbHit.War_Yr6_Bkt7 * fullYearHitBucket.War7);

                    mlbHit.Pa6 = (mlbHit.Pa_Yr6_Bkt0 * fullYearHitBucket.Pa0) + (mlbHit.Pa_Yr6_Bkt1 * fullYearHitBucket.Pa1) + (mlbHit.Pa_Yr6_Bkt2 * fullYearHitBucket.Pa2) + (mlbHit.Pa_Yr6_Bkt3 * fullYearHitBucket.Pa3) +
                        (mlbHit.Pa_Yr6_Bkt4 * fullYearHitBucket.Pa4) + (mlbHit.Pa_Yr6_Bkt5 * fullYearHitBucket.Pa5) + (mlbHit.Pa_Yr6_Bkt6 * fullYearHitBucket.Pa6);

                    progressBar.Tick();
                }
            }

            modelDb.SaveChanges();
            modelDb.ChangeTracker.Clear();

            using (ProgressBar progressBar = new ProgressBar(modelDb.Output_PitcherMlbWarAggregation.Count(), "Fixing MLB Pitcher War Aggregation"))
            {
                foreach (var mlbPit in modelDb.Output_PitcherMlbWarAggregation)
                {
                    var month = mlbPit.Month == 9 ? 0 : mlbPit.Month;
                    var year0Bucket = pitcherDict[month];

                    mlbPit.WarSP0 = (mlbPit.WarSP_Yr0_Bkt0 * year0Bucket.WarSP0) + (mlbPit.WarSP_Yr0_Bkt1 * year0Bucket.WarSP1) + (mlbPit.WarSP_Yr0_Bkt2 * year0Bucket.WarSP2) + (mlbPit.WarSP_Yr0_Bkt3 * year0Bucket.WarSP3) +
                        (mlbPit.WarSP_Yr0_Bkt4 * year0Bucket.WarSP4) + (mlbPit.WarSP_Yr0_Bkt5 * year0Bucket.WarSP5) + (mlbPit.WarSP_Yr0_Bkt6 * year0Bucket.WarSP6) + (mlbPit.WarSP_Yr0_Bkt7 * year0Bucket.WarSP7);

                    mlbPit.OutsSP0 = (mlbPit.OutsSP_Yr0_Bkt0 * year0Bucket.OutsSP0) + (mlbPit.OutsSP_Yr0_Bkt1 * year0Bucket.OutsSP1) + (mlbPit.OutsSP_Yr0_Bkt2 * year0Bucket.OutsSP2) + (mlbPit.OutsSP_Yr0_Bkt3 * year0Bucket.OutsSP3) +
                        (mlbPit.OutsSP_Yr0_Bkt4 * year0Bucket.OutsSP4) + (mlbPit.OutsSP_Yr0_Bkt5 * year0Bucket.OutsSP5) + (mlbPit.OutsSP_Yr0_Bkt6 * year0Bucket.OutsSP6);

                    mlbPit.WarRP0 = (mlbPit.WarRP_Yr0_Bkt0 * year0Bucket.WarRP0) + (mlbPit.WarRP_Yr0_Bkt1 * year0Bucket.WarRP1) + (mlbPit.WarRP_Yr0_Bkt2 * year0Bucket.WarRP2) + (mlbPit.WarRP_Yr0_Bkt3 * year0Bucket.WarRP3) +
                        (mlbPit.WarRP_Yr0_Bkt4 * year0Bucket.WarRP4) + (mlbPit.WarRP_Yr0_Bkt5 * year0Bucket.WarRP5) + (mlbPit.WarRP_Yr0_Bkt6 * year0Bucket.WarRP6) + (mlbPit.WarRP_Yr0_Bkt7 * year0Bucket.WarRP7);

                    mlbPit.OutsRP0 = (mlbPit.OutsRP_Yr0_Bkt0 * year0Bucket.OutsRP0) + (mlbPit.OutsRP_Yr0_Bkt1 * year0Bucket.OutsRP1) + (mlbPit.OutsRP_Yr0_Bkt2 * year0Bucket.OutsRP2) + (mlbPit.OutsRP_Yr0_Bkt3 * year0Bucket.OutsRP3) +
                        (mlbPit.OutsRP_Yr0_Bkt4 * year0Bucket.OutsRP4) + (mlbPit.OutsRP_Yr0_Bkt5 * year0Bucket.OutsRP5) + (mlbPit.OutsRP_Yr0_Bkt6 * year0Bucket.OutsRP6);

                    mlbPit.WarSP1 = (mlbPit.WarSP_Yr1_Bkt0 * fullYearPitBucket.WarSP0) + (mlbPit.WarSP_Yr1_Bkt1 * fullYearPitBucket.WarSP1) + (mlbPit.WarSP_Yr1_Bkt2 * fullYearPitBucket.WarSP2) + (mlbPit.WarSP_Yr1_Bkt3 * fullYearPitBucket.WarSP3) +
                        (mlbPit.WarSP_Yr1_Bkt4 * fullYearPitBucket.WarSP4) + (mlbPit.WarSP_Yr1_Bkt5 * fullYearPitBucket.WarSP5) + (mlbPit.WarSP_Yr1_Bkt6 * fullYearPitBucket.WarSP6) + (mlbPit.WarSP_Yr1_Bkt7 * fullYearPitBucket.WarSP7);

                    mlbPit.OutsSP1 = (mlbPit.OutsSP_Yr1_Bkt0 * fullYearPitBucket.OutsSP0) + (mlbPit.OutsSP_Yr1_Bkt1 * fullYearPitBucket.OutsSP1) + (mlbPit.OutsSP_Yr1_Bkt2 * fullYearPitBucket.OutsSP2) + (mlbPit.OutsSP_Yr1_Bkt3 * fullYearPitBucket.OutsSP3) +
                        (mlbPit.OutsSP_Yr1_Bkt4 * fullYearPitBucket.OutsSP4) + (mlbPit.OutsSP_Yr1_Bkt5 * fullYearPitBucket.OutsSP5) + (mlbPit.OutsSP_Yr1_Bkt6 * fullYearPitBucket.OutsSP6);

                    mlbPit.WarRP1 = (mlbPit.WarRP_Yr1_Bkt0 * fullYearPitBucket.WarRP0) + (mlbPit.WarRP_Yr1_Bkt1 * fullYearPitBucket.WarRP1) + (mlbPit.WarRP_Yr1_Bkt2 * fullYearPitBucket.WarRP2) + (mlbPit.WarRP_Yr1_Bkt3 * fullYearPitBucket.WarRP3) +
                        (mlbPit.WarRP_Yr1_Bkt4 * fullYearPitBucket.WarRP4) + (mlbPit.WarRP_Yr1_Bkt5 * fullYearPitBucket.WarRP5) + (mlbPit.WarRP_Yr1_Bkt6 * fullYearPitBucket.WarRP6) + (mlbPit.WarRP_Yr1_Bkt7 * fullYearPitBucket.WarRP7);

                    mlbPit.OutsRP1 = (mlbPit.OutsRP_Yr1_Bkt0 * fullYearPitBucket.OutsRP0) + (mlbPit.OutsRP_Yr1_Bkt1 * fullYearPitBucket.OutsRP1) + (mlbPit.OutsRP_Yr1_Bkt2 * fullYearPitBucket.OutsRP2) + (mlbPit.OutsRP_Yr1_Bkt3 * fullYearPitBucket.OutsRP3) +
                        (mlbPit.OutsRP_Yr1_Bkt4 * fullYearPitBucket.OutsRP4) + (mlbPit.OutsRP_Yr1_Bkt5 * fullYearPitBucket.OutsRP5) + (mlbPit.OutsRP_Yr1_Bkt6 * fullYearPitBucket.OutsRP6);

                    mlbPit.WarSP2 = (mlbPit.WarSP_Yr2_Bkt0 * fullYearPitBucket.WarSP0) + (mlbPit.WarSP_Yr2_Bkt1 * fullYearPitBucket.WarSP1) + (mlbPit.WarSP_Yr2_Bkt2 * fullYearPitBucket.WarSP2) + (mlbPit.WarSP_Yr2_Bkt3 * fullYearPitBucket.WarSP3) +
                        (mlbPit.WarSP_Yr2_Bkt4 * fullYearPitBucket.WarSP4) + (mlbPit.WarSP_Yr2_Bkt5 * fullYearPitBucket.WarSP5) + (mlbPit.WarSP_Yr2_Bkt6 * fullYearPitBucket.WarSP6) + (mlbPit.WarSP_Yr2_Bkt7 * fullYearPitBucket.WarSP7);

                    mlbPit.OutsSP2 = (mlbPit.OutsSP_Yr2_Bkt0 * fullYearPitBucket.OutsSP0) + (mlbPit.OutsSP_Yr2_Bkt1 * fullYearPitBucket.OutsSP1) + (mlbPit.OutsSP_Yr2_Bkt2 * fullYearPitBucket.OutsSP2) + (mlbPit.OutsSP_Yr2_Bkt3 * fullYearPitBucket.OutsSP3) +
                        (mlbPit.OutsSP_Yr2_Bkt4 * fullYearPitBucket.OutsSP4) + (mlbPit.OutsSP_Yr2_Bkt5 * fullYearPitBucket.OutsSP5) + (mlbPit.OutsSP_Yr2_Bkt6 * fullYearPitBucket.OutsSP6);

                    mlbPit.WarRP2 = (mlbPit.WarRP_Yr2_Bkt0 * fullYearPitBucket.WarRP0) + (mlbPit.WarRP_Yr2_Bkt1 * fullYearPitBucket.WarRP1) + (mlbPit.WarRP_Yr2_Bkt2 * fullYearPitBucket.WarRP2) + (mlbPit.WarRP_Yr2_Bkt3 * fullYearPitBucket.WarRP3) +
                        (mlbPit.WarRP_Yr2_Bkt4 * fullYearPitBucket.WarRP4) + (mlbPit.WarRP_Yr2_Bkt5 * fullYearPitBucket.WarRP5) + (mlbPit.WarRP_Yr2_Bkt6 * fullYearPitBucket.WarRP6) + (mlbPit.WarRP_Yr2_Bkt7 * fullYearPitBucket.WarRP7);

                    mlbPit.OutsRP2 = (mlbPit.OutsRP_Yr2_Bkt0 * fullYearPitBucket.OutsRP0) + (mlbPit.OutsRP_Yr2_Bkt1 * fullYearPitBucket.OutsRP1) + (mlbPit.OutsRP_Yr2_Bkt2 * fullYearPitBucket.OutsRP2) + (mlbPit.OutsRP_Yr2_Bkt3 * fullYearPitBucket.OutsRP3) +
                        (mlbPit.OutsRP_Yr2_Bkt4 * fullYearPitBucket.OutsRP4) + (mlbPit.OutsRP_Yr2_Bkt5 * fullYearPitBucket.OutsRP5) + (mlbPit.OutsRP_Yr2_Bkt6 * fullYearPitBucket.OutsRP6);

                    mlbPit.WarSP3 = (mlbPit.WarSP_Yr3_Bkt0 * fullYearPitBucket.WarSP0) + (mlbPit.WarSP_Yr3_Bkt1 * fullYearPitBucket.WarSP1) + (mlbPit.WarSP_Yr3_Bkt2 * fullYearPitBucket.WarSP2) + (mlbPit.WarSP_Yr3_Bkt3 * fullYearPitBucket.WarSP3) +
                        (mlbPit.WarSP_Yr3_Bkt4 * fullYearPitBucket.WarSP4) + (mlbPit.WarSP_Yr3_Bkt5 * fullYearPitBucket.WarSP5) + (mlbPit.WarSP_Yr3_Bkt6 * fullYearPitBucket.WarSP6) + (mlbPit.WarSP_Yr3_Bkt7 * fullYearPitBucket.WarSP7);

                    mlbPit.OutsSP3 = (mlbPit.OutsSP_Yr3_Bkt0 * fullYearPitBucket.OutsSP0) + (mlbPit.OutsSP_Yr3_Bkt1 * fullYearPitBucket.OutsSP1) + (mlbPit.OutsSP_Yr3_Bkt2 * fullYearPitBucket.OutsSP2) + (mlbPit.OutsSP_Yr3_Bkt3 * fullYearPitBucket.OutsSP3) +
                        (mlbPit.OutsSP_Yr3_Bkt4 * fullYearPitBucket.OutsSP4) + (mlbPit.OutsSP_Yr3_Bkt5 * fullYearPitBucket.OutsSP5) + (mlbPit.OutsSP_Yr3_Bkt6 * fullYearPitBucket.OutsSP6);

                    mlbPit.WarRP3 = (mlbPit.WarRP_Yr3_Bkt0 * fullYearPitBucket.WarRP0) + (mlbPit.WarRP_Yr3_Bkt1 * fullYearPitBucket.WarRP1) + (mlbPit.WarRP_Yr3_Bkt2 * fullYearPitBucket.WarRP2) + (mlbPit.WarRP_Yr3_Bkt3 * fullYearPitBucket.WarRP3) +
                        (mlbPit.WarRP_Yr3_Bkt4 * fullYearPitBucket.WarRP4) + (mlbPit.WarRP_Yr3_Bkt5 * fullYearPitBucket.WarRP5) + (mlbPit.WarRP_Yr3_Bkt6 * fullYearPitBucket.WarRP6) + (mlbPit.WarRP_Yr3_Bkt7 * fullYearPitBucket.WarRP7);

                    mlbPit.OutsRP3 = (mlbPit.OutsRP_Yr3_Bkt0 * fullYearPitBucket.OutsRP0) + (mlbPit.OutsRP_Yr3_Bkt1 * fullYearPitBucket.OutsRP1) + (mlbPit.OutsRP_Yr3_Bkt2 * fullYearPitBucket.OutsRP2) + (mlbPit.OutsRP_Yr3_Bkt3 * fullYearPitBucket.OutsRP3) +
                        (mlbPit.OutsRP_Yr3_Bkt4 * fullYearPitBucket.OutsRP4) + (mlbPit.OutsRP_Yr3_Bkt5 * fullYearPitBucket.OutsRP5) + (mlbPit.OutsRP_Yr3_Bkt6 * fullYearPitBucket.OutsRP6);

                    mlbPit.WarSP4 = (mlbPit.WarSP_Yr4_Bkt0 * fullYearPitBucket.WarSP0) + (mlbPit.WarSP_Yr4_Bkt1 * fullYearPitBucket.WarSP1) + (mlbPit.WarSP_Yr4_Bkt2 * fullYearPitBucket.WarSP2) + (mlbPit.WarSP_Yr4_Bkt3 * fullYearPitBucket.WarSP3) +
                        (mlbPit.WarSP_Yr4_Bkt4 * fullYearPitBucket.WarSP4) + (mlbPit.WarSP_Yr4_Bkt5 * fullYearPitBucket.WarSP5) + (mlbPit.WarSP_Yr4_Bkt6 * fullYearPitBucket.WarSP6) + (mlbPit.WarSP_Yr4_Bkt7 * fullYearPitBucket.WarSP7);

                    mlbPit.OutsSP4 = (mlbPit.OutsSP_Yr4_Bkt0 * fullYearPitBucket.OutsSP0) + (mlbPit.OutsSP_Yr4_Bkt1 * fullYearPitBucket.OutsSP1) + (mlbPit.OutsSP_Yr4_Bkt2 * fullYearPitBucket.OutsSP2) + (mlbPit.OutsSP_Yr4_Bkt3 * fullYearPitBucket.OutsSP3) +
                        (mlbPit.OutsSP_Yr4_Bkt4 * fullYearPitBucket.OutsSP4) + (mlbPit.OutsSP_Yr4_Bkt5 * fullYearPitBucket.OutsSP5) + (mlbPit.OutsSP_Yr4_Bkt6 * fullYearPitBucket.OutsSP6);

                    mlbPit.WarRP4 = (mlbPit.WarRP_Yr4_Bkt0 * fullYearPitBucket.WarRP0) + (mlbPit.WarRP_Yr4_Bkt1 * fullYearPitBucket.WarRP1) + (mlbPit.WarRP_Yr4_Bkt2 * fullYearPitBucket.WarRP2) + (mlbPit.WarRP_Yr4_Bkt3 * fullYearPitBucket.WarRP3) +
                        (mlbPit.WarRP_Yr4_Bkt4 * fullYearPitBucket.WarRP4) + (mlbPit.WarRP_Yr4_Bkt5 * fullYearPitBucket.WarRP5) + (mlbPit.WarRP_Yr4_Bkt6 * fullYearPitBucket.WarRP6) + (mlbPit.WarRP_Yr4_Bkt7 * fullYearPitBucket.WarRP7);

                    mlbPit.OutsRP4 = (mlbPit.OutsRP_Yr4_Bkt0 * fullYearPitBucket.OutsRP0) + (mlbPit.OutsRP_Yr4_Bkt1 * fullYearPitBucket.OutsRP1) + (mlbPit.OutsRP_Yr4_Bkt2 * fullYearPitBucket.OutsRP2) + (mlbPit.OutsRP_Yr4_Bkt3 * fullYearPitBucket.OutsRP3) +
                        (mlbPit.OutsRP_Yr4_Bkt4 * fullYearPitBucket.OutsRP4) + (mlbPit.OutsRP_Yr4_Bkt5 * fullYearPitBucket.OutsRP5) + (mlbPit.OutsRP_Yr4_Bkt6 * fullYearPitBucket.OutsRP6);

                    mlbPit.WarSP5 = (mlbPit.WarSP_Yr5_Bkt0 * fullYearPitBucket.WarSP0) + (mlbPit.WarSP_Yr5_Bkt1 * fullYearPitBucket.WarSP1) + (mlbPit.WarSP_Yr5_Bkt2 * fullYearPitBucket.WarSP2) + (mlbPit.WarSP_Yr5_Bkt3 * fullYearPitBucket.WarSP3) +
                        (mlbPit.WarSP_Yr5_Bkt4 * fullYearPitBucket.WarSP4) + (mlbPit.WarSP_Yr5_Bkt5 * fullYearPitBucket.WarSP5) + (mlbPit.WarSP_Yr5_Bkt6 * fullYearPitBucket.WarSP6) + (mlbPit.WarSP_Yr5_Bkt7 * fullYearPitBucket.WarSP7);

                    mlbPit.OutsSP5 = (mlbPit.OutsSP_Yr5_Bkt0 * fullYearPitBucket.OutsSP0) + (mlbPit.OutsSP_Yr5_Bkt1 * fullYearPitBucket.OutsSP1) + (mlbPit.OutsSP_Yr5_Bkt2 * fullYearPitBucket.OutsSP2) + (mlbPit.OutsSP_Yr5_Bkt3 * fullYearPitBucket.OutsSP3) +
                        (mlbPit.OutsSP_Yr5_Bkt4 * fullYearPitBucket.OutsSP4) + (mlbPit.OutsSP_Yr5_Bkt5 * fullYearPitBucket.OutsSP5) + (mlbPit.OutsSP_Yr5_Bkt6 * fullYearPitBucket.OutsSP6);

                    mlbPit.WarRP5 = (mlbPit.WarRP_Yr5_Bkt0 * fullYearPitBucket.WarRP0) + (mlbPit.WarRP_Yr5_Bkt1 * fullYearPitBucket.WarRP1) + (mlbPit.WarRP_Yr5_Bkt2 * fullYearPitBucket.WarRP2) + (mlbPit.WarRP_Yr5_Bkt3 * fullYearPitBucket.WarRP3) +
                        (mlbPit.WarRP_Yr5_Bkt4 * fullYearPitBucket.WarRP4) + (mlbPit.WarRP_Yr5_Bkt5 * fullYearPitBucket.WarRP5) + (mlbPit.WarRP_Yr5_Bkt6 * fullYearPitBucket.WarRP6) + (mlbPit.WarRP_Yr5_Bkt7 * fullYearPitBucket.WarRP7);

                    mlbPit.OutsRP5 = (mlbPit.OutsRP_Yr5_Bkt0 * fullYearPitBucket.OutsRP0) + (mlbPit.OutsRP_Yr5_Bkt1 * fullYearPitBucket.OutsRP1) + (mlbPit.OutsRP_Yr5_Bkt2 * fullYearPitBucket.OutsRP2) + (mlbPit.OutsRP_Yr5_Bkt3 * fullYearPitBucket.OutsRP3) +
                        (mlbPit.OutsRP_Yr5_Bkt4 * fullYearPitBucket.OutsRP4) + (mlbPit.OutsRP_Yr5_Bkt5 * fullYearPitBucket.OutsRP5) + (mlbPit.OutsRP_Yr5_Bkt6 * fullYearPitBucket.OutsRP6);

                    mlbPit.WarSP6 = (mlbPit.WarSP_Yr6_Bkt0 * fullYearPitBucket.WarSP0) + (mlbPit.WarSP_Yr6_Bkt1 * fullYearPitBucket.WarSP1) + (mlbPit.WarSP_Yr6_Bkt2 * fullYearPitBucket.WarSP2) + (mlbPit.WarSP_Yr6_Bkt3 * fullYearPitBucket.WarSP3) +
                        (mlbPit.WarSP_Yr6_Bkt4 * fullYearPitBucket.WarSP4) + (mlbPit.WarSP_Yr6_Bkt5 * fullYearPitBucket.WarSP5) + (mlbPit.WarSP_Yr6_Bkt6 * fullYearPitBucket.WarSP6) + (mlbPit.WarSP_Yr6_Bkt7 * fullYearPitBucket.WarSP7);

                    mlbPit.OutsSP6 = (mlbPit.OutsSP_Yr6_Bkt0 * fullYearPitBucket.OutsSP0) + (mlbPit.OutsSP_Yr6_Bkt1 * fullYearPitBucket.OutsSP1) + (mlbPit.OutsSP_Yr6_Bkt2 * fullYearPitBucket.OutsSP2) + (mlbPit.OutsSP_Yr6_Bkt3 * fullYearPitBucket.OutsSP3) +
                        (mlbPit.OutsSP_Yr6_Bkt4 * fullYearPitBucket.OutsSP4) + (mlbPit.OutsSP_Yr6_Bkt5 * fullYearPitBucket.OutsSP5) + (mlbPit.OutsSP_Yr6_Bkt6 * fullYearPitBucket.OutsSP6);

                    mlbPit.WarRP6 = (mlbPit.WarRP_Yr6_Bkt0 * fullYearPitBucket.WarRP0) + (mlbPit.WarRP_Yr6_Bkt1 * fullYearPitBucket.WarRP1) + (mlbPit.WarRP_Yr6_Bkt2 * fullYearPitBucket.WarRP2) + (mlbPit.WarRP_Yr6_Bkt3 * fullYearPitBucket.WarRP3) +
                        (mlbPit.WarRP_Yr6_Bkt4 * fullYearPitBucket.WarRP4) + (mlbPit.WarRP_Yr6_Bkt5 * fullYearPitBucket.WarRP5) + (mlbPit.WarRP_Yr6_Bkt6 * fullYearPitBucket.WarRP6) + (mlbPit.WarRP_Yr6_Bkt7 * fullYearPitBucket.WarRP7);

                    mlbPit.OutsRP6 = (mlbPit.OutsRP_Yr6_Bkt0 * fullYearPitBucket.OutsRP0) + (mlbPit.OutsRP_Yr6_Bkt1 * fullYearPitBucket.OutsRP1) + (mlbPit.OutsRP_Yr6_Bkt2 * fullYearPitBucket.OutsRP2) + (mlbPit.OutsRP_Yr6_Bkt3 * fullYearPitBucket.OutsRP3) +
                        (mlbPit.OutsRP_Yr6_Bkt4 * fullYearPitBucket.OutsRP4) + (mlbPit.OutsRP_Yr6_Bkt5 * fullYearPitBucket.OutsRP5) + (mlbPit.OutsRP_Yr6_Bkt6 * fullYearPitBucket.OutsRP6);

                    progressBar.Tick();
                }
            }

            modelDb.SaveChanges();
            modelDb.ChangeTracker.Clear();
        }
    }
}
