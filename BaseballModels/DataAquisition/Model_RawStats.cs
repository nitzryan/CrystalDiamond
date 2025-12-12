using Db;
using ShellProgressBar;
using System.ComponentModel;

namespace DataAquisition
{
    internal class Model_RawStats
    {
        private class TeamLevelStadium
        {
            public required int teamId { get; set; }
            public required int stadiumId { get; set; }
            public required int leagueId { get; set; }
            public required int levelId { get; set; }
            public required float parkFactor { get; set; }
        }

        private static TeamLevelStadium GetTLS(int teamId, int year, SqliteDbContext db, IEnumerable<Player_Hitter_GameLog> yearGames)
        { 
            var teamGames = yearGames.Where(f => f.TeamId == teamId && f.IsHome == 1);
            if (!teamGames.Any())
                return new TeamLevelStadium
                {
                    leagueId = -1,
                    stadiumId = -1,
                    teamId = teamId,
                    parkFactor = -1,
                    levelId = -1
                };

            TeamLevelStadium tls = new()
            {
                teamId = teamId,
                stadiumId = teamGames.Select(f => f.StadiumId).First(),
                leagueId = teamGames.Select(f => f.LeagueId).First(),
                levelId = teamGames.Select(f => f.LevelId).First(),
                parkFactor = 0
            };

            tls.parkFactor = db.Park_Factors.Where(f => f.Year == year && f.StadiumId == tls.stadiumId).Single().RunFactor;

            return tls;
        }

        private static float GetParkFactor(IEnumerable<TeamLevelStadium> thisYearPairs, IEnumerable<TeamLevelStadium> nextYearPairs, int levelIdx, float thisYearFraction, float nextYearFraction)
        {
            var thisStadium = thisYearPairs.Where(f => f.levelId == Constants.SPORT_IDS[levelIdx]);
            var nextStadium = nextYearPairs.Where(f => f.levelId == Constants.SPORT_IDS[levelIdx]);

            if (!thisStadium.Any() && !nextStadium.Any()) // Short Season A got eliminated
                return -1.0f;

            // Only 1 year of data in stadium, take only that year
            if (!thisStadium.Any())
                return nextStadium.Select(f => f.parkFactor).Average();

            if (!nextStadium.Any())
                return thisStadium.Select(f => f.parkFactor).Average();

            return (thisStadium.Select(f => f.parkFactor).Average() * thisYearFraction) +
                                (nextStadium.Select(f => f.parkFactor).Average() * nextYearFraction);
        }

        private static float GetMonthsFrac(IEnumerable<TeamLevelStadium> thisYearPairs, IEnumerable<TeamLevelStadium> nextYearPairs, int levelIdx, float thisYearFraction, float nextYearFraction)
        {
            var thisStadium = thisYearPairs.Where(f => f.levelId == Constants.SPORT_IDS[levelIdx]);
            var nextStadium = nextYearPairs.Where(f => f.levelId == Constants.SPORT_IDS[levelIdx]);

            if (!thisStadium.Any() && !nextStadium.Any()) // Short Season A got eliminated
                return 0.0f;

            // Only 1 year of data in stadium, take only that year
            if (!thisStadium.Any())
                return nextYearFraction;

            if (!nextStadium.Any())
                return thisYearFraction;

            return 1.0f;
        }

        public static bool OrgLeagueStatus(int year, int month, bool forceReset)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // User requested data deletion
                if (forceReset)
                {
                    db.Model_OrgLeagueStatus.RemoveRange(db.Model_OrgLeagueStatus.Where(f => f.Year == year && f.Month == month));
                    db.SaveChanges();
                }

                // Early exit if data exists
                if (db.Model_OrgLeagueStatus.Where(f => f.Year == year && f.Month == month).Any())
                    return true;

                // Get fraction of games that are in this year vs next
                float thisYearFraction = (9 - month) / 6.0f;
                float nextYearFraction = 1.0f - thisYearFraction;

                var yearGames = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.IsHome == 1).AsEnumerable().DistinctBy(f => new { f.StadiumId, f.TeamId }).ToList();
                var nextYearGames = db.Player_Hitter_GameLog.Where(f => f.Year == year + 1 && f.IsHome == 1).AsEnumerable().DistinctBy(f => new { f.StadiumId, f.TeamId }).ToList();
                // Go through each org
                var orgIds = db.Team_Parents.Select(f => f.Id);
                using (ProgressBar progressBar = new(orgIds.Count(), $"Calculating OrgLeagueStatus for {year}-{month}"))
                {
                    foreach (var orgId in orgIds)
                    {
                        // Get teams of parent
                        var thisYearTeams = db.Team_OrganizationMap.Where(f => f.ParentOrgId == orgId && f.Year == year)
                            .Select(f => f.TeamId).AsEnumerable().Append(orgId);

                        var nextYearTeams = db.Team_OrganizationMap.Where(f => f.ParentOrgId == orgId && f.Year == year + 1)
                            .Select(f => f.TeamId).AsEnumerable().Append(orgId);

                        // Get Data for park factors, levels, leagues etc
                        List<TeamLevelStadium> thisYearPairs = new();
                        List<TeamLevelStadium> nextYearPairs = new();
                        
                        foreach (var teamId in thisYearTeams)
                        {
                            thisYearPairs.Add(GetTLS(teamId, year, db, yearGames));
                        }
                        foreach (var teamId in nextYearTeams)
                        {
                            nextYearPairs.Add(GetTLS(teamId, year + 1, db, nextYearGames));
                        }

                        // Get parkFactors for each level
                        Model_OrgLeagueStatus ols = new Model_OrgLeagueStatus
                        {
                            OrgId = orgId,
                            Year = year,
                            Month = month,
                            MLB_PF = GetParkFactor(thisYearPairs, nextYearPairs, 0, thisYearFraction, nextYearFraction),
                            AAA_PF = GetParkFactor(thisYearPairs, nextYearPairs, 1, thisYearFraction, nextYearFraction),
                            AA_PF = GetParkFactor(thisYearPairs, nextYearPairs, 2, thisYearFraction, nextYearFraction),
                            HA_PF = GetParkFactor(thisYearPairs, nextYearPairs, 3, thisYearFraction, nextYearFraction),
                            A_PF = GetParkFactor(thisYearPairs, nextYearPairs, 4, thisYearFraction, nextYearFraction),
                            LA_PF = GetParkFactor(thisYearPairs, nextYearPairs, 5, thisYearFraction, nextYearFraction),
                            Rk_PF = GetParkFactor(thisYearPairs, nextYearPairs, 6, thisYearFraction, nextYearFraction),
                            DSL_PF = GetParkFactor(thisYearPairs, nextYearPairs, 7, thisYearFraction, nextYearFraction),
                            MLB_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 0, thisYearFraction, nextYearFraction),
                            AAA_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 1, thisYearFraction, nextYearFraction),
                            AA_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 2, thisYearFraction, nextYearFraction),
                            HA_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 3, thisYearFraction, nextYearFraction),
                            A_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 4, thisYearFraction, nextYearFraction),
                            LA_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 5, thisYearFraction, nextYearFraction),
                            Rk_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 6, thisYearFraction, nextYearFraction),
                            DSL_MonthsFrac = GetMonthsFrac(thisYearPairs, nextYearPairs, 7, thisYearFraction, nextYearFraction),
                        };
                        db.Model_OrgLeagueStatus.Add(ols);

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::OrgLeagueStatus");
                Utilities.LogException(e);
                return false;
            }
        }

        public static bool LeagueBaselines(int year, int month, bool forceReset)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // User requested data deletion
                if (forceReset)
                {
                    db.Model_LeagueHittingBaselines.RemoveRange(db.Model_LeagueHittingBaselines.Where(f => f.Year == year && f.Month == month));
                    db.Model_LeaguePitchingBaselines.RemoveRange(db.Model_LeaguePitchingBaselines.Where(f => f.Year == year && f.Month == month));
                    db.SaveChanges();
                }

                // Early exit if data exists
                if (db.Model_LeagueHittingBaselines.Where(f => f.Year == year && f.Month == month).Any())
                    return true;

                var games = db.Player_Hitter_GameLog.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));
                var pitchingGames = db.Player_Pitcher_GameLog.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));

                // Get fraction of games that are in this year vs next
                float thisYearFraction = (9 - month) / 6.0f;
                float nextYearFraction = 1.0f - thisYearFraction;

                var leagueIds = games.Select(f => f.LeagueId).Distinct();
                using (ProgressBar progressBar = new(leagueIds.Count(), $"Calculating Model_LeagueBaselines for {year}-{month}"))
                {
                    foreach (var leagueId in leagueIds)
                    {
                        var leagueStats = games.Where(f => f.LeagueId == leagueId).Aggregate(Utilities.HitterGameLogAggregation);
                        int singles = leagueStats.H - leagueStats.Hit2B - leagueStats.Hit3B - leagueStats.HR;
                        db.Model_LeagueHittingBaselines.Add(new Model_LeagueHittingBaselines
                        {
                            Year = year,
                            Month = month,
                            LeagueId = leagueId,
                            LevelId = leagueStats.LevelId,
                            Hit1B = Utilities.SafeDivide(singles, leagueStats.PA),
                            Hit2B = Utilities.SafeDivide(leagueStats.Hit2B, leagueStats.PA),
                            Hit3B = Utilities.SafeDivide(leagueStats.Hit3B, leagueStats.PA),
                            HitHR = Utilities.SafeDivide(leagueStats.HR, leagueStats.PA),
                            BB = Utilities.SafeDivide(leagueStats.BB, leagueStats.PA),
                            HBP = Utilities.SafeDivide(leagueStats.HBP, leagueStats.PA),
                            K = Utilities.SafeDivide(leagueStats.K, leagueStats.PA),
                            SB = Utilities.SafeDivide(leagueStats.SB, leagueStats.PA),
                            CS = Utilities.SafeDivide(leagueStats.CS, leagueStats.PA)
                        });

                        // LERP CFIP
                        float thisYearCFIP = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId).Select(f => f.CFIP).SingleOrDefault();
                        float nextYearCFIP = db.LeagueStats.Where(f => f.Year == year + 1 && f.LeagueId == leagueId).Select(f => f.CFIP).SingleOrDefault();
                        float cfip = 0;
                        if (thisYearCFIP == 0)
                            cfip = nextYearCFIP;
                        else if (nextYearCFIP == 0)
                            cfip = thisYearCFIP;
                        else
                            cfip = (thisYearFraction * thisYearCFIP) + (nextYearFraction * nextYearCFIP);

                        var lps = pitchingGames.Where(f => f.LeagueId == leagueId).Aggregate(Utilities.PitcherGameLogAggregation);
                        db.Model_LeaguePitchingBaselines.Add(new Model_LeaguePitchingBaselines
                        {
                            Year = year,
                            Month = month,
                            LeagueId = leagueId,
                            LevelId = leagueStats.LevelId,
                            ERA = Utilities.SafeDivide(lps.ER * 27, lps.Outs),
                            FIP = Utilities.CalculateFip(cfip, lps.HR, lps.K, lps.BB + lps.HBP, lps.Outs),
                            HR = Utilities.SafeDivide(lps.HR, lps.BattersFaced),
                            BB = Utilities.SafeDivide(lps.BB, lps.BattersFaced),
                            HBP = Utilities.SafeDivide(lps.HBP, lps.BattersFaced),
                            K = Utilities.SafeDivide(lps.K, lps.BattersFaced)
                        });

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::LeagueBaselines");
                Utilities.LogException(e);
                return false;
            }
        }

        public static bool HitterPlayerStats(int year, int month, bool forceReset)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // User requested data deletion
                if (forceReset)
                {
                    db.Model_HitterLevelStats.RemoveRange(db.Model_HitterLevelStats.Where(f => f.Year == year && f.Month == month));
                    db.SaveChanges();
                }

                // Early exit if data exists
                if (db.Model_HitterLevelStats.Where(f => f.Year == year && f.Month == month).Any())
                    return true;

                var games = db.Player_Hitter_GameLog.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));
                var leagueBaselines = db.Model_LeagueHittingBaselines.Where(f => f.Year == year && f.Month == month);
                var playerIds = games.Select(f => f.MlbId).Distinct();
                using (ProgressBar progressBar = new(playerIds.Count(), $"Calculating Model_RawHitterStats for {year}-{month}"))
                {
                    // Group by player
                    foreach(var mlbId in playerIds)
                    {
                        // Group by League
                        var levelGroups = games.Where(f => f.MlbId == mlbId)
                            .GroupBy(f => f.LevelId);

                        foreach(var lvlG in levelGroups)
                        {
                            // Group by level
                            var teamGroups = lvlG.GroupBy(f => new { f.TeamId, f.Year});
                            Model_HitterLevelStats mhls = new Model_HitterLevelStats
                            {
                                MlbId = mlbId,
                                Year = year,
                                Month = month,
                                LevelId = lvlG.Key,
                                Pa = 0,
                                Hit1B = 1,
                                Hit2B = 1,
                                Hit3B = 1,
                                HitHR = 1,
                                BB = 1,
                                HBP = 1,
                                K = 1,
                                SB = 1,
                                CS = 1,
                                ParkRunFactor = 1
                            };

                            // Merge shared level stats together
                            foreach (var tg in teamGroups)
                            {
                                // Get stat rates at this team
                                var teamStats = tg.Aggregate(Utilities.HitterGameLogAggregation);
                                var statRates = new Model_LeagueHittingBaselines
                                {
                                    Year = -1,
                                    Month = -1,
                                    LeagueId = -1,
                                    LevelId = -1,
                                    Hit1B = Utilities.SafeDivide(teamStats.H - teamStats.Hit2B - teamStats.Hit3B - teamStats.HR, teamStats.PA, 0),
                                    Hit2B = Utilities.SafeDivide(teamStats.Hit2B, teamStats.PA, 0),
                                    Hit3B = Utilities.SafeDivide(teamStats.Hit3B, teamStats.PA, 0),
                                    HitHR = Utilities.SafeDivide(teamStats.HR, teamStats.PA, 0),
                                    BB = Utilities.SafeDivide(teamStats.BB, teamStats.PA, 0),
                                    HBP = Utilities.SafeDivide(teamStats.HBP, teamStats.PA, 0),
                                    K = Utilities.SafeDivide(teamStats.K, teamStats.PA, 0),
                                    SB = Utilities.SafeDivide(teamStats.SB, teamStats.PA, 0),
                                    CS = Utilities.SafeDivide(teamStats.CS, teamStats.PA, 0)
                                };

                                // Adjust to rates of the league
                                Model_LeagueHittingBaselines leagueBaseline = leagueBaselines.Where(f => f.LeagueId == teamStats.LeagueId).Single();
                                statRates.Hit1B = Utilities.SafeDivide(statRates.Hit1B, leagueBaseline.Hit1B);
                                statRates.Hit2B = Utilities.SafeDivide(statRates.Hit2B, leagueBaseline.Hit2B);
                                statRates.Hit3B = Utilities.SafeDivide(statRates.Hit3B, leagueBaseline.Hit3B);
                                statRates.HitHR = Utilities.SafeDivide(statRates.HitHR, leagueBaseline.HitHR);
                                statRates.BB = Utilities.SafeDivide(statRates.BB, leagueBaseline.BB);
                                statRates.HBP = Utilities.SafeDivide(statRates.HBP, leagueBaseline.HBP);
                                statRates.K = Utilities.SafeDivide(statRates.K, leagueBaseline.K);
                                statRates.SB = Utilities.SafeDivide(statRates.SB, leagueBaseline.SB);
                                statRates.CS = Utilities.SafeDivide(statRates.CS, leagueBaseline.CS);

                                // Get parkfactor
                                var stadiumIds = tg.Where(f => f.IsHome == 1).Select(f => f.StadiumId);
                                float parkFactor = stadiumIds.Any() ?
                                    db.Park_Factors.Where(f => f.Year == tg.Key.Year && f.StadiumId == stadiumIds.First()).Single().RunFactor :
                                    1.0f;
                                

                                // Combine to level stats based on PA
                                float thisProp = mhls.Pa == 0 ? 1.0f : (float)teamStats.PA / (mhls.Pa + teamStats.PA);
                                float otherProp = 1.0f - thisProp;
                                mhls.Pa += teamStats.PA;
                                mhls.Hit1B = (otherProp * mhls.Hit1B) + (thisProp * statRates.Hit1B);
                                mhls.Hit2B = (otherProp * mhls.Hit2B) + (thisProp * statRates.Hit2B);
                                mhls.Hit3B = (otherProp * mhls.Hit3B) + (thisProp * statRates.Hit3B);
                                mhls.HitHR = (otherProp * mhls.HitHR) + (thisProp * statRates.HitHR);
                                mhls.BB = (otherProp * mhls.BB) + (thisProp * statRates.BB);
                                mhls.HBP = (otherProp * mhls.HBP) + (thisProp * statRates.HBP);
                                mhls.K = (otherProp * mhls.K) + (thisProp * statRates.K);
                                mhls.SB = (otherProp * mhls.SB) + (thisProp * statRates.SB);
                                mhls.CS = (otherProp * mhls.CS) + (thisProp * statRates.CS);
                                mhls.ParkRunFactor = (otherProp * mhls.ParkRunFactor) + (thisProp * parkFactor);
                            }

                            db.Model_HitterLevelStats.Add(mhls);
                        }

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::HitterPlayerStats");
                Utilities.LogException(e);
                return false;
            }
        }

        public static bool PitcherPlayerStats(int year, int month, bool forceReset)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // User requested data deletion
                if (forceReset)
                {
                    db.Model_PitcherLevelStats.RemoveRange(db.Model_PitcherLevelStats.Where(f => f.Year == year && f.Month == month));
                    db.SaveChanges();
                }

                // Early exit if data exists
                if (db.Model_PitcherLevelStats.Where(f => f.Year == year && f.Month == month).Any())
                    return true;

                var games = db.Player_Pitcher_GameLog.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));
                var leagueBaselines = db.Model_LeaguePitchingBaselines.Where(f => f.Year == year && f.Month == month);
                var playerIds = games.Select(f => f.MlbId).Distinct();
                using (ProgressBar progressBar = new(playerIds.Count(), $"Calculating Model_RawPitcherStats for {year}-{month}"))
                {
                    // Group by player
                    foreach (var mlbId in playerIds)
                    {
                        // Group by League
                        var levelGroups = games.Where(f => f.MlbId == mlbId)
                            .GroupBy(f => f.LevelId);

                        foreach (var lvlG in levelGroups)
                        {
                            // Group by level
                            var teamGroups = lvlG.GroupBy(f => new { f.TeamId, f.Year });
                            Model_PitcherLevelStats mhls = new Model_PitcherLevelStats
                            {
                                MlbId = mlbId,
                                Year = year,
                                Month = month,
                                LevelId = lvlG.Key,
                                Outs_RP = 0,
                                Outs_SP = 0,
                                G = 0,
                                GS = 0,
                                ERA = 1,
                                FIP = 1,
                                HR = 1,
                                BB = 1,
                                HBP = 1,
                                K = 1,
                                ParkRunFactor = 1
                            };
                            int battersFaced = 0;

                            // Merge shared level stats together
                            foreach (var tg in teamGroups)
                            {
                                // Get stat rates at this team
                                var teamStats = tg.Aggregate(Utilities.PitcherGameLogAggregation);
                                var statRates = new Model_LeaguePitchingBaselines
                                {
                                    Year = -1,
                                    Month = -1,
                                    LeagueId = -1,
                                    LevelId = -1,
                                    ERA = Utilities.SafeDivide(teamStats.ER * 27, teamStats.Outs, teamStats.ER * 27),
                                    FIP = Utilities.CalculateFip(db.LeagueStats.Where(f => f.Year == tg.Key.Year && f.LeagueId == teamStats.LeagueId).Single().CFIP, teamStats.HR, teamStats.K, teamStats.BB + teamStats.HBP, teamStats.Outs),
                                    HR = Utilities.SafeDivide(teamStats.HR, teamStats.BattersFaced, 0),
                                    BB = Utilities.SafeDivide(teamStats.BB, teamStats.BattersFaced, 0),
                                    HBP = Utilities.SafeDivide(teamStats.HBP, teamStats.BattersFaced, 0),
                                    K = Utilities.SafeDivide(teamStats.K, teamStats.BattersFaced, 0),
                                };

                                // Adjust to rates of the league
                                Model_LeaguePitchingBaselines leagueBaseline = leagueBaselines.Where(f => f.LeagueId == teamStats.LeagueId).Single();
                                statRates.HR = Utilities.SafeDivide(statRates.HR, leagueBaseline.HR);
                                statRates.ERA = Utilities.SafeDivide(statRates.ERA, leagueBaseline.ERA);
                                statRates.FIP = Utilities.SafeDivide(statRates.FIP, leagueBaseline.FIP);
                                statRates.BB = Utilities.SafeDivide(statRates.BB, leagueBaseline.BB);
                                statRates.HBP = Utilities.SafeDivide(statRates.HBP, leagueBaseline.HBP);
                                statRates.K = Utilities.SafeDivide(statRates.K, leagueBaseline.K);

                                // Get parkfactor
                                var stadiumIds = tg.Where(f => f.IsHome == 1).Select(f => f.StadiumId);
                                float parkFactor = stadiumIds.Any() ?
                                    db.Park_Factors.Where(f => f.Year == tg.Key.Year && f.StadiumId == stadiumIds.First()).Single().RunFactor :
                                    1.0f;

                                // Combine to level stats based on PA
                                float thisProp = battersFaced == 0 ? 1.0f : (float)teamStats.BattersFaced / (battersFaced + teamStats.BattersFaced);
                                float otherProp = 1.0f - thisProp;
                                battersFaced += teamStats.BattersFaced;
                                mhls.ERA = (otherProp * mhls.ERA) + (thisProp * statRates.ERA);
                                mhls.FIP = (otherProp * mhls.FIP) + (thisProp * statRates.FIP);
                                mhls.HR = (otherProp * mhls.HR) + (thisProp * statRates.HR);
                                mhls.BB = (otherProp * mhls.BB) + (thisProp * statRates.BB);
                                mhls.HBP = (otherProp * mhls.HBP) + (thisProp * statRates.HBP);
                                mhls.K = (otherProp * mhls.K) + (thisProp * statRates.K);
                                mhls.ParkRunFactor = (otherProp * mhls.ParkRunFactor) + (thisProp * parkFactor);

                                // Add started/relief stats
                                var startedGames = tg.Where(f => f.Started == 1);
                                var reliefGames = tg.Where(f => f.Started == 0);
                                mhls.G += tg.Count();
                                mhls.GS += startedGames.Count();
                                mhls.Outs_SP += startedGames.Select(f => f.Outs).Sum();
                                mhls.Outs_RP += reliefGames.Select(f => f.Outs).Sum();
                            }

                            db.Model_PitcherLevelStats.Add(mhls);
                        }

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_RawStats::PitcherPlayerStats");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
