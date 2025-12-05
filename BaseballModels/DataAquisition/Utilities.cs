using Db;

namespace DataAquisition
{
    internal class Utilities
    {
        public static Player_Hitter_MonthAdvanced HitterNormalToAdvanced(Player_Hitter_MonthStats stats, LeagueStats ls)
        {
            int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;
            int pa = stats.PA;
            float avg = stats.AB > 0 ? (float)stats.H / stats.AB : 0.0f;
            float iso = stats.AB > 0 ? (float)(stats.Hit2B + (2 * stats.Hit3B) + (3 * stats.HR)) / stats.AB : 0;
            float woba = Utilities.CalculateWOBA(ls, stats.HBP, stats.BB, singles, stats.Hit2B, stats.Hit3B, stats.HR, pa);
            Player_Hitter_MonthAdvanced ma = new()
            {
                MlbId = stats.MlbId,
                LevelId = stats.LevelId,
                Year = stats.Year,
                Month = stats.Month,
                TeamId = -1, // Needs to get entered elsewhere, but not needed unless submitting to db
                LeagueId = -1,
                ParkFactor = stats.ParkRunFactor,
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
                CrBSR = -100000, // Needs to be calculated elsewhere, depends on factors not in this function
                CrDEF = -100000, // Set as insane value so it's clear in DB if this isn't getting set
                CrOFF = -100000,
                CrWAR = -100000,
            };

            return ma;
        }

        public static Player_Hitter_MonthAdvanced HitterNormalToAdvanced(Player_Hitter_GameLog stats, LeagueStats ls, float parkFactor)
        {
            int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;
            int pa = stats.PA;
            float avg = stats.AB > 0 ? (float)stats.H / stats.AB : 0.0f;
            float iso = stats.AB > 0 ? (float)(stats.Hit2B + (2 * stats.Hit3B) + (3 * stats.HR)) / stats.AB : 0;
            float woba = Utilities.CalculateWOBA(ls, stats.HBP, stats.BB, singles, stats.Hit2B, stats.Hit3B, stats.HR, pa);
            Player_Hitter_MonthAdvanced ma = new()
            {
                MlbId = stats.MlbId,
                LevelId = stats.LevelId,
                Year = stats.Year,
                Month = stats.Month,
                TeamId = -1, // Needs to get entered elsewhere, but not needed unless submitting to db
                LeagueId = -1,
                ParkFactor = parkFactor,
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
                CrBSR = -100000, // Needs to be calculated elsewhere, depends on factors not in this function
                CrDEF = -100000, // Set as insane value so it's clear in DB if this isn't getting set
                CrOFF = -100000,
                CrWAR = -100000,
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
            StadiumId = a.StadiumId,
            IsHome = a.IsHome,
            TeamId = a.TeamId,
            OppTeamId = a.OppTeamId,
            LeagueId = a.TeamId
        };

        public static Player_Pitcher_MonthAdvanced PitcherNormalToAdvanced(Player_Pitcher_MonthStats stats, LeagueStats ls, SqliteDbContext db)
        {
            int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;

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
                SPPerc = stats.SPPerc,
                WOBA = Utilities.CalculateWOBA(ls, stats.HBP, stats.BB, singles, stats.Hit2B, stats.Hit3B, stats.HR, stats.BattersFaced),
                HRPerc = stats.BattersFaced > 0 ? (float)stats.HR / stats.BattersFaced : 0,
                BBPerc = stats.BattersFaced > 0 ? (float)stats.BB / stats.BattersFaced : 0,
                KPerc = stats.BattersFaced > 0 ? (float)stats.K / stats.BattersFaced : 0,
                ERA = stats.Outs > 0 ? (float)stats.ER * 27 / stats.Outs : stats.ER * 27.0f,
                FIP = Utilities.CalculateFip(ls.CFIP, stats.HR, stats.K, stats.BB + stats.HBP, stats.Outs),
                GBRatio = stats.AO > 0 ? (float)stats.GO / (stats.GO + stats.AO) : 1.0f,
                HR = stats.HR,
                CrWAR = -100000,
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
            Started = a.Started + b.Started,
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
            StadiumId = a.StadiumId,
            IsHome = a.IsHome,
            TeamId = a.TeamId,
            OppTeamId = a.OppTeamId,
            LeagueId = a.TeamId
        };

        public static (float, float) GetParkFactors(IEnumerable<Player_Hitter_GameLog> games, SqliteDbContext db)
        {
            float parkFactor = 1.0f;
            float parkHRFactor = 1.0f;
            int totalPA = 0;

            foreach (var game in games)
            {
                if (game.PA == 0)
                    continue;

                totalPA += game.PA;
                float thisGameFrac = (float)game.PA / totalPA;

                float stadiumParkFactor = 1.0f;
                float stadiumParkHRFactor = 1.0f;
                try {
                    Park_Factors pf = db.Park_Factors.Where(f => f.Year == game.Year && f.StadiumId == game.StadiumId).Single();
                    stadiumParkFactor = pf.RunFactor;
                    stadiumParkHRFactor = pf.HRFactor;
                } catch (Exception) { } // Not enough data for park factor, use default

                parkFactor = (thisGameFrac * stadiumParkFactor) + ((1 - thisGameFrac) * parkFactor);
                parkHRFactor = (thisGameFrac * stadiumParkHRFactor) + ((1 - thisGameFrac) * parkHRFactor);
            }

            return (parkFactor, parkHRFactor);
        }

        public static float CalculateWOBA(LeagueStats ls, int hbp, int bb, int h1B, int h2B, int h3B, int hr, int pa)
        {
            if (pa == 0)
                return ls.AvgWOBA;
            return ((ls.WHBP * hbp) +
                (ls.WBB * bb) +
                (ls.W1B * h1B) +
                (ls.W2B * h2B) +
                (ls.W3B * h3B) +
                (ls.WHR * hr)) / pa;
        }

        public static float CalculateFip(float cFIP, int hr, int k, int bbPlusHBP, int outs)
        {
            if (outs == 0)
                return 20.0f; // Don't want too high otherwise will mess up normalization
            return ((13 * hr) + (3 * bbPlusHBP) - (2 * k)) / ((float)outs / 3) + cFIP;
        }

        public static float SafeDivide(float num, float denom)
        {
            return denom == 0 ?
                1.0f : num / denom;
        }

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

        public static float ClampWRC(float wrc)
        {
            return Math.Min(300, Math.Max(-100, wrc));
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
            int monthPA = db.Level_GameCounts.Where(f => f.LevelId == level && f.Year == year && f.Month == month)
                .Select(f => f.MaxPA).SingleOrDefault();

            var playerPAs = db.Level_GameCounts.Where(f => f.LevelId == level && f.Year == year)
                .Select(f => f.MaxPA);

            if (!playerPAs.Any())
                return false;

            int yearMaxPAs = playerPAs.Max();

            return GetGamesFrac(month, level, year, db) >= (0.2f * yearMaxPAs);
        }

        public static float GetGamesFrac(int month, int level, int year, SqliteDbContext db)
        {
            int monthPA = db.Level_GameCounts.Where(f => f.LevelId == level && f.Year == year && f.Month == month)
                .Select(f => f.MaxPA).SingleOrDefault();

            int? yearMaxPAs = db.Level_GameCounts.Where(f => f.LevelId == level && f.Year == year)
                .Select(f => f.MaxPA).Max();

            if (yearMaxPAs == null) // No data for year at all
                return 0.0f;
            
            else
                return (float)monthPA / (yearMaxPAs ?? 0); // Divide by zero because something went wrong
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
