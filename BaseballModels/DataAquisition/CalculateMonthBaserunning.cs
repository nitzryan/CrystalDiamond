using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateMonthBaserunning
    {
        public static bool Update(int year, int month)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Database.ExecuteSqlRaw(
                    "DELETE FROM Player_Hitter_MonthBaserunning WHERE Year = {0} AND Month = {1}",
                    year, month);

                List<GamePlayByPlay> monthPBP;
                if (month == 4)
                    monthPBP = db.GamePlayByPlay.Where(f => f.Year == year && f.Month <= 4).AsNoTracking().ToList();
                else if (month == 9)
                    monthPBP = db.GamePlayByPlay.Where(f => f.Year == year && f.Month >= 9).AsNoTracking().ToList();
                else
                    monthPBP = db.GamePlayByPlay.Where(f => f.Year == year && f.Month == month).AsNoTracking().ToList();

                var leagues = db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month)
                                    .Select(f => f.LeagueId).Distinct();

                List<Player_Hitter_MonthBaserunning> output = new(db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month).Count());
                using (ProgressBar progressBar = new ProgressBar(leagues.Count(), $"Generating Baserunning Stats for {year}-{month}"))
                {
                    foreach (var league in leagues)
                    {
                        var leaguePBP = monthPBP.Where(f => f.LeagueId == league).ToList();
                        LeagueStats leagueStats = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == league).Single();
                        var monthStats = db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month && f.LeagueId == league).ToList();

                        LeagueRunMatrix lrm = db.LeagueRunMatrix.Where(f => f.Year == year && f.LeagueId == league).Single();
                        BaserunningDict BsrAdv1st3rdSingleDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAdv1st3rdSingleDict);
                        BaserunningDict BsrAdv2ndHomeSingleDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAdv2ndHomeSingleDict);
                        BaserunningDict BsrAdv1stHomeDoubleDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAdv1stHomeDoubleDict);
                        BaserunningDict BsrAvoidForce2ndDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAvoidForce2ndDict);
                        BaserunningDict BsrAdv1st2ndFlyoutDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAdv1st2ndFlyoutDict);
                        BaserunningDict BsrAdv2nd3rdFlyoutDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAdv2nd3rdFlyoutDict);
                        BaserunningDict BsrAdv3rdHomeFlyoutDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAdv3rdHomeFlyoutDict);
                        BaserunningDict BsrAdv2nd3rdGroundoutDict = LeagueRunMatrixDicts.GetBaserunningDict(lrm.BsrAdv2nd3rdGroundoutDict);

                        foreach (var stat in monthStats)
                        { 
                            var hitterPBP = leaguePBP.Where(f => f.HitterId == stat.MlbId).ToArray();
                            var onFirstPBP = leaguePBP.Where(f => f.Run1stId == stat.MlbId).ToArray();
                            var onSecondPBP = leaguePBP.Where(f => f.Run2ndId == stat.MlbId).ToArray();
                            var onThirdPBP = leaguePBP.Where(f => f.Run3rdId == stat.MlbId).ToArray();

                            // Calculate Runs from stealing bases
                            float rSB = (leagueStats.RunSB * stat.SB) + (leagueStats.RunCS * stat.CS);
                            int sbChances = stat.H - stat.Hit2B - stat.Hit3B - stat.HR + stat.BB + stat.HBP;

                            // Calculate runs from hitting into/avoiding double plays
                            var gidpChanceEvents = PBP_Utilities.GetDoublePlayOpportunities(hitterPBP);
                            int gidpChances = gidpChanceEvents.Count();
                            int gidp = gidpChanceEvents.Where(f => f.Result.HasFlag(DbEnums.PBP_Events.GIDP)).Count();
                            float gidpRunsAdded = gidp * leagueStats.RunGIDP;
                            float expectedRunsAdded = gidpChances * leagueStats.ProbGIDP * leagueStats.RunGIDP;
                            float rGIDP = gidpRunsAdded - expectedRunsAdded;

                            // Calculate runs from player advancing/getting thrown out on bases
                            float runnerAvoidForceoutRuns = PBP_Utilities.GetScenarioRunsScored(onFirstPBP, BsrAvoidForce2ndDict, PBP_Utilities.Avoid_1stToSecondForceout_Opportunities, 1, 2);
                            float single1st3rdRuns = PBP_Utilities.GetScenarioRunsScored(onFirstPBP, BsrAdv1st3rdSingleDict, PBP_Utilities.GetAdvance_1stTo3rdOnSingle_Opportunities, 1, 3);
                            float single2ndHomeRuns = PBP_Utilities.GetScenarioRunsScored(onSecondPBP, BsrAdv2ndHomeSingleDict, PBP_Utilities.GetAdvance_2ndToHomeOnSingle_Opportunities, 2, 4);
                            float double1stHomeRuns = PBP_Utilities.GetScenarioRunsScored(onFirstPBP, BsrAdv1stHomeDoubleDict, PBP_Utilities.GetAdvance_1stToHomeOnDouble_Opportunities, 1, 4);
                            float groundout2nd3rdRuns = PBP_Utilities.GetScenarioRunsScored(onSecondPBP, BsrAdv2nd3rdGroundoutDict, PBP_Utilities.GetAdvance_2ndTo3rdOnGroundout_Opportunities, 2, 3);
                            float flyout1st2ndRuns = PBP_Utilities.GetScenarioRunsScored(onFirstPBP, BsrAdv1st2ndFlyoutDict, PBP_Utilities.GetAdvance_1stTo2ndOnFlyout_Opportunities, 1, 2);
                            float flyout2nd3rdRuns = PBP_Utilities.GetScenarioRunsScored(onSecondPBP, BsrAdv2nd3rdFlyoutDict, PBP_Utilities.GetAdvance_2ndTo3rdOnFlyout_Opportunities, 2, 3);
                            float flyout3rdHomeRuns = PBP_Utilities.GetScenarioRunsScored(onThirdPBP, BsrAdv3rdHomeFlyoutDict, PBP_Utilities.GetAdvance_3rdToHomeOnFlyout_Opportunities, 3, 4);
                            float advanceRuns = runnerAvoidForceoutRuns + single1st3rdRuns + single2ndHomeRuns +
                                double1stHomeRuns + groundout2nd3rdRuns + flyout1st2ndRuns + flyout2nd3rdRuns + flyout3rdHomeRuns;

                            output.Add(new Player_Hitter_MonthBaserunning
                            {
                                MlbId = stat.MlbId,
                                Year = year,
                                Month = month,
                                LevelId = stat.LevelId,
                                LeagueId = stat.LeagueId,
                                RSB = rSB,
                                RUBR = advanceRuns,
                                RGIDP = rGIDP,
                                TimesOnFirst = stat.H + stat.BB + stat.HBP - stat.Hit2B - stat.Hit3B - stat.HR,
                                TimesOnBase = stat.H + stat.BB + stat.HBP - stat.HR
                            });
                        }
                        progressBar.Tick();
                    }
                }

                db.BulkInsert(output);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateMonthBaserunning");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
