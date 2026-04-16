using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using System.Text.Json;
using static Db.DbEnums;

namespace DataAquisition
{
    internal class PitchData
    {
        private const int NUM_THREADS = 256; // Older games are really slow to get from server, so use a lot of threads since tons of time will be waiting
        private static int progress_bar_thread = 0;
        private static List<int> thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];
        private const int PITCHES_PER_GAME = 500;
        private const int GAMES_TO_DROP_INDEXES = 10000;

        private const string PBP_LOG_DIRECTORY = Constants.DATA_AQ_DIRECTORY + "/Logs/Statcast/";

        private static async Task<(List<PitchStatcast>, List<PitchNonStatcast>)> GetPBPThreadFunction(IEnumerable<int> gameIds, int thread_idx, ProgressBar progressBar, int totalCount)
        {
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            List<PitchStatcast> pbpStatcast = new(gameIds.Count() * PITCHES_PER_GAME);
            List<PitchNonStatcast> pbpNonStatcast = new(pbpStatcast.Count());
            IProgress<float> progress = progressBar.AsProgress<float>();

            foreach (int gameId in gameIds)
            {
                try
                {
                    (var statcast, var nonstatcast) = await GetPlayByPlayAsync(gameId, httpClient, db);
                    pbpStatcast.AddRange(statcast);
                    pbpNonStatcast.AddRange(nonstatcast);
                }
                catch (Exception e)
                {
                    File.WriteAllText(PBP_LOG_DIRECTORY + $"GetStatcast_{gameId}.txt", e.Message + "\n\n" + e.InnerException + "\n\n" + e.StackTrace);
                }

                thread_counts[thread_idx]++; // Allows for tracking progress

                if (thread_idx == progress_bar_thread) // Only update from single thread
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / totalCount);
                }
            }

            return (pbpStatcast, pbpNonStatcast);
        }

        private static PitchPaResult GetResult(JsonElement resultElement)
        {
            string? eventType = resultElement.GetProperty("type").GetString();
            if (eventType != "atBat")
                throw new Exception("Unexpected Event Type: " + eventType);

            // If game just ended, will not have event
            if (!resultElement.TryGetProperty("event", out var eventElement))
            {
                return PitchPaResult.Other;
            }
            string? eventString = eventElement.GetString();

            switch(eventString)
            {
                case "Strikeout":
                case "Strikeout Double Play":
                case "Strikeout Triple Play":
                    return PitchPaResult.Strikeout | PitchPaResult.Out;
                case "Groundout":
                case "Forceout":
                case "Fielders Choice":
                case "Fielders Choice Out":
                    return PitchPaResult.Groundout | PitchPaResult.Out;
                case "Flyout":
                case "Pop Out":
                case "Lineout":
                case "Sac Fly":
                case "Double Play":
                case "Triple Play":
                case "Sac Fly Double Play":
                    return PitchPaResult.Flyout | PitchPaResult.Out;
                case "Grounded Into DP":
                    return PitchPaResult.Groundout | PitchPaResult.GIDP | PitchPaResult.Out;
                case "Walk":
                    return PitchPaResult.BB;
                case "Hit By Pitch":
                    return PitchPaResult.HBP;
                case "Single":
                    return PitchPaResult.Hit1B;
                case "Double":
                    return PitchPaResult.Hit2B;
                case "Triple":
                    return PitchPaResult.Hit3B;
                case "Home Run":
                    return PitchPaResult.HitHR;

                case "Field Error":
                case "Error":
                    return PitchPaResult.Error;

                case "Intent Walk":
                case "Sac Bunt":
                case "Sac Bunt Double Play":
                case "Bunt Groundout":
                case "Bunt Pop Out":
                case "Bunt Lineout":
                case "Caught Stealing 2B":
                case "Caught Stealing 3B":
                case "Caught Stealing Home":
                case "Catcher Interference":
                case "Pickoff Caught Stealing 2B":
                case "Pickoff Caught Stealing 3B":
                case "Pickoff Caught Stealing Home":
                case "Pickoff 1B":
                case "Pickoff 2B":
                case "Pickoff 3B":
                case "Stolen Base 2B":
                case "Stolen Base 3B":
                case "Stolen Base Home":
                case "Runner Out":
                case "Wild Pitch":
                case "Pitching Substitution": // Bug on MLBAPI's end, but it has 0 pitch events so fine
                case "Runner Double Play":
                case "Balk":
                case "Pickoff Error 1B":
                case "Pickoff Error 2B":
                case "Pickoff Error 3B":
                case "Field Out":
                case "Batter Out":
                case "Ejection":
                case "Defensive Indiff":
                case "Passed Ball":
                case "Defensive Sub":
                case "Offensive Substitution":
                case "Injury":
                case "Other Advance":
                case "Defensive Switch":
                case "Cs Double Play":
                case "CS Double Play":
                case "Game Advisory":
                case "Official Scorer Ruling Pending":
                    return PitchPaResult.Other;

                default:
                    throw new Exception("Unexpected Event: " + eventString);
            }
        }

        private static (PitchResult, bool, bool, bool, bool) GetPitchResult(JsonElement pitchEvent)
        {
            var details = pitchEvent.GetProperty("details");
            bool isInPlay = details.GetProperty("isInPlay").GetBoolean();
            string? code = details.GetProperty("code").GetString();

            bool hadSwing = true;
            bool hadContact = true;
            bool skipPitch = false;
            PitchResult pitchResult;
            
            switch (code)
            {
                case "C":
                case "K":
                    pitchResult = PitchResult.CalledStrike;
                    hadSwing = false;
                    hadContact = false;
                    break;
                case "T":
                case "S":
                case "M":
                case "W":
                case "O":
                    pitchResult = PitchResult.SwingingStrike;
                    hadContact = false;
                    break;
                case "B":
                case "*B":
                    pitchResult = PitchResult.Ball;
                    hadSwing = false;
                    hadContact = false;
                    break;
                case "H":
                    pitchResult = PitchResult.HBP;
                    hadSwing = false;
                    hadContact = false;
                    break;
                case "F":
                case "L":
                    pitchResult = PitchResult.Foul;
                    break;
                case "D":
                case "X":
                case "E":
                case "Z":
                case "Y":
                case "J":
                    pitchResult = PitchResult.InPlay;
                    break;
                case "V": // Automatic/Intent Ball/Strike, don't log
                case "I":
                case "P":
                case "R":
                case "Q":
                    skipPitch = true;
                    pitchResult = PitchResult.Ball; // Won't matter
                    break;
                default:
                    throw new Exception($"Unexpected code found: {code}");
            }

            return (pitchResult, hadSwing, hadContact, isInPlay, skipPitch);
        }

        private static BaseOccupancy OccupancyArrayToInt(List<int?> occupancy)
        {
            return (occupancy[0] != null ? BaseOccupancy.B1 : BaseOccupancy.Empty) |
                    (occupancy[1] != null ? BaseOccupancy.B2 : BaseOccupancy.Empty) |
                    (occupancy[2] != null ? BaseOccupancy.B3 : BaseOccupancy.Empty);
        }

        private static int JsonElementToBaseInt(JsonElement b)
        {
            if (b.ValueKind == JsonValueKind.Null)
                return -1;

            string b_string = b.ToString();
            if (b_string == "1B")
                return 0;
            if (b_string == "2B")
                return 1;
            if (b_string == "3B")
                return 2;
            if (b_string == "score" || b_string == "4B")
                return 3;

            if (b_string == "0B") // Not sure, definately not coded correctly
                return -1;

            throw new Exception("JsonElementToBaseInt did not find a proper base");
        }

        private static (float?, float?, float?, float?, float?) GetHitData(JsonElement pitchElement)
        {
            if (pitchElement.TryGetProperty("hitData", out var hitData) && hitData.TryGetProperty("launchSpeed", out var launchElement))
            {
                float? launchSpeed = GetPropertyIfExistsFloat(hitData, "launchSpeed");
                float? launchAngle = GetPropertyIfExistsFloat(hitData, "launchAngle");
                float? totalDistance = GetPropertyIfExistsFloat(hitData, "totalDistance");

                JsonElement coord = hitData.GetProperty("coordinates");
                float? coordX = GetPropertyIfExistsFloat(coord, "coordX");
                float? coordY = GetPropertyIfExistsFloat(coord, "coordY");

                return (launchSpeed, launchAngle, totalDistance, coordX, coordY);
            }
            return (null, null, null, null, null);
        }

        private static PitchType GetPitchType(string code)
        {
            switch(code)
            {
                case "FF":
                    return PitchType.Fourseam;
                case "SI":
                    return PitchType.Sinker;
                case "FA":
                    return PitchType.Fastball;
                case "FT":
                    return PitchType.Twoseam;
                case "FC":
                    return PitchType.Cutter;
                case "FS":
                    return PitchType.Splitter;
                case "CH":
                    return PitchType.Changeup;
                case "FO":
                    return PitchType.Forkball;
                case "SL":
                    return PitchType.Slider;
                case "CU":
                    return PitchType.Curveball;
                case "KC":
                    return PitchType.KnuckleCurve;
                case "ST":
                    return PitchType.Sweeper;
                case "SV":
                    return PitchType.Slurve;
                case "EP":
                    return PitchType.Eephus;
                case "KN":
                    return PitchType.Knuckleball;
                case "SC":
                    return PitchType.Screwball;
                case "CS":
                    return PitchType.SlowCurve;
                case "":
                case "UN":
                case "AB":
                case "PO":
                case "K":
                    return PitchType.Unknown;
                default:
                    throw new Exception("Pitch Code Not Handled: " + code);
            }
        }

        private static int? GetPropertyIfExists(JsonElement element, string property)
        {
            if (element.TryGetProperty(property, out var propertyElement))
                return propertyElement.GetInt32();
            return null;
        }

        private static float? GetPropertyIfExistsFloat(JsonElement element, string property)
        {
            if (element.TryGetProperty(property, out var propertyElement))
                return (float)propertyElement.GetDouble();
            return null;
        }

        private static async Task<(List<PitchStatcast>, List<PitchNonStatcast>)> GetPlayByPlayAsync(int gameId, HttpClient httpClient, SqliteDbContext db)
        {
            List<PitchStatcast> statcastPitches = new(PITCHES_PER_GAME);
            List<PitchNonStatcast> nonStatcastPitches = new(PITCHES_PER_GAME);

            int gamePitchIndex = 0;
            var date = db.Player_Hitter_GameLog.Where(f => f.GameId == gameId).Select(f => new { f.Year, f.Month, f.LeagueId, f.LevelId }).First();

            HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/game/{gameId}/playByPlay");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting Game log for id={gameId}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(responseBody);
            var allPlaysArray = json.RootElement.GetProperty("allPlays").EnumerateArray();

            var playsByInningElement = json.RootElement.GetProperty("playsByInning").EnumerateArray();
            // Track current state, set to impossible values to make sure they are properly getting reset upon entering
            // Both base-occupancy and outs may change due to baserunning moves, but will just ignore, they will average out
            // Only using to see if there are different results for different situations
            int currentOuts = -1;
            List<int?> baseOccupancy = [null, null, null], endBaseOccupancy = [null, null, null];
            int currentInning = 0;
            bool currentInningTop = false;
            int runsScoredInning = -1;
            foreach (var play in allPlaysArray)
            {
                // Check to see if new half-inning
                JsonElement about = play.GetProperty("about");
                int inning = about.GetProperty("inning").GetInt32();
                bool isTop = about.GetProperty("isTopInning").GetBoolean();
                if (inning != currentInning || isTop != currentInningTop)
                {
                    currentOuts = 0;
                    baseOccupancy = [null, null, null];
                    endBaseOccupancy = [null, null, null];
                    currentInning = inning;
                    currentInningTop = isTop;
                    runsScoredInning = 0;

                    // Go through all plays in inning, see how many runners scored
                    var indInningPlays = playsByInningElement.ElementAt(inning - 1);
                    var halfInningPlays = (isTop ? indInningPlays.GetProperty("top") : indInningPlays.GetProperty("bottom"))
                        .EnumerateArray().Select(f => f.GetInt32());

                    foreach (int playIdx in halfInningPlays)
                    {
                        var p = allPlaysArray.ElementAt(playIdx);
                        var runn = p.GetProperty("runners").EnumerateArray();
                        foreach (var r in runn)
                        {
                            if (r.TryGetProperty("movement", out var movementProperty)
                                && movementProperty.TryGetProperty("end", out var endProperty)
                                && endProperty.GetString() == "score")
                            {
                                runsScoredInning++;
                            }
                        }
                    }
                }

                // Get results for all pitches this play
                PitchPaResult playResult = GetResult(play.GetProperty("result"));
                JsonElement matchup = play.GetProperty("matchup");
                int hitterId = matchup.GetProperty("batter").GetProperty("id").GetInt32();
                bool isBatR = matchup.GetProperty("batSide").GetProperty("code").GetString() == "R";
                int pitcherId = matchup.GetProperty("pitcher").GetProperty("id").GetInt32();
                bool isPitR = matchup.GetProperty("pitchHand").GetProperty("code").GetString() == "R";

                // Get pitches
                var pitchIndexArray = play.GetProperty("pitchIndex").EnumerateArray().Select(f => f.GetInt32());
                var playEvents = play.GetProperty("playEvents");
                int countBalls = 0;
                int countStrikes = 0;

                // Update occupancy
                var runners = play.GetProperty("runners").EnumerateArray();
                foreach (var runner in runners)
                {
                    var movement = runner.GetProperty("movement");
                    int originBase = JsonElementToBaseInt(movement.GetProperty("originBase"));
                    int startBase = JsonElementToBaseInt(movement.GetProperty("start"));
                    int endBase = JsonElementToBaseInt(movement.GetProperty("end"));

                    var runDetails = runner.GetProperty("details");
                    int runnerId = runDetails.GetProperty("runner").GetProperty("id").GetInt32();

                    if (endBase != -1 && endBase < 3)
                        endBaseOccupancy[endBase] = runnerId;

                    if (originBase >= 0 && originBase < 3 && endBaseOccupancy[originBase] == runnerId)
                        endBaseOccupancy[originBase] = null;
                    if (startBase >= 0 && startBase < 3 && endBaseOccupancy[startBase] == runnerId)
                        endBaseOccupancy[startBase] = null;
                }

                // Get Runs scored this Pa and this inning
                int runsScoredThisPa = runners.Count(f => f.TryGetProperty("movement", out var movementProperty) && movementProperty.TryGetProperty("end", out var endProperty) && endProperty.GetString() == "score");
                int outsThisPa = runners.Count(f => f.TryGetProperty("movement", out var movementProperty) && movementProperty.TryGetProperty("isOut", out var isOutProperty) && (isOutProperty.ValueKind != JsonValueKind.Null) && isOutProperty.GetBoolean());
                runsScoredInning -= runsScoredThisPa;

                foreach (int pitchIndex in pitchIndexArray)
                {
                    // Get what happened on pitch
                    var pitchEvent = playEvents[pitchIndex];

                    // Make sure that event is an actual pitch
                    bool isPitch = pitchEvent.GetProperty("isPitch").GetBoolean();
                    if (!isPitch)
                        continue;

                    (var pitchResult, var hadSwing, var hadContact, var isInPlay, var skipPitch) = GetPitchResult(pitchEvent);
                    
                    // For event that wasn't a pitch (automatic ball/strike), don't log
                    if (skipPitch)
                        continue;

                    if (!pitchEvent.TryGetProperty("pitchData", out var pitchData))
                        continue;

                    // Need to check if statcast data exists or not
                    // Pre-15 had PitchFx, which will just pollute data
                    if (date.Year >= 2015 && pitchData.TryGetProperty("startSpeed", out var startSpeedElement))
                    {
                        (var launchSpeed, var launchAngle, var totalDist, var hitCoordX, var hitCoordY) = GetHitData(pitchEvent);

                        var coordinates = pitchData.GetProperty("coordinates");
                        var breaks = pitchData.GetProperty("breaks");

                        #pragma warning disable CS8604 // API will not return null
                        PitchType pitchType = GetPitchType(pitchEvent.GetProperty("details").GetProperty("type").GetProperty("code").GetString());
                        #pragma warning restore CS8604

                        int? spinRate = null, spinDirection = null;
                        if (breaks.TryGetProperty("spinRate", out var spinRateElement) && breaks.TryGetProperty("spinDirection", out var spinDirectionElement))
                        {
                            spinRate = spinRateElement.GetInt32();
                            spinDirection = spinDirectionElement.GetInt32();
                        }

                        statcastPitches.Add(new PitchStatcast
                        {
                            GameId = gameId,
                            PitchId = gamePitchIndex,
                            PitcherId = pitcherId,
                            HitterId = hitterId,
                            LeagueId = date.LeagueId,
                            LevelId = date.LevelId,
                            Year = date.Year,
                            Month = date.Month,
                            CountBalls = countBalls,
                            CountStrike = countStrikes,
                            Outs = currentOuts,
                            BaseOccupancy = OccupancyArrayToInt(baseOccupancy),

                            PitchType = pitchType,
                            PaResult = playResult,
                            PaResultOccupancy = OccupancyArrayToInt(endBaseOccupancy),
                            PaResultOuts = outsThisPa,
                            PaResultDirectRuns = runsScoredThisPa,
                            RunsAfterPa = runsScoredInning,

                            Result = pitchResult,
                            HadSwing = hadSwing,
                            HadContact = hadContact,
                            IsInPlay = isInPlay,
                            HitIsR = isBatR,
                            PitIsR = isPitR,
                            VX = GetPropertyIfExistsFloat(coordinates, "vX0"),
                            VY = GetPropertyIfExistsFloat(coordinates, "vY0"),
                            VZ = GetPropertyIfExistsFloat(coordinates, "vZ0"),
                            VStart = GetPropertyIfExistsFloat(pitchData, "startSpeed"),
                            VEnd = GetPropertyIfExistsFloat(pitchData, "endSpeed"),
                            AX = GetPropertyIfExistsFloat(coordinates, "aX"),
                            AY = GetPropertyIfExistsFloat(coordinates, "aY"),
                            AZ = GetPropertyIfExistsFloat(coordinates, "aZ"),
                            PfxX = GetPropertyIfExistsFloat(coordinates, "pfxX"),
                            PfxZ = GetPropertyIfExistsFloat(coordinates, "pfxZ"),
                            BreakAngle = GetPropertyIfExistsFloat(breaks, "breakAngle"),
                            BreakVertical = GetPropertyIfExistsFloat(breaks, "breakVertical"),
                            BreakInduced = GetPropertyIfExistsFloat(breaks, "breakVerticalInduced"),
                            BreakHorizontal = GetPropertyIfExistsFloat(breaks, "breakHorizontal"),
                            SpinRate = GetPropertyIfExists(breaks, "spinRate"),
                            SpinDirection = GetPropertyIfExists(breaks, "spinDirection"),
                            PX = GetPropertyIfExistsFloat(coordinates, "pX"),
                            PZ = GetPropertyIfExistsFloat(coordinates, "pZ"),
                            ZoneTop = GetPropertyIfExistsFloat(pitchData, "strikeZoneTop"),
                            ZoneBot = GetPropertyIfExistsFloat(pitchData, "strikeZoneBottom"),
                            Extension = GetPropertyIfExistsFloat(pitchData, "extension"),
                            X0 = GetPropertyIfExistsFloat(coordinates, "x0"),
                            Y0 = GetPropertyIfExistsFloat(coordinates, "y0"),
                            Z0 = GetPropertyIfExistsFloat(coordinates, "z0"),
                            PlateTime = GetPropertyIfExistsFloat(pitchData, "plateTime"),
                            LaunchSpeed = launchSpeed,
                            LaunchAngle = launchAngle,
                            TotalDist = totalDist,
                            HitCoordX = hitCoordX,
                            HitCoordY = hitCoordY,
                            RunValueHitter = -100000, // Will be modified later
                        });
                    } else {
                        nonStatcastPitches.Add(new PitchNonStatcast
                        {
                            GameId = gameId,
                            PitchId = gamePitchIndex,
                            PitcherId = pitcherId,
                            HitterId = hitterId,
                            Year = date.Year,
                            Month = date.Month,
                            LeagueId = date.LeagueId,
                            LevelId = date.LevelId,
                            CountBalls = countBalls,
                            CountStrike = countStrikes,
                            Outs = currentOuts,
                            BaseOccupancy = OccupancyArrayToInt(baseOccupancy),
                            PaResult = playResult,
                            Result = pitchResult,
                            HadSwing = hadSwing,
                            HadContact = hadContact,
                            IsInPlay = isInPlay,
                            HitIsR = isBatR,
                            PitIsR = isPitR,
                            RunValueHitter = -100000, // Will be modified later
                        });
                    }

                    gamePitchIndex++;

                    // Update count, outs
                    JsonElement count = pitchEvent.GetProperty("count");
                    countBalls = count.GetProperty("balls").GetInt32();
                    countStrikes = count.GetProperty("strikes").GetInt32();
                    currentOuts = count.GetProperty("outs").GetInt32();
                }

                for (int i = 0; i < 3; i++)
                    baseOccupancy[i] = endBaseOccupancy[i];
            }

            return (statcastPitches, nonStatcastPitches);
        }

        public static async Task<bool> Update(int year, bool createIndex)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                var alreadyLoggedStatcast = db.PitchStatcast.Where(f => f.Year == year).Select(f => f.GameId).Distinct().ToList().Order();
                var alreadyLoggedNonStatcast = db.PitchNonStatcast.Where(f => f.Year == year).Select(f => f.GameId).Distinct().ToList().Order();
                var gameIds = db.Player_Hitter_GameLog.Where(f => f.Year == year && !alreadyLoggedStatcast.Contains(f.GameId) && !alreadyLoggedNonStatcast.Contains(f.GameId)).Select(f => f.GameId).Distinct().ToList();

                if (!gameIds.Any())
                    return true;

                // Drop indexes to speed up inserts
                bool updateIndices = gameIds.Count() > GAMES_TO_DROP_INDEXES;
                if (updateIndices)
                {
                    // Drop indexes to speedup insertion
                    List<string> indexNames = ["idx_PitchStatcastPitcher", "idx_PitchStatcastHitter", "idx_PitchStatcastYearMonth", "idx_PitchNonStatcastPitcher", "idx_PitchNonStatcastHitter", "idx_PitchNonStatcastYearMonth"];
                    foreach (var name in indexNames)
                    {
                        try
                        {
                            #pragma warning disable EF1002 // No possible SQL injection, name is compile-time
                            db.Database.ExecuteSqlRaw($"DROP INDEX {name};");
                            #pragma warning restore EF1002
                        }
                        catch (Exception)
                        {
                            // Index doesn't exist, so fine
                        }
                    }
                }

                // Split ids into groups for tasks
                int j = 0;
                IEnumerable<IEnumerable<int>> id_partitions = from item in gameIds
                                                              group item by j++ % NUM_THREADS into part
                                                              select part.AsEnumerable();

                progress_bar_thread = 0;
                thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];
                List<Task<(List<PitchStatcast>, List<PitchNonStatcast>)>> tasks = new(NUM_THREADS);

                // Use transaction to speed up insert
                await using var transaction = await db.Database.BeginTransactionAsync();
                {
                    try
                    {
                        // Get data
                        using (ProgressBar progressBar = new(gameIds.Count(), $"Generating Statcast Data for {year}"))
                        {
                            // Send out gameIds for threads to collect data
                            for (int i = 0; i < NUM_THREADS; i++)
                            {
                                if (i >= id_partitions.Count())
                                    break;
                                int idx = i;
                                Task<(List<PitchStatcast>, List<PitchNonStatcast>)> task = GetPBPThreadFunction(id_partitions.ElementAt(i), idx, progressBar, gameIds.Count());
                                tasks.Add(task);
                            }

                            // Collect data from threads, insert to db
                            List<List<PitchStatcast>> gamesStatcast = new();
                            List<List<PitchNonStatcast>> gamesNonStatcast = new();
                            foreach (var task in tasks)
                            {
                                (var pbpList, var subList) = await task;
                                gamesStatcast.Add(pbpList);
                                gamesNonStatcast.Add(subList);

                                progress_bar_thread++;
                            }

                            // Need to wait until all threads are done to start inserting to prevent locking the db
                            foreach (var games in gamesStatcast)
                            {
                                await db.BulkInsertAsync(games);
                            }
                            foreach (var games in gamesNonStatcast)
                            {
                                await db.BulkInsertAsync(games);
                            }

                            progressBar.Tick(gameIds.Count());
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                if (updateIndices && createIndex)
                {
                    try
                    {
                        // Restore indexes
                        db.Database.ExecuteSql($"CREATE INDEX idx_PitchStatcastPitcher ON PitchStatcast(PitcherId, Year, Month, LevelId, LeagueId);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PitchStatcastHitter ON PitchStatcast(HitterId, Year, Month, LevelId, LeagueId);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PitchStatcastYearMonth ON PitchStatcast(Year, Month, LevelId, LeagueId);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PitchNonStatcastPitcher ON PitchNonStatcast(PitcherId, Year, Month, LevelId, LeagueId);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PitchNonStatcastHitter ON PitchNonStatcast(HitterId, Year, Month, LevelId, LeagueId);");
                        db.Database.ExecuteSql($"CREATE INDEX idx_PitchNonStatcastYearMonth ON PitchNonStatcast(Year, Month, LevelId, LeagueId);");
                    }
                    catch (Exception)
                    {
                        // Fails if they already exist, which means they don't need to be created
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetPlayByPlay");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
