using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ModelDb;
using ShellProgressBar;
using SiteDb;

namespace SitePrep
{
    internal class DraftRankings
    {
        private record DraftValue
        {
            public int tbcId;
            public float value;
            public bool isHitter;
            public bool isEligible;
        }

        // All players with 3 years are eligible, 1 are not, 2 are eligible if 21 within 45 days of draft
        private static bool IsPlayerEligible(College_Player player, int year, int expYears)
        {
            if (expYears >= 3)
                return true;
            if (expYears <= 1)
                return false;

            // Get cutoff date
            int cutoffMonth = -1;
            int cutoffDay = -1;

            switch (year)
            {
                case 2004:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2005:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2006:
                    cutoffMonth = 7;
                    cutoffDay = 22;
                    break;
                case 2007:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2008:
                    cutoffMonth = 7;
                    cutoffDay = 21;
                    break;
                case 2009:
                    cutoffMonth = 7;
                    cutoffDay = 26;
                    break;
                case 2010:
                    cutoffMonth = 7;
                    cutoffDay = 24;
                    break;
                case 2011:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2012:
                    cutoffMonth = 7;
                    cutoffDay = 21;
                    break;
                case 2013:
                    cutoffMonth = 7;
                    cutoffDay = 23;
                    break;
                case 2014:
                    cutoffMonth = 7;
                    cutoffDay = 22;
                    break;
                case 2015:
                    cutoffMonth = 7;
                    cutoffDay = 25;
                    break;
                case 2016:
                    cutoffMonth = 7;
                    cutoffDay = 26;
                    break;
                case 2017:
                    cutoffMonth = 7;
                    cutoffDay = 29;
                    break;
                case 2018:
                    cutoffMonth = 7;
                    cutoffDay = 21;
                    break;
                case 2019:
                    cutoffMonth = 7;
                    cutoffDay = 20;
                    break;
                case 2020:
                    cutoffMonth = 7;
                    cutoffDay = 26;
                    break;
                case 2021:
                    cutoffMonth = 8;
                    cutoffDay = 27;
                    break;
                case 2022:
                    cutoffMonth = 9;
                    cutoffDay = 1;
                    break;
                default: // Fixed August 1 rule applies onward until MLB changes it
                    cutoffMonth = 8;
                    cutoffDay = 1;
                    break;
            }

            float age = Utilities.GetAge1MinusAge0(year, cutoffMonth, cutoffDay, player.BirthYear, player.BirthMonth, player.BirthDay);
            return age >= 21;
        }

        public static void Update()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            siteDb.DraftRank.ExecuteDelete();

            var modelYears = modelDb.Output_College_HitterAggregation.Where(f => f.Year >= 2005)
                .Select(f => new { f.Year, f.Model })
                .Distinct();

            using (ProgressBar progressBar = new ProgressBar(modelYears.Count(), "Generating Draft Rankings"))
            {
                foreach (var modelYear in modelYears)
                {
                    // Get all players ranked
                    var hitterStats = modelDb.Output_College_HitterAggregation
                        .Where(f => f.Year == modelYear.Year && f.Model == modelYear.Model);
                    var pitcherStats = modelDb.Output_College_PitcherAggregation
                        .Where(f => f.Year == modelYear.Year && f.Model == modelYear.Model);

                    // Get DraftValue for each hitter, pitcher in list
                    List<DraftValue> playerValues = new();
                    playerValues.Capacity = hitterStats.Count() + pitcherStats.Count();
                    foreach (var hs in hitterStats)
                    {
                        College_Player cp = db.College_Player.Where(f => f.TBCId == hs.TbcId).Single();
                        Model_College_HitterYear hitStats = db.Model_College_HitterYear.Where(f => f.TBCId == hs.TbcId && f.Year == hs.Year).Single();

                        playerValues.Add(new DraftValue
                        {
                            tbcId = hs.TbcId,
                            value = hs.War,
                            isHitter = true,
                            isEligible = IsPlayerEligible(cp, hs.Year, hitStats.ExpYears),
                        });
                    }
                    foreach (var ps in pitcherStats)
                    {
                        College_Player cp = db.College_Player.Where(f => f.TBCId == ps.TbcId).Single();
                        Model_College_PitcherYear pitStats = db.Model_College_PitcherYear.Where(f => f.TBCId == ps.TbcId && f.Year == ps.Year).Single();

                        playerValues.Add(new DraftValue
                        {
                            tbcId = ps.TbcId,
                            value = ps.War,
                            isHitter = false,
                            isEligible = IsPlayerEligible(cp, ps.Year, pitStats.ExpYears),
                        });
                    }

                    // Order draft-eligible ones
                    List<DraftRank> draftRanks = new();
                    draftRanks.Capacity = playerValues.Count;
                    var draftRankings = playerValues
                        .Where(f => f.isEligible)
                        .OrderByDescending(f => f.value);

                    int rank = 1;
                    foreach (var dr in draftRankings)
                    {
                        draftRanks.Add(new DraftRank
                        {
                            TbcId = dr.tbcId,
                            ModelId = modelYear.Model,
                            IsHitter = dr.isHitter,
                            Year = modelYear.Year,
                            IsEligible = true,
                            RankEligible = rank,
                            Value=dr.value,
                        });
                        rank++;
                    }

                    // Non-eligible rankings included for player history
                    var ineligiblePlayers = playerValues.Where(f => !f.isEligible);
                    foreach (var ip in ineligiblePlayers)
                    {
                        draftRanks.Add(new DraftRank
                        {
                            TbcId = ip.tbcId,
                            ModelId = modelYear.Model,
                            IsHitter = ip.isHitter,
                            Year = modelYear.Year,
                            IsEligible = false,
                            RankEligible = -1,
                            Value = ip.value,
                        });
                    }

                    siteDb.BulkInsert(draftRanks);

                    progressBar.Tick();
                }
            }
        }
    }
}
