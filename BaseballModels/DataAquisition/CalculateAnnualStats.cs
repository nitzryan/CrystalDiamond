using Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataAquisition
{
    internal class CalculateAnnualStats
    {
        public static bool Main(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // Hitter Yearly Stats
                db.Player_Hitter_YearAdvanced.RemoveRange(
                    db.Player_Hitter_YearAdvanced.Where(f => f.Year == year)
                );
                db.SaveChanges();

                var hitterData = db.Player_Hitter_GameLog.Where(f => f.Year == year).Select(f => new { f.MlbId, f.LevelId, f.TeamId, f.LeagueId }).Distinct();
                foreach (var d in hitterData)
                {
                    var s = db.Player_Hitter_GameLog.Where(f => f.MlbId == d.MlbId
                                                    && f.LevelId == d.LevelId
                                                    && f.TeamId == d.TeamId
                                                    && f.LeagueId == d.LeagueId
                                                    && f.Year == year)
                        .Aggregate(Utilities.HitterGameLogAggregation);

                    int pa = s.AB + s.HBP + s.BB;
                    float iso = s.AB > 0 ? (float)(s.Hit2B + (2 * s.Hit3B) + (3 * s.HR)) / s.AB : 0;
                    float avg = s.AB > 0 ? (float)s.H / s.AB : 0;
                    int singles = s.H - s.Hit2B - s.Hit3B - s.HR;
                    float woba = pa > 0 ? ((0.69f * s.BB) + (0.72f * s.HBP) + (0.89f * singles) + (1.27f * s.Hit2B) + (1.62f * s.Hit3B) + (2.10f * s.HR)) / pa : 0;
                    db.Player_Hitter_YearAdvanced.Add(new Player_Hitter_YearAdvanced
                    {
                        MlbId = d.MlbId,
                        LevelId = d.LevelId,
                        Year = year,
                        TeamId = d.TeamId,
                        LeagueId = d.LeagueId,
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
                db.SaveChanges();

                // Pitcher Yearly Stats
                db.Player_Pitcher_YearAdvanced.RemoveRange(
                    db.Player_Pitcher_YearAdvanced.Where(f => f.Year == year)
                );
                db.SaveChanges();

                var pitcherData = db.Player_Pitcher_GameLog.Where(f => f.Year == year).Select(f => new { f.MlbId, f.LevelId, f.TeamId, f.LeagueId }).Distinct();
                foreach (var d in pitcherData)
                {
                    var s = db.Player_Pitcher_GameLog.Where(f => f.MlbId == d.MlbId
                                                    && f.LevelId == d.LevelId
                                                    && f.TeamId == d.TeamId
                                                    && f.LeagueId == d.LeagueId
                                                    && f.Year == year)
                        .Aggregate(Utilities.PitcherGameLogAggregation);

                    int pa = s.BattersFaced;
                    float leagueFipConstant = db.Level_PitcherStats.Where(f => f.Year == year && f.LevelId == d.LevelId).Select(f => f.FipConstant).Average();
                    int singles = s.H - s.Hit2B - s.Hit3B - s.HR;
                    float woba = pa > 0 ? ((0.69f * s.BB) + (0.72f * s.HBP) + (0.89f * singles) + (1.27f * s.Hit2B) + (1.62f * s.Hit3B) + (2.10f * s.HR)) / pa : 0;
                    int outs = s.Outs > 0 ? s.Outs : 1;
                    db.Player_Pitcher_YearAdvanced.Add(new Player_Pitcher_YearAdvanced
                    {
                        MlbId = d.MlbId,
                        LevelId = d.LevelId,
                        Year = year,
                        TeamId = d.TeamId,
                        LeagueId = d.LeagueId,
                        BF = s.BattersFaced,
                        Outs = s.Outs,
                        GBRatio = s.AO > 0 ? (float)s.GO / (s.GO + s.AO) : 1,
                        ERA = (float)s.ER * 27 / outs,
                        FIP = (float)((13 * s.HR) + 3 * (s.BB + s.HBP) - (2 * s.K)) * 3 / outs + leagueFipConstant,
                        WOBA = woba,
                        HR = s.HR,
                        BBPerc = pa > 0 ? (float)s.BB / pa : 0,
                        KPerc = pa > 0 ? (float)s.K / pa : 0,
                    });
                }
                db.SaveChanges();

                return true;
            } catch (Exception e)
            {
                Console.WriteLine("Error in CalculateAnnualStats");
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                return false;
            }
        }
    }
}
