using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using System.Text.Json;
using static Db.DbEnums;
using static Db.DbTextColumns;

namespace DataAquisition
{
    internal class CalculateMonthFielding
    {
        private record PlayerPosition {
            public required int MlbId { get; set; }
            public required Position Pos { get; set; }
        }

        private record CurrentLineup {
            public required PlayerPosition IdP { get; set; }
            public required PlayerPosition IdC { get; set; }
            public required PlayerPosition Id1B { get; set; }
            public required PlayerPosition Id2B { get; set; }
            public required PlayerPosition Id3B { get; set; }
            public required PlayerPosition IdSS { get; set; }
            public required PlayerPosition IdLF { get; set; }
            public required PlayerPosition IdCF { get; set; }
            public required PlayerPosition IdRF { get; set; }

            public PlayerPosition ExtractPosition(Position pos)
            {
                switch (pos)
                {
                    case Position.P: return IdP;
                    case Position.C: return IdC;
                    case Position.B1: return Id1B;
                    case Position.B2: return Id2B;
                    case Position.B3: return Id3B;
                    case Position.SS: return IdSS;
                    case Position.LF: return IdLF;
                    case Position.CF: return IdCF;
                    case Position.RF: return IdRF;
                    default: throw new Exception("Unexpected position in ExtractPostion");
                }
            }
        }

        // Use dicts as static to not have to pass them around everywhere
        private static DoublePlayDict doublePlayDict = new();
        private static FieldingDict fieldingDict = new();
        private static BaserunningDict BsrAdv1st3rdSingleDict = new();
        private static BaserunningDict BsrAdv2ndHomeSingleDict = new();
        private static BaserunningDict BsrAdv1stHomeDoubleDict = new();
        private static BaserunningDict BsrAvoidForce2ndDict = new();
        private static BaserunningDict BsrAdv1st2ndFlyoutDict = new();
        private static BaserunningDict BsrAdv2nd3rdFlyoutDict = new();
        private static BaserunningDict BsrAdv3rdHomeFlyoutDict = new();
        private static BaserunningDict BsrAdv2nd3rdGroundoutDict = new();

        private static void WriteGIDPRuns(GamePlayByPlay pbp, Dictionary<PlayerPosition, Player_Fielder_MonthStats> playerDict, CurrentLineup lineup)
        {
            DoublePlayScenario? scenario = PBP_TypeConversions.GetDoublePlayScenario(pbp);
            if (scenario == null)
                return;

            DoublePlayResult expectedResult = doublePlayDict[scenario];

            float runsAboveExpected = 0;
            if (pbp.EndOuts - pbp.StartOuts >= 2)
                runsAboveExpected = expectedResult.RunsDP;
            else if (pbp.EndOuts == pbp.StartOuts)
                runsAboveExpected = expectedResult.RunsNeither;
            else if (pbp.Run1stOutcome == 0 || pbp.Run2ndOutcome == 0 || pbp.Run3rdOutcome == 0)
                runsAboveExpected = expectedResult.RunsLeading;
            else
                runsAboveExpected = expectedResult.RunsHitter;

            // Wrapped in try/catch because occasionally MLB messes up and a player has a one-off substitution and doesn't get included in gamestats
            // A single game for a single player at their non-primary position is insignificant, so safe to ignore
            if (scenario.Zone == 4 || scenario.Zone == 5)
            {
                try { playerDict[lineup.Id2B].R_GIDP -= runsAboveExpected; } catch { }
                try { playerDict[lineup.IdSS].R_GIDP -= runsAboveExpected; } catch { }
            }
            else if (scenario.Zone == 6)
            {
                try { playerDict[lineup.Id2B].R_GIDP -= runsAboveExpected; } catch { }
                try { playerDict[lineup.Id3B].R_GIDP -= runsAboveExpected; } catch { }
            } else if (scenario.Zone == 3)
            {
                try { playerDict[lineup.Id1B].R_GIDP -= runsAboveExpected; } catch { }
                try { playerDict[lineup.IdSS].R_GIDP -= runsAboveExpected; } catch { }
            }
        }

        private static void UpdateSpecificArmScenario(PlayerPosition pp, Dictionary<PlayerPosition, Player_Fielder_MonthStats> playerDict, IEnumerable<GamePlayByPlay> pbpEnumerable, Func<IEnumerable<GamePlayByPlay>, IEnumerable<GamePlayByPlay>> fCheck, int startBase, int endBase, BaserunningDict baserunningDict, BaserunningScenario scenario)
        {
            if (!fCheck(pbpEnumerable).Any())
                return;

            GamePlayByPlay pbp = pbpEnumerable.Single();
            int outcomeBase = -1;
            switch (startBase)
            {
                case 1: outcomeBase = pbp.Run1stOutcome.Value; break;
                case 2: outcomeBase = pbp.Run2ndOutcome.Value; break;
                case 3: outcomeBase = pbp.Run3rdOutcome.Value; break;
                default: throw new Exception($"Unspecified outcome base encounterd in UpdateSpecificArmScenario: {startBase}");
            }


            // Wrapped in try/catch because occasionally MLB messes up and a player has a one-off substitution and doesn't get included in gamestats
            // A single game for a single player at their non-primary position is insignificant, so safe to ignore
            // Player is out
            if (outcomeBase == 0)
                try { playerDict[pp].R_ARM -= baserunningDict[scenario].RunsOut; } catch { }
            // Player advanced
            else if (outcomeBase >= endBase)
                try { playerDict[pp].R_ARM -= baserunningDict[scenario].RunsAdvance; } catch { }
            // Player Stayed
            else
                try { playerDict[pp].R_ARM -= baserunningDict[scenario].RunsStay; } catch { }
        }

        private static void WriteArmRuns(GamePlayByPlay pbp, Dictionary<PlayerPosition, Player_Fielder_MonthStats> playerDict, CurrentLineup lineup)
        {
            // Arm is only for OF
            if (pbp.HitZone < 7 || pbp.HitZone == 78 || pbp.HitZone == 89)
                return;

            BaserunningScenario? scenario = PBP_TypeConversions.GetBaserunningScenario(pbp);
            if (scenario == null)
                return;

            // Get player to evaluate
            PlayerPosition pp = pbp.HitZone == 7 ? lineup.IdLF :
                            pbp.HitZone == 8 ? lineup.IdCF : lineup.IdRF;

            // Loop through different scenarios
            var pbpEnumerable = Enumerable.Repeat(pbp, 1);
            UpdateSpecificArmScenario(pp, playerDict, pbpEnumerable, PBP_Utilities.GetAdvance_1stTo3rdOnSingle_Opportunities, 1, 3, BsrAdv1st3rdSingleDict, scenario);
            UpdateSpecificArmScenario(pp, playerDict, pbpEnumerable, PBP_Utilities.GetAdvance_2ndToHomeOnSingle_Opportunities, 2, 4, BsrAdv2ndHomeSingleDict, scenario);
            UpdateSpecificArmScenario(pp, playerDict, pbpEnumerable, PBP_Utilities.GetAdvance_1stToHomeOnDouble_Opportunities, 1, 4, BsrAdv1stHomeDoubleDict, scenario);
            UpdateSpecificArmScenario(pp, playerDict, pbpEnumerable, PBP_Utilities.GetAdvance_1stTo2ndOnFlyout_Opportunities, 1, 2, BsrAdv1st2ndFlyoutDict, scenario);
            UpdateSpecificArmScenario(pp, playerDict, pbpEnumerable, PBP_Utilities.GetAdvance_2ndTo3rdOnFlyout_Opportunities, 2, 3, BsrAdv2nd3rdFlyoutDict, scenario);
            UpdateSpecificArmScenario(pp, playerDict, pbpEnumerable, PBP_Utilities.GetAdvance_3rdToHomeOnFlyout_Opportunities, 3, 4, BsrAdv3rdHomeFlyoutDict, scenario);
        }

        private static void WritePMRuns(GamePlayByPlay pbp, Dictionary<PlayerPosition, Player_Fielder_MonthStats> playerDict, CurrentLineup lineup)
        {
            FieldingScenario? scenario = PBP_TypeConversions.GetFieldingScenario(pbp);
            if (scenario == null)
                return;

            FieldingResults expectedResult = fieldingDict[scenario];

            if ((pbp.Result & PBP_HIT_EVENT) == 0)
            {
                Position zonePos = (Position)pbp.HitZone.Value;
                PlayerPosition pp = lineup.ExtractPosition(zonePos);
                try { playerDict[pp].R_PM -= expectedResult.RunsMake; } catch { }
                return;
            }

            // Assign blame to players based on chance they would make it if it was made
            // Wrapped in try/catch because occasionally MLB messes up and a player has a one-off substitution and doesn't get included in gamestats
            // A single game for a single player at their non-primary position is insignificant, so safe to ignore
            try { playerDict[lineup.IdP].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[0]; } catch { }
            try { playerDict[lineup.IdC].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[1]; } catch { }
            try { playerDict[lineup.Id1B].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[2]; } catch { }
            try { playerDict[lineup.Id2B].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[3]; } catch { }
            try { playerDict[lineup.Id3B].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[4]; } catch { }
            try { playerDict[lineup.IdSS].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[5]; } catch { }
            try { playerDict[lineup.IdLF].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[6]; } catch { }
            try { playerDict[lineup.IdCF].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[7]; } catch { }
            try { playerDict[lineup.IdRF].R_PM -= expectedResult.RunsMiss * expectedResult.ProbMakeWhenMade[8]; } catch { }
        }

        private static void UpdatePlayerStatsForGame(IEnumerable<GamePlayByPlay> pbp, GamePlayByPlay_GameFielders gf, Dictionary<PlayerPosition, Player_Fielder_MonthStats> playerDict)
        {
            CurrentLineup lineup = new() 
            { 
                IdP = new PlayerPosition { MlbId = gf.IdP, Pos = Position.P }, 
                IdC = new PlayerPosition { MlbId = gf.IdC, Pos = Position.C },
                Id1B = new PlayerPosition { MlbId = gf.Id1B, Pos = Position.B1 },
                Id2B = new PlayerPosition { MlbId = gf.Id2B, Pos = Position.B2 },
                Id3B = new PlayerPosition { MlbId = gf.Id3B, Pos = Position.B3 },
                IdSS = new PlayerPosition { MlbId = gf.IdSS, Pos = Position.SS },
                IdLF = new PlayerPosition { MlbId = gf.IdLF, Pos = Position.LF },
                IdCF = new PlayerPosition { MlbId = gf.IdCF, Pos = Position.CF },
                IdRF = new PlayerPosition { MlbId = gf.IdRF, Pos = Position.RF },
            };
            List<FielderSub> subs = JsonSerializer.Deserialize<List<FielderSub>>(gf.SubList) ?? throw new Exception($"Could not deserialize for gameId={pbp.First().GameId}");

            int subsIdx = 0; // Place to start looking in sub index
            int inning = 0; // Previous inning, used to reset inningIndex
            int inningIdx = 0; // Index of play in inning, used for sub index

            pbp = pbp.OrderBy(f => f.EventId).ToArray();
            foreach (var play in pbp)
            {
                if (play.Inning > inning)
                {
                    inning = play.Inning;
                    inningIdx = 0;
                }

                while (subsIdx < subs.Count && subs[subsIdx].Inning == play.Inning && subs[subsIdx].HalfInningEventNum == inningIdx)
                {
                    switch((Position)subs[subsIdx].Position)
                    {
                        case Position.P: lineup.IdP.MlbId = subs[subsIdx].MlbId; break;
                        case Position.C: lineup.IdC.MlbId = subs[subsIdx].MlbId; break;
                        case Position.B1: lineup.Id1B.MlbId = subs[subsIdx].MlbId; break;
                        case Position.B2: lineup.Id2B.MlbId = subs[subsIdx].MlbId; break;
                        case Position.B3: lineup.Id3B.MlbId = subs[subsIdx].MlbId; break;
                        case Position.SS: lineup.IdSS.MlbId = subs[subsIdx].MlbId; break;
                        case Position.LF: lineup.IdLF.MlbId = subs[subsIdx].MlbId; break;
                        case Position.CF: lineup.IdCF.MlbId = subs[subsIdx].MlbId; break;
                        case Position.RF: lineup.IdRF.MlbId = subs[subsIdx].MlbId; break;
                        default: break;
                    }
                    subsIdx++;
                }

                inningIdx++;
                if (play.EventFlag != GameFlags.Valid) // Event was flagged to be skipped for some reason
                    continue;

                WriteGIDPRuns(play, playerDict, lineup);
                WriteArmRuns(play, playerDict, lineup);
                WritePMRuns(play, playerDict, lineup);
            }
        }

        public static Dictionary<Position, float> GetExpectedErrorRate(IEnumerable<Player_Fielder_GameLog> logs)
        {
            Dictionary<Position, float> errorRate = new();
            Position[] positions = [Position.P, Position.C, Position.B1, Position.B2, Position.B3, Position.SS, Position.LF, Position.CF, Position.RF];
            foreach (var pos in positions)
            {
                int chances = logs.Where(f => f.Position == pos).Sum(f => f.Chances);
                int errors = logs.Where(f => f.Position == pos).Sum(f => f.Errors);
                errorRate[pos] = (float)errors / chances;
            }
            errorRate[Position.DH] = 0;

            return errorRate;
        }

        public static bool Update(int year, int month)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                db.Player_Fielder_MonthStats.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

                // Fieldings stats will be combined for MLB, so add combined MLB if doesn't exist
                if (!db.Leagues.Where(f => f.Id == 1).Any())
                {
                    db.Leagues.Add(new Leagues
                    {
                        Id = 1,
                        Name = "Major League Baseball",
                        Abbr = "MLB"
                    });
                    db.SaveChanges();
                }

                // Get logs for this month, grouping in March with april and october with september
                Player_Fielder_GameLog[] thisMonthsLogs;
                IQueryable<GamePlayByPlay> thisMonthsPBP;
                if (month == 4)
                {
                    thisMonthsLogs = db.Player_Fielder_GameLog.Where(f => f.Year == year && f.Month <= month).ToArray();
                    thisMonthsPBP = db.GamePlayByPlay.Where(f => f.Year == year && f.Month <= month);
                }
                else if (month == 9)
                {
                    thisMonthsLogs = db.Player_Fielder_GameLog.Where(f => f.Year == year && f.Month >= month).ToArray();
                    thisMonthsPBP = db.GamePlayByPlay.Where(f => f.Year == year && f.Month >= month);
                }
                else
                {
                    thisMonthsLogs = db.Player_Fielder_GameLog.Where(f => f.Year == year && f.Month == month).ToArray();
                    thisMonthsPBP = db.GamePlayByPlay.Where(f => f.Year == year && f.Month == month);
                }

                int[] leagueIds = thisMonthsLogs.Select(f => f.LeagueId).Distinct().ToArray();

                using (ProgressBar progressBar = new(leagueIds.Length, $"Calculating Month Fieldings Stats for {year}-{month}"))
                {
                    foreach(int leagueId in leagueIds)
                    {
                        int levelId = leagueId == 1 ? 1 : db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.LeagueId == leagueId).First().LevelId;
                        // Create hash table of all player-position combinations at this league this month
                        var playerPositions = thisMonthsLogs.Where(f => f.LeagueId == leagueId).Select(f => new PlayerPosition { MlbId=f.MlbId, Pos=f.Position }).Distinct();
                        Dictionary<PlayerPosition, Player_Fielder_MonthStats> playerDict = new(playerPositions.Count());
                        foreach (var pp in playerPositions)
                        {
                            playerDict.Add(pp, new Player_Fielder_MonthStats
                            {
                                MlbId = pp.MlbId,
                                Year = year,
                                Month = month,
                                LevelId = levelId,
                                LeagueId = leagueId,
                                Position = pp.Pos,
                                Chances = 0,
                                Errors = 0,
                                ThrowErrors = 0,
                                Outs = 0,
                                R_ERR = 0,
                                R_PM = 0,
                                PosAdjust = 0,
                                D_RAA = 0,
                                R_GIDP = 0,
                                R_ARM = 0,
                                R_SB = 0,
                                SB = 0,
                                CS = 0,
                                R_PB = 0,
                                PB = 0,
                            });
                        }

                        // Get dicts stored in LeagueRunMatrix
                        LeagueRunMatrix runMatrix = db.LeagueRunMatrix.Where(f => f.Year == year && f.LeagueId == leagueId).Single();
                        doublePlayDict = LeagueRunMatrixDicts.GetDoublePlayDict(runMatrix.DoublePlayDict);
                        fieldingDict = LeagueRunMatrixDicts.GetFieldingDict(runMatrix.FieldOutcomeDict);
                        BsrAdv1st3rdSingleDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAdv1st3rdSingleDict);
                        BsrAdv2ndHomeSingleDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAdv2ndHomeSingleDict);
                        BsrAdv1stHomeDoubleDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAdv1stHomeDoubleDict);
                        BsrAvoidForce2ndDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAvoidForce2ndDict);
                        BsrAdv1st2ndFlyoutDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAdv1st2ndFlyoutDict);
                        BsrAdv2nd3rdFlyoutDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAdv2nd3rdFlyoutDict);
                        BsrAdv3rdHomeFlyoutDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAdv3rdHomeFlyoutDict);
                        BsrAdv2nd3rdGroundoutDict = LeagueRunMatrixDicts.GetBaserunningDict(runMatrix.BsrAdv2nd3rdGroundoutDict);

                        // Iterate through PBP game by game
                        var leaguePBP = (leagueId == 1 ?    thisMonthsPBP.Where(f => f.LeagueId == 103 || f.LeagueId == 104) :
                                                            thisMonthsPBP.Where(f => f.LeagueId == leagueId))
                            .GroupBy(f => new { f.GameId, f.IsTop });
                        foreach (var gamePBP in leaguePBP)
                        {
                            var gameFielders = db.GamePlayByPlay_GameFielders.Where(f => f.GameId == gamePBP.Key.GameId && f.IsHome == (gamePBP.Key.IsTop == 1)).Single();
                            UpdatePlayerStatsForGame(gamePBP, gameFielders, playerDict);
                        }

                        // Update counting stats
                        Dictionary<Position, float> errorRates = GetExpectedErrorRate(thisMonthsLogs.Where(f => f.LeagueId == leagueId));
                        LeagueStats leagueStats = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId).Single();
                        var playerValues = playerDict.Values;
                        foreach (var pv in playerValues)
                        {
                            var stats = thisMonthsLogs.Where(f => f.MlbId == pv.MlbId && f.LeagueId == leagueId && f.Position == pv.Position).Aggregate(Utilities.FielderGameLogAggregation);
                            pv.Chances = stats.Chances;
                            pv.Errors = stats.Errors;
                            pv.ThrowErrors = stats.ThrowErrors;
                            pv.Outs = stats.Outs;
                            if (pv.Position == Position.C)
                            {
                                pv.SB = stats.SB;
                                pv.CS = stats.CS;
                                pv.PB = stats.PassedBall;
                                pv.R_SB = -((leagueStats.RunCS * stats.CS) + (leagueStats.RunSB + stats.SB));

                                // Get PB runs above average
                                float expectedPB = pv.Outs * leagueStats.PBPerOut;
                                float pbBelowAverage = expectedPB - stats.PassedBall;
                                pv.R_PB = pbBelowAverage * leagueStats.RunPB;
                            }
                            // Get error runs above average
                            float expectedErrors = errorRates[pv.Position] * stats.Chances;
                            float errorsBelowAverage = expectedErrors - stats.Errors;
                            pv.R_ERR = errorsBelowAverage * leagueStats.RunErr;

                            pv.PosAdjust = Utilities.CalculatePosValue(pv.Position, pv.Outs);
                            pv.D_RAA = pv.R_ERR + pv.R_PM + pv.R_GIDP + pv.R_ARM + pv.R_SB + pv.R_PB;
                        }
                        db.BulkInsert(playerValues);

                        progressBar.Tick();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateMonthFielding");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
