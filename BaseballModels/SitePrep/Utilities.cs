using Db;
using System.Text;

namespace SitePrep
{
    internal class Utilities
    {
        private static List<float> WAR_BUCKETS = [0, 0.5f, 3, 7.5f, 15, 25, 35];
        private static List<float> VALUE_BUCKETS = [0, 2.5f, 12.5f, 35, 75, 150, 250];

        public static void LogException(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            Console.Write(e.StackTrace);
        }

        public static Func<List<int>, Player_Hitter_GameLog, List<int>> PositionAggregation = (l, gl) =>
        {
            List<int> gamesAtPosition = [l[0], l[1], l[2], l[3], l[4], l[5], l[6], l[7], l[8], l[9], l[10], l[11]];
            gamesAtPosition[gl.Position]++;
            return gamesAtPosition;
        };

        public static string GetPosition(SqliteDbContext db, int mlbId, int year)
        {
            string position = "";
            var games = db.Player_Hitter_GameLog.Where(f => f.MlbId == mlbId && (f.Year == year || f.Year == year - 1) && f.Position < 10)
                .AsEnumerable();

            if (!games.Any())
                return "DH";

            List<int> initList = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            var gamesAtPosition = games.Aggregate(initList, (a, b) => PositionAggregation(a, b));
            int totalGames = games.Count();
            List<float> gamesProp = [.. gamesAtPosition.Select(f => (float)f / totalGames)];

            const float SINGLE_POSITION_CUTOFF = 0.8f;
            const float MULTI_POSITION_CUTOFF = 0.2f;

            List<bool> sp = [.. gamesProp.Select(f => f > SINGLE_POSITION_CUTOFF)];
            List<bool> mps = [.. gamesProp.Select(f => f > MULTI_POSITION_CUTOFF)];
            // Single Position
            if (sp[2])
                position = "C";
            else if (sp[3])
                position = "1B";
            else if (sp[4])
                position = "2B";
            else if (sp[5])
                position = "3B";
            else if (sp[6])
                position = "SS";
            else if (sp[7])
                position = "LF";
            else if (sp[8])
                position = "CF";
            else if (sp[9])
                position = "RF";
            else if (gamesProp[7] + gamesProp[8] + gamesProp[9] > SINGLE_POSITION_CUTOFF)
                position = "OF";
            else
            {
                if (mps[2])
                    position += "C/";
                if (mps[3])
                    position += "1B/";
                if (mps[4])
                    position += "2B/";
                if (mps[5])
                    position += "3B/";
                if (mps[6])
                    position += "SS/";
                if (mps[7])
                    position += "LF/";
                if (mps[8])
                    position += "CF/";
                if (mps[9])
                    position += "RF/";

                if (position.Length > 0)
                    position = position.Substring(0, position.Length - 1);
                else
                    position = "UTIL";
            }

            return position;
        }

        public static LeagueStats MergeLeagueStats(IEnumerable<LeagueStats> ls)
        {
            int totalAbs = ls.Sum(f => f.LeaguePA);

            LeagueStats leagueStats = new LeagueStats
            {
                LeagueId = -1,
                Year = ls.First().Year,
                AvgWOBA = 0,
                AvgHitterWOBA = 0,
                WOBAScale = 0,
                WBB = 0,
                WHBP = 0,
                W1B = 0,
                W2B = 0,
                W3B = 0,
                WHR = 0,
                RunSB = 0,
                RunCS = 0,
                RPerPA = 0,
                RPerWin = 0,
                RunErr = 0,
                LeaguePA = 0,
                LeagueGames = 0,
                CFIP = 0,
                FIPR9Adjustment = 0,
                LeagueERA = 0,
            };

            for (int i = 0; i < ls.Count(); i++)
            {
                var l = ls.ElementAt(i);
                float abFrac = (float)(l.LeaguePA) / totalAbs;                

                leagueStats.AvgWOBA += l.AvgWOBA * abFrac;
                leagueStats.AvgHitterWOBA += l.AvgHitterWOBA * abFrac;
                leagueStats.WOBAScale += l.WOBAScale * abFrac;
                leagueStats.WBB += l.WBB * abFrac;
                leagueStats.WHBP += l.WHBP * abFrac;
                leagueStats.W1B += l.W1B * abFrac;
                leagueStats.W2B += l.W2B * abFrac;
                leagueStats.W3B += l.W3B * abFrac;
                leagueStats.WHR += l.WHR * abFrac;
                leagueStats.RunSB += l.RunSB * abFrac;
                leagueStats.RunCS += l.RunCS * abFrac;
                leagueStats.RPerPA += l.RPerPA * abFrac;
                leagueStats.RPerWin += l.RPerWin * abFrac;
                leagueStats.LeaguePA += l.LeaguePA;
                leagueStats.LeagueGames += l.LeagueGames;
                leagueStats.CFIP += l.CFIP * abFrac;
                leagueStats.FIPR9Adjustment += l.FIPR9Adjustment * abFrac;
                leagueStats.LeagueERA += l.LeagueERA * abFrac;
            }

            return leagueStats;
        }

        public static League_HitterYearStats MergeLeagueHitterYearStats(IEnumerable<League_HitterYearStats> lhs)
        {
            int totalAbs = lhs.Sum(f => f.AB);

            League_HitterYearStats leagueStats = new League_HitterYearStats
            {
                LeagueId = -1,
                Year = lhs.First().Year,
                Month = lhs.First().Month,
                AB = 0,
                AVG = 0,
                OBP = 0,
                SLG = 0,
                ISO = 0,
                WOBA = 0,
                HRPerc = 0,
                BBPerc = 0,
                KPerc = 0,
                SBRate = 0,
                SBPerc = 0,
                Hit1B = 0,
                Hit2B = 0,
                Hit3B = 0,
                HitHR = 0,
                BB = 0,
                HBP = 0,
                K = 0,
                SB = 0,
                CS = 0
            };

            for (int i = 0; i < lhs.Count(); i++)
            {
                var l = lhs.ElementAt(i);
                float abFrac = (float)(l.AB) / totalAbs;

                leagueStats.AB += l.AB;
                leagueStats.AVG += l.AVG * abFrac;
                leagueStats.OBP += l.OBP * abFrac;
                leagueStats.SLG += l.SLG * abFrac;
                leagueStats.ISO += l.ISO * abFrac;
                leagueStats.WOBA += l.WOBA * abFrac;
                leagueStats.HRPerc += l.HRPerc * abFrac;
                leagueStats.BBPerc += l.BBPerc * abFrac;
                leagueStats.KPerc += l.KPerc * abFrac;
                leagueStats.SBRate += l.SBRate * abFrac;
                leagueStats.SBPerc += l.SBPerc * abFrac;
                leagueStats.Hit1B += l.Hit1B * abFrac;
                leagueStats.Hit2B += l.Hit2B * abFrac;
                leagueStats.Hit3B += l.Hit3B * abFrac;
                leagueStats.HitHR += l.HitHR * abFrac;
                leagueStats.BB += l.BB * abFrac;
                leagueStats.HBP += l.HBP * abFrac;
                leagueStats.K += l.K * abFrac;
                leagueStats.SB += l.SB * abFrac;
                leagueStats.CS += l.CS * abFrac;
            }

            return leagueStats;
        }

        public static League_PitcherYearStats MergeLeaguePitcherYearStats(IEnumerable<League_PitcherYearStats> lhs, IEnumerable<int> outs)
        {
            int totalOuts = outs.Sum();

            League_PitcherYearStats leagueStats = new League_PitcherYearStats
            {
                LeagueId = -1,
                Year = lhs.First().Year,
                Month = lhs.First().Month,
                ERA = 0,
                RA = 0,
                FipConstant = 0,
                WOBA = 0,
                HRPerc = 0,
                BBPerc = 0,
                KPerc = 0,
                GOPerc = 0,
                Avg = 0,
                Iso = 0
            };

            for (int i = 0; i < lhs.Count(); i++)
            {
                var l = lhs.ElementAt(i);
                float abFrac = (float)(outs.ElementAt(i)) / totalOuts;

                leagueStats.ERA += l.ERA * abFrac;
                leagueStats.RA += l.RA * abFrac;
                leagueStats.FipConstant += l.FipConstant * abFrac;
                leagueStats.Iso += l.Iso * abFrac;
                leagueStats.WOBA += l.WOBA * abFrac;
                leagueStats.HRPerc += l.HRPerc * abFrac;
                leagueStats.BBPerc += l.BBPerc * abFrac;
                leagueStats.KPerc += l.KPerc * abFrac;
                leagueStats.Avg += l.Avg * abFrac;
            }

            return leagueStats;
        }

        public static float CalculateWOBA(LeagueStats ls, float hbp, float bb, float h1B, float h2B, float h3B, float hr, float pa)
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

        public static float CalculateFip(float cFIP, float hr, float k, float bbPlusHBP, float outs)
        {
            if (outs == 0)
                return 20.0f; // Don't want too high otherwise will mess up normalization
            return ((13 * hr) + (3 * bbPlusHBP) - (2 * k)) / ((float)outs / 3) + cFIP;
        }

        public static float CalculateDef(float pa, float percC, float perc1B, float perc2B, float perc3B, float percSS, float percLF, float percCF, float percRF, float percDH)
        {
            float seasons = pa / 600.0f;
            float def = 0;

            def += seasons * percC * Constants.POSITIONAL_ADJUSTMENT_C;
            def += seasons * perc1B * Constants.POSITIONAL_ADJUSTMENT_1B;
            def += seasons * perc2B * Constants.POSITIONAL_ADJUSTMENT_2B;
            def += seasons * perc3B * Constants.POSITIONAL_ADJUSTMENT_3B;
            def += seasons * percSS * Constants.POSITIONAL_ADJUSTMENT_SS;
            def += seasons * percRF * Constants.POSITIONAL_ADJUSTMENT_LF;
            def += seasons * percCF * Constants.POSITIONAL_ADJUSTMENT_CF;
            def += seasons * percRF * Constants.POSITIONAL_ADJUSTMENT_RF;
            def += seasons * percDH * Constants.POSITIONAL_ADJUSTMENT_DH;

            return def;
        }
    }
}
