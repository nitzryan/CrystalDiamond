using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateAnnualStats
    {
        private static void CreateBaserunningYearStats(SqliteDbContext db, int year)
        {
            db.Player_Hitter_YearBaserunning.Where(f => f.Year == year).ExecuteDelete();

            var playerTeams = db.Player_Hitter_MonthBaserunning.Where(f => f.Year == year)
                .GroupBy(f => new { f.MlbId, f.TeamId });

            int yearStatsCount = playerTeams.Count();

            List<Player_Hitter_YearBaserunning> output = new(yearStatsCount);
            foreach (var pt in playerTeams)
            {
                Player_Hitter_MonthBaserunning[] stats = pt.ToArray();
                output.Add(new Player_Hitter_YearBaserunning
                {
                    MlbId = pt.Key.MlbId,
                    Year = year,
                    LevelId = stats[0].LevelId,
                    LeagueId = stats[0].LeagueId,
                    TeamId = pt.Key.TeamId,
                    RSB = stats.Sum(f => f.RSB),
                    RSBNorm = stats.Sum(f => f.RSBNorm),
                    RUBR = stats.Sum(f => f.RUBR),
                    RGIDP = stats.Sum(f => f.RGIDP),
                    RBSR = stats.Sum(f => f.RBSR),
                    TimesOnBase = stats.Sum(f => f.TimesOnBase),
                    TimesOnFirst = stats.Sum(f => f.TimesOnFirst)
                });
            }

            db.BulkInsert(output);
        }

        private static void CreateFieldingYearStats(SqliteDbContext db, int year)
        {
            db.Player_Fielder_YearStats.Where(f => f.Year == year).ExecuteDelete();

            var playerTeamPositions = db.Player_Fielder_MonthStats.Where(f => f.Year == year)
                .GroupBy(f => new { f.MlbId, f.TeamId, f.Position });

            int yearStatsCount = playerTeamPositions.Count();

            List<Player_Fielder_YearStats> output = new(yearStatsCount);
            foreach (var ptp in playerTeamPositions)
            {
                Player_Fielder_MonthStats[] stats = ptp.ToArray();
                output.Add(new Player_Fielder_YearStats
                {
                    MlbId = ptp.Key.MlbId,
                    Year = year,
                    LevelId = stats[0].LevelId,
                    LeagueId = stats[0].LeagueId,
                    TeamId = ptp.Key.TeamId,
                    Position = ptp.Key.Position,
                    Chances = stats.Sum(f => f.Chances),
                    Errors = stats.Sum(f => f.Errors),
                    ThrowErrors = stats.Sum(f => f.ThrowErrors),
                    Outs = stats.Sum(f => f.Outs),
                    R_ERR = stats.Sum(f => f.R_ERR),
                    R_PM = stats.Sum(f => f.R_PM),
                    PosAdjust = stats.Sum(f => f.PosAdjust),
                    D_RAA = stats.Sum(f => f.D_RAA),
                    ScaledDRAA = -100000, // Will get set later
                    R_GIDP = stats.Sum(f => f.R_GIDP),
                    R_ARM = stats.Sum(f => f.R_ARM),
                    R_SB = stats.Sum(f => f.R_SB),
                    SB = stats.Sum(f => f.SB),
                    CS = stats.Sum(f => f.CS),
                    R_PB = stats.Sum(f => f.R_PB),
                    PB = stats.Sum(f => f.PB)
                });
            }

            db.BulkInsert(output);
        }

        private static void CreateHittingYearStats(SqliteDbContext db, int year)
        {
            db.Player_Hitter_YearAdvanced.Where(f => f.Year == year).ExecuteDelete();
            var hitterData = db.Player_Hitter_GameLog.Where(f => f.Year == year)
                .GroupBy(f => new { f.MlbId, f.TeamId });

            Dictionary<int, LeagueStats> leagueDict = new();
            foreach (int league in db.Player_Hitter_MonthStats.Where(f => f.Year == year).Select(f => f.LeagueId).Distinct())
            {
                LeagueStats ls = db.LeagueStats.Where(f => f.LeagueId == league && f.Year == year).Single();
                leagueDict.Add(league, ls);
            }

            int yearStatsCount = hitterData.Count();
            List<Player_Hitter_YearAdvanced> output = new(yearStatsCount);
            foreach (var games in hitterData)
            {
                var s = games.Aggregate(Utilities.HitterGameLogAggregation);
                var (parkFactor, parkHRFactor) = Utilities.GetParkFactors(games, db);

                int pa = s.PA;
                float iso = s.AB > 0 ? (float)(s.Hit2B + (2 * s.Hit3B) + (3 * s.HR)) / s.AB : 0;
                float avg = s.AB > 0 ? (float)s.H / s.AB : 0;
                int singles = s.H - s.Hit2B - s.Hit3B - s.HR;
                int leagueId = games.First().LeagueId;

                LeagueStats ls = leagueDict[leagueId];
                float woba = Utilities.CalculateWOBA(ls, s.HBP, s.BB, singles, s.Hit2B, s.Hit3B, s.HR, s.PA);
                output.Add(new Player_Hitter_YearAdvanced
                {
                    MlbId = games.Key.MlbId,
                    LevelId = games.First().LevelId,
                    Year = year,
                    TeamId = games.Key.TeamId,
                    LeagueId = leagueId,
                    ParkFactor = parkFactor,
                    PA = pa,
                    AVG = avg,
                    OBP = pa > 0 ? (float)(s.H + s.BB + s.HBP) / pa : 0,
                    SLG = avg + iso,
                    ISO = iso,
                    WOBA = woba,
                    WRC = -1,
                    HR = s.HR,
                    BBPerc = pa > 0 ? (float)s.BB / pa : 0,
                    KPerc = pa > 0 ? (float)s.K / pa : 0,
                    SB = s.SB,
                    CS = s.CS
                });
            }

            db.BulkInsert(output);
        }

        private static void CreatePitchingYearStats(SqliteDbContext db, int year)
        {
            db.Player_Pitcher_YearAdvanced.Where(f => f.Year == year).ExecuteDelete();

            var pitcherData = db.Player_Pitcher_GameLog.Where(f => f.Year == year)
                .GroupBy(f => new { f.MlbId, f.TeamId });

            Dictionary<int, LeagueStats> leagueDict = new();
            foreach (int league in db.Player_Hitter_MonthStats.Where(f => f.Year == year).Select(f => f.LeagueId).Distinct())
            {
                LeagueStats ls = db.LeagueStats.Where(f => f.LeagueId == league && f.Year == year).Single();
                leagueDict.Add(league, ls);
            }

            int yearStatsCount = pitcherData.Count();
            List<Player_Pitcher_YearAdvanced> output = new(yearStatsCount);
            foreach (var games in pitcherData)
            {
                var s = games.Aggregate(Utilities.PitcherGameLogAggregation);
                var (parkFactor, parkHRFactor) = Utilities.GetParkFactors(games, db);
                int outsSP = games.Where(f => f.Started == 1).Sum(f => f.Outs);

                int leagueId = games.First().LeagueId;
                LeagueStats ls = leagueDict[leagueId];

                int pa = s.BattersFaced;
                int singles = s.H - s.Hit2B - s.Hit3B - s.HR;
                float woba = Utilities.CalculateWOBA(ls, s.HBP, s.BB, singles, s.Hit2B, s.Hit3B, s.HR, pa);
                int outs = s.Outs > 0 ? s.Outs : 1;

                float era = (float)s.ER * 27 / outs;
                float fip = Utilities.CalculateFip(ls.CFIP, s.HR, s.K, s.BB + s.HBP, s.Outs);
                output.Add(new Player_Pitcher_YearAdvanced
                {
                    MlbId = games.Key.MlbId,
                    LevelId = games.First().LevelId,
                    Year = year,
                    TeamId = games.Key.TeamId,
                    LeagueId = leagueId,
                    SPPerc = s.Outs > 0 ? (float)(outsSP) / s.Outs : 0.5f,
                    BF = s.BattersFaced,
                    Outs = s.Outs,
                    GBRatio = s.AO > 0 ? (float)s.GO / (s.GO + s.AO) : 1,
                    ERA = era,
                    FIP = fip,
                    ERAMinus = ((2 - parkFactor) * era) / ls.LeagueERA * 100,
                    FIPMinus = ((2 - parkFactor) * fip) / ls.LeagueERA * 100,
                    WOBA = woba,
                    HR = s.HR,
                    BBPerc = pa > 0 ? (float)s.BB / pa : 0,
                    KPerc = pa > 0 ? (float)s.K / pa : 0,
                });
            }
            db.BulkInsert(output);
        }

        public static bool Main(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                
                using (ProgressBar progressBar = new ProgressBar(4, $"Calculating Annual stats for Year={year}"))
                {
                    CreateHittingYearStats(db, year);
                    progressBar.Tick();
                    CreatePitchingYearStats(db, year);
                    progressBar.Tick();
                    CreateBaserunningYearStats(db, year);
                    progressBar.Tick();
                    CreateFieldingYearStats(db, year);
                    progressBar.Tick();
                }

                return true;
            } catch (Exception e)
            {
                Console.WriteLine("Error in CalculateAnnualStats");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
