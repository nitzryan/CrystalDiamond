using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateAnnualStats
    {
        private class PlayerTeamType
        {
            public required int MlbId { get; set; }
            public required int LevelId { get; set; }
            public required int TeamId { get; set; }
            public required int LeagueId { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is PlayerTeamType other)
                {
                    return MlbId == other.MlbId && LevelId == other.LevelId
                        && TeamId == other.TeamId && LeagueId == other.LeagueId;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(MlbId, LevelId, TeamId, LeagueId);
            }
        }

        public static bool Main(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // Hitter Yearly Stats
                db.Player_Hitter_YearAdvanced.RemoveRange(
                    db.Player_Hitter_YearAdvanced.Where(f => f.Year == year)
                );
                db.SaveChanges();

                var hitterData = db.Player_Hitter_GameLog.Where(f => f.Year == year).Select(f => new PlayerTeamType { MlbId=f.MlbId, LevelId=f.LevelId, TeamId=f.TeamId, LeagueId=f.LeagueId }).Distinct();
                var hitterGames = db.Player_Hitter_GameLog.Where(f => f.Year == year)
                    .OrderBy(f => f.MlbId)
                    .ThenBy(f => f.LevelId)
                    .ThenBy(f => f.TeamId)
                    .ThenBy(f => f.LeagueId);
                using (ProgressBar progressBar = new ProgressBar(hitterData.Count(), $"Calculating Hitter Advanced stats for Year={year}"))
                {
                    foreach (var d in hitterData)
                    {
                        var games = hitterGames.Where(f => f.MlbId == d.MlbId
                                                        && f.LevelId == d.LevelId
                                                        && f.TeamId == d.TeamId
                                                        && f.LeagueId == d.LeagueId);
                        var s = games.Aggregate(Utilities.HitterGameLogAggregation);
                        var (parkFactor, parkHRFactor) = Utilities.GetParkFactors(games, db);

                        int pa = s.PA;
                        float iso = s.AB > 0 ? (float)(s.Hit2B + (2 * s.Hit3B) + (3 * s.HR)) / s.AB : 0;
                        float avg = s.AB > 0 ? (float)s.H / s.AB : 0;
                        int singles = s.H - s.Hit2B - s.Hit3B - s.HR;
                        LeagueStats ls = db.LeagueStats.Where(f => f.LeagueId == d.LeagueId && f.Year == year).Single();
                        float woba = Utilities.CalculateWOBA(ls, s.HBP, s.BB, singles, s.Hit2B, s.Hit3B, s.HR, s.PA);
                        db.Player_Hitter_YearAdvanced.Add(new Player_Hitter_YearAdvanced
                        {
                            MlbId = d.MlbId,
                            LevelId = d.LevelId,
                            Year = year,
                            TeamId = d.TeamId,
                            LeagueId = d.LeagueId,
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

                        progressBar.Tick();
                    }
                }
                
                db.SaveChanges();

                // Pitcher Yearly Stats
                db.Player_Pitcher_YearAdvanced.RemoveRange(
                    db.Player_Pitcher_YearAdvanced.Where(f => f.Year == year)
                );
                db.SaveChanges();

                var pitcherData = db.Player_Pitcher_GameLog.Where(f => f.Year == year).Select(f => new PlayerTeamType { MlbId = f.MlbId, LevelId = f.LevelId, TeamId = f.TeamId, LeagueId = f.LeagueId }).Distinct();
                var pitcherGames = db.Player_Pitcher_GameLog.Where(f => f.Year == year)
                    .OrderBy(f => f.MlbId)
                    .ThenBy(f => f.LevelId)
                    .ThenBy(f => f.TeamId)
                    .ThenBy(f => f.LeagueId);
                using (ProgressBar progressBar = new ProgressBar(pitcherData.Count(), $"Calculating Pitcher Advanced stats for Year={year}"))
                {
                    foreach (var d in pitcherData)
                    {
                        var games = pitcherGames.Where(f => f.MlbId == d.MlbId
                                                        && f.LevelId == d.LevelId
                                                        && f.TeamId == d.TeamId
                                                        && f.LeagueId == d.LeagueId);

                        var s = games.Aggregate(Utilities.PitcherGameLogAggregation);
                        int outsSP = games.Where(f => f.Started == 1).Sum(f => f.Outs);

                        LeagueStats ls = db.LeagueStats.Where(f => f.LeagueId == d.LeagueId && f.Year == year).Single();

                        int pa = s.BattersFaced;
                        int singles = s.H - s.Hit2B - s.Hit3B - s.HR;
                        float woba = Utilities.CalculateWOBA(ls, s.HBP, s.BB, singles, s.Hit2B, s.Hit3B, s.HR, pa);
                        int outs = s.Outs > 0 ? s.Outs : 1;
                        db.Player_Pitcher_YearAdvanced.Add(new Player_Pitcher_YearAdvanced
                        {
                            MlbId = d.MlbId,
                            LevelId = d.LevelId,
                            Year = year,
                            TeamId = d.TeamId,
                            LeagueId = d.LeagueId,
                            SPPerc = s.Outs > 0 ? (float)(outsSP) / s.Outs : 0.5f,
                            BF = s.BattersFaced,
                            Outs = s.Outs,
                            GBRatio = s.AO > 0 ? (float)s.GO / (s.GO + s.AO) : 1,
                            ERA = (float)s.ER * 27 / outs,
                            FIP = Utilities.CalculateFip(ls.CFIP, s.HR, s.K, s.BB + s.HBP, s.Outs),
                            WOBA = woba,
                            HR = s.HR,
                            BBPerc = pa > 0 ? (float)s.BB / pa : 0,
                            KPerc = pa > 0 ? (float)s.K / pa : 0,
                        });

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

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
