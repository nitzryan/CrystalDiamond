using Db;

namespace DataAquisition
{
    internal class Utilities
    {
        public static Player_Hitter_MonthAdvanced HitterNormalToAdvanced(Player_Hitter_MonthStats stats)
        {
            int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;
            int pa = stats.PA;
            float avg = stats.AB > 0 ? (float)stats.H / stats.AB : 0.0f;
            float iso = stats.AB > 0 ? (float)(stats.Hit2B + (2 * stats.Hit3B) + (3 * stats.HR)) / stats.AB : 0;
            float woba = pa > 0 ?
                ((0.69f * stats.BB) + (0.72f * stats.HBP) + (0.89f * singles) + (1.27f * stats.Hit2B) + (1.62f * stats.Hit3B) + (2.10f * stats.HR)) / pa
                : 0;
            Player_Hitter_MonthAdvanced ma = new()
            {
                MlbId = stats.MlbId,
                LevelId = stats.LevelId,
                Year = stats.Year,
                Month = stats.Month,
                TeamId = -1, // Needs to get entered elsewhere, but not needed unless submitting to db
                LeagueId = -1,
                PA = pa,
                AVG = avg,
                OBP = pa > 0 ? (float)(stats.H + stats.BB + stats.HBP) / pa : 0.3f,
                SLG = avg + iso,
                ISO = iso,
                WOBA = woba,
                WRC = -1.0f, // Fill in later, need league wOBA
                HRPerc = pa > 0 ? (float)stats.HR / pa : 0,
                BBPerc = pa > 0 ? (float)stats.BB / pa : 0,
                KPerc = pa > 0 ? (float)stats.K / pa : 0,
                SBRate = pa > 0 ? (float)stats.SB / pa : 0,
                SBPerc = (stats.SB + stats.CS) > 0 ? (float)stats.SB / (stats.SB + stats.CS) : 0,
                SB = stats.SB,
                CS = stats.CS,
                HR = stats.HR,
            };

            return ma;
        }

        public static Player_Hitter_MonthAdvanced HitterNormalToAdvanced(Player_Hitter_GameLog stats)
        {
            int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;
            int pa = stats.PA;
            float avg = stats.AB > 0 ? (float)stats.H / stats.AB : 0.0f;
            float iso = stats.AB > 0 ? (float)(stats.Hit2B + (2 * stats.Hit3B) + (3 * stats.HR)) / stats.AB : 0;
            float woba = pa > 0 ?
                ((0.69f * stats.BB) + (0.72f * stats.HBP) + (0.89f * singles) + (1.27f * stats.Hit2B) + (1.62f * stats.Hit3B) + (2.10f * stats.HR)) / pa
                : 0;
            Player_Hitter_MonthAdvanced ma = new()
            {
                MlbId = stats.MlbId,
                LevelId = stats.LevelId,
                Year = stats.Year,
                Month = stats.Month,
                TeamId = -1, // Needs to get entered elsewhere, but not needed unless submitting to db
                LeagueId = -1,
                PA = pa,
                AVG = avg,
                OBP = pa > 0 ? (float)(stats.H + stats.BB + stats.HBP) / pa : 0.3f,
                SLG = avg + iso,
                ISO = iso,
                WOBA = woba,
                WRC = -1.0f, // Fill in later, need league wOBA
                HRPerc = pa > 0 ? (float)stats.HR / pa : 0,
                BBPerc = pa > 0 ? (float)stats.BB / pa : 0,
                KPerc = pa > 0 ? (float)stats.K / pa : 0,
                SBRate = pa > 0 ? (float)stats.SB / pa : 0,
                SBPerc = (stats.SB + stats.CS) > 0 ? (float)stats.SB / (stats.SB + stats.CS) : 0,
                SB = stats.SB,
                CS = stats.CS,
                HR = stats.HR,
            };

            return ma;
        }

        public static Func<Player_Hitter_GameLog, Player_Hitter_GameLog, Player_Hitter_GameLog> HitterGameLogAggregation = (a, b) =>
        new Player_Hitter_GameLog
        {
            GameId = a.GameId,
            MlbId = a.MlbId,
            Day = a.Day,
            Month = a.Month,
            Year = a.Year,
            AB = a.AB + b.AB,
            PA = a.PA + b.PA,
            H = a.H + b.H,
            Hit2B = a.Hit2B + b.Hit2B,
            Hit3B = a.Hit3B + b.Hit3B,
            HR = a.HR + b.HR,
            K = a.K + b.K,
            BB = a.BB + b.BB,
            SB = a.SB + b.SB,
            CS = a.CS + b.CS,
            HBP = a.HBP + b.HBP,
            Position = a.Position,
            LevelId = a.LevelId,
            HomeTeamId = a.HomeTeamId,
            TeamId = a.TeamId,
            LeagueId = a.TeamId
        };

        public static Player_Pitcher_MonthAdvanced PitcherNormalToAdvanced(Player_Pitcher_MonthStats stats, SqliteDbContext db)
        {
            int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;
            float woba = stats.BattersFaced > 0 ?
                ((0.69f * stats.BB) + (0.72f * stats.HBP) + (0.89f * singles) + (1.27f * stats.Hit2B) + (1.62f * stats.Hit3B) + (2.10f * stats.HR)) / stats.BattersFaced
                : 0;

            float fipConstant = db.Level_PitcherStats.Where(f => f.Year == stats.Year && f.Month == stats.Month && f.LevelId == stats.LevelId).Select(f => f.FipConstant).Single();
            Player_Pitcher_MonthAdvanced ma = new()
            {
                MlbId = stats.MlbId,
                LevelId = stats.LevelId,
                Year = stats.Year,
                Month = stats.Month,
                TeamId = -1, // Needs to get entered elsewhere, but not needed unless submitting to db
                LeagueId = -1,
                BF = stats.BattersFaced,
                Outs = stats.Outs,
                WOBA = woba,
                HRPerc = stats.BattersFaced > 0 ? (float)stats.HR / stats.BattersFaced : 0,
                BBPerc = stats.BattersFaced > 0 ? (float)stats.BB / stats.BattersFaced : 0,
                KPerc = stats.BattersFaced > 0 ? (float)stats.K / stats.BattersFaced : 0,
                ERA = stats.Outs > 0 ? (float)stats.ER * 27 / stats.Outs : stats.ER * 27.0f,
                FIP = stats.Outs > 0 ? (float)((13 * stats.HR) + 3 * (stats.BB + stats.HBP) - (2 * stats.K)) * 3 / stats.Outs + fipConstant : 99.0f,
                GBRatio = stats.AO > 0 ? (float)stats.GO / (stats.GO + stats.AO) : 1.0f,
                HR = stats.HR,
            };
            return ma;
        }

        public static Func<Player_Pitcher_GameLog, Player_Pitcher_GameLog, Player_Pitcher_GameLog> PitcherGameLogAggregation = (a, b) =>
        new Player_Pitcher_GameLog
        {
            GameId = a.GameId,
            MlbId = a.MlbId,
            Day = a.Day,
            Month = a.Month,
            Year = a.Year,
            BattersFaced = a.BattersFaced + b.BattersFaced,
            Outs = a.Outs + b.Outs,
            H = a.H + b.H,
            Hit2B = a.Hit2B + b.Hit2B,
            Hit3B = a.Hit3B + b.Hit3B,
            HR = a.HR + b.HR,
            K = a.K + b.K,
            BB = a.BB + b.BB,
            GO = a.GO + b.GO,
            AO = a.AO + b.AO,
            ER = a.ER + b.ER,
            R = a.R + b.R,
            HBP = a.HBP + b.HBP,
            LevelId = a.LevelId,
            HomeTeamId = a.HomeTeamId,
            TeamId = a.TeamId,
            LeagueId = a.TeamId
        };

        public static int GetModelMask(Db.Model_Players player, int year, int month)
        {
            int mask = 0;

            if (player.LastProspectYear > year || (player.LastProspectYear == year && player.LastProspectMonth >= month))
                mask += 1;

            return mask;
        }

        public static void LogException(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            Console.Write(e.StackTrace);
        }

        public static float GetAge1MinusAge0(int y1, int m1, int d1, int y0, int m0, int d0)
        {
            int deltaYears = y1 - y0;
            int deltaMonths = m1 - m0;
            int deltaDays = d1 - d0;

            return deltaYears + (deltaMonths / 12.0f) + (deltaDays / 365.0f);
        }

        public static bool GamesAtLevel(int month, int level, int year, SqliteDbContext db)
        {
            
            // Get Games at level, find max ABs in a month
            var lhs = db.Level_HitterStats.Where(f => f.LevelId == level && f.Year == year);

            // No games at all
            if (!lhs.Any()) 
                return false;
                

            int maxAbs = lhs.Max(f => f.AB);

            // Not enough games
            if (!lhs.Where(f => f.Month == month).Any())
                return false;


            // Return whether games played is half max month
            var ab = lhs.Where(f => f.Month == month).Single().AB;
            return ab >= (maxAbs / 5);
        }

        public static float GetGamesFrac(int month, int level, int year, SqliteDbContext db)
        {
            var lhs = db.Level_HitterStats.Where(f => f.LevelId == level && f.Year == year);

            // No games at all
            if (!lhs.Any())
                return 0;

            int maxAbs = lhs.Max(f => f.AB);

            var ab = lhs.Where(f => f.Month == month).Single().AB;
            return (float)ab / maxAbs;
        }

        public static int GetInjStatus(int month, int year, int mlbId, SqliteDbContext db)
        {
            var ils = db.Transaction_Log.Where(f => f.MlbId == mlbId && f.Year == year && f.Month == month)
                .Select(f => f.ToIL).Distinct();

            int ilStatus = 0;
            foreach (int il in ils.Where(f => f > 0))
            {
                ilStatus += (1 << (il - 1));
            }

            return ilStatus;
        }

        public static int ModelLevelToMlbLevel(int level)
        {
            return level == 1 ? level : level + 9;
        }

        public static int MlbLevelToModelLevel(int level)
        {
            return level == 1 ? level : level - 9;
        }

        public static int GetParentOrgId(int teamId, int year, SqliteDbContext db)
        {
            // Check if team is parent
            if (db.Team_OrganizationMap.Any(f => f.ParentOrgId == teamId))
                return teamId;

            var tom = db.Team_OrganizationMap.Where(f => f.TeamId == teamId && f.Year == year);
            if (!tom.Any())
                return -2;
            return tom.Single().ParentOrgId;
        }
    }
}
