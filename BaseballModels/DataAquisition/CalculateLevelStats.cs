using Db;
using System.Security.Cryptography;
using System;

namespace DataAquisition
{
    internal class CalculateLevelStats
    {
        private static bool LevelHitterStats(SqliteDbContext db, int year, int month)
        {
            db.Level_HitterStats.RemoveRange(db.Level_HitterStats.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            foreach (int level in Constants.SPORT_IDS)
            {
                var levelStats = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.Month == month && f.Level == level).AsEnumerable();
                if (!levelStats.Any()) // Skip empty
                    continue;

                // Sum all stats at that level
                Player_Hitter_GameLog summedStats = levelStats.Aggregate((a, b) => new Player_Hitter_GameLog
                {
                    GameLogId = 0,
                    GameId = 0,
                    MlbId = 0,
                    Day = 0,
                    Month = 0,
                    Year = 0,
                    AB = a.AB + b.AB,
                    H = a.H + b.H,
                    Hit2B = a.Hit2B + b.Hit2B,
                    Hit3B = a.Hit3B + b.Hit3B,
                    HR = a.HR + b.HR,
                    K = a.K + b.K,
                    BB = a.BB + b.BB,
                    SB = a.SB + b.SB,
                    CS = a.CS + b.CS,
                    HBP = a.HBP + b.HBP,
                    Position = 0,
                    Level = 0,
                    HomeTeamId = 0,
                    TeamId = 0,
                    LeagueId = 0
                });

                // Transform to get desired stats
                float avg = (float)summedStats.H / summedStats.AB;
                int singles = summedStats.H - summedStats.HR - summedStats.Hit2B - summedStats.Hit3B;
                float iso = (float)(summedStats.Hit2B + (2 * summedStats.Hit3B) + (3 * summedStats.HR)) / summedStats.AB;
                float pa = summedStats.AB + summedStats.BB + summedStats.HBP;
                float obp = (float)(summedStats.BB + summedStats.HBP + summedStats.H) / pa;
                float slg = avg + iso;
                float woba = ((0.69f * summedStats.BB) + (0.72f * summedStats.HBP) + (0.89f * singles) + (1.27f * summedStats.Hit2B) + (1.62f * summedStats.Hit3B) + (2.10f * summedStats.HR)) / pa;

                Level_HitterStats lhs = new Level_HitterStats
                {
                    LevelId = level,
                    Year = year,
                    Month = month,
                    AVG = avg,
                    OBP = obp,
                    SLG = slg,
                    ISO = iso,
                    WOBA = woba,
                    HRPerc = summedStats.HR / pa,
                    BBPerc = summedStats.BB / pa,
                    KPerc = summedStats.K / pa,
                    SBRate = summedStats.SB / pa,
                    SBPerc = (float)summedStats.SB / (summedStats.SB + summedStats.CS)
                };
                db.Level_HitterStats.Add(lhs);
            }
            db.SaveChanges();

            return true;
        }

        private static bool LevelPitcherStats(SqliteDbContext db, int year, int month)
        {
            db.Level_PitcherStats.RemoveRange(db.Level_PitcherStats.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            foreach (int level in Constants.SPORT_IDS)
            {
                var levelStats = db.Player_Pitcher_GameLog.Where(f => f.Year == year && f.Month == month && f.Level == level).AsEnumerable(); ;
                if (!levelStats.Any()) // Skip empty
                    continue;

                // Sum all stats at that level
                Player_Pitcher_GameLog summedStats = levelStats.Aggregate((a, b) => new Player_Pitcher_GameLog
                {
                    GameLogId = 0,
                    GameId = 0,
                    MlbId = 0,
                    Day = 0,
                    Month = 0,
                    Year = 0,
                    BattersFaced = a.BattersFaced + b.BattersFaced,
                    H = a.H + b.H,
                    Hit2B = a.Hit2B + b.Hit2B,
                    Hit3B = a.Hit3B + b.Hit3B,
                    HR = a.HR + b.HR,
                    K = a.K + b.K,
                    Bb = a.Bb + b.Bb,
                    Hbp = a.Hbp + b.Hbp,
                    R = a.R + b.R,
                    Er = a.Er + b.Er,
                    Outs = a.Outs + b.Outs,
                    Go = a.Go + b.Go,
                    Ao = a.Ao + b.Ao,
                    Level = 0,
                    HomeTeamId = 0,
                    TeamId = 0,
                    LeagueId = 0
                });

                // Transform to get desired stats
                int ab = summedStats.BattersFaced - summedStats.Bb + summedStats.Hbp;
                float avg = (float)summedStats.H / ab;
                int singles = summedStats.H - summedStats.HR - summedStats.Hit2B - summedStats.Hit3B;
                float iso = (float)(summedStats.Hit2B + (2 * summedStats.Hit3B) + (3 * summedStats.HR)) / ab;
                float pa = summedStats.BattersFaced;
                float woba = ((0.69f * summedStats.Bb) + (0.72f * summedStats.Hbp) + (0.89f * singles) + (1.27f * summedStats.Hit2B) + (1.62f * summedStats.Hit3B) + (2.10f * summedStats.HR)) / pa;
                float era = (float)summedStats.Er / ((float)summedStats.Outs / 27);
                float ra = (float)summedStats.R / ((float)summedStats.Outs / 27);

                // Calculate fip constant
                float fipNoConstant = (float)((13 * summedStats.HR) + (3 * (summedStats.Hbp + summedStats.Bb)) - (2 * summedStats.K)) / ((float)summedStats.Outs / 3);
                float fipConstant = era - fipNoConstant;

                Level_PitcherStats lps = new Level_PitcherStats
                {
                    Level = level,
                    Year = year,
                    Month = month,
                    ERA = era,
                    RA = ra,
                    FipConstant = fipConstant,
                    WOBA = woba,
                    HrPerc = summedStats.HR / pa,
                    BbPerc = summedStats.Bb / pa,
                    KPerc = summedStats.K / pa,
                    GoPerc = (float)summedStats.Go / (summedStats.Go + summedStats.Ao),
                    Avg = avg,
                    Iso = iso
                };
                db.Level_PitcherStats.Add(lps);
            }
            db.SaveChanges();

            return true;
        }

        public static bool Main(SqliteDbContext db, int year, int month)
        {
            if (!LevelHitterStats(db, year, month))
                return false;

            if (!LevelPitcherStats(db, year, month))
                return false;

            return true;
        }
    }
}
