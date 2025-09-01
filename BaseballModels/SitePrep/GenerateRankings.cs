using Db;
using ShellProgressBar;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SitePrep
{
    internal class GenerateRankings
    {
        internal class PlayerWar {
            public required int MlbId;
            public required string ModelName;
            public required float War;
            public required int Month;
            public required int Year;
        }

        internal class PlayerMonthWar {
            public required int MLbId;
            public required string ModelName;
            public required string position;
            public required float War;
            public required int ParentOrgId;
        }

        internal class PlayerYearPosition {
            public required int Year;
            public required string position;
        }

        private static JsonObject GetJson(PlayerMonthWar pmw, SqliteDbContext db)
        {
            JsonObject player = new()
            {
                ["war"] = Math.Round((double)pmw.War, 1),
                ["id"] = pmw.MLbId,
                ["model"] = pmw.ModelName,
                ["team"] = pmw.ParentOrgId,
                ["name"] = db.Player.Where(f => f.MlbId == pmw.MLbId)
                                    .Select(f => f.UseFirstName + " " + f.UseLastName)
                                    .Single(),
                ["position"] = pmw.position
            };

            return player;
        }

        public static bool Main(int endYear, int endMonth)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // Create Position List
                Dictionary<int, List<PlayerYearPosition>> playerYearPositions = new();
                using (ProgressBar progressBar = new ProgressBar(db.Model_Players.Count(), "Creating playerYearPositions"))
                {
                    foreach (var mp in db.Model_Players)
                    {
                        List<PlayerYearPosition> pyps = new();
                    
                        var years = mp.IsHitter == 1 ?
                            db.Player_Hitter_YearAdvanced.Where(f => f.MlbId == mp.MlbId).Select(f => f.Year).OrderBy(f => f) :
                            db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == mp.MlbId).Select(f => f.Year).OrderBy(f => f);
                        if (mp.IsPitcher == 1)
                        {
                            foreach (var y in years)
                            {
                                pyps.Add(new PlayerYearPosition
                                {
                                    Year = y,
                                    position = "P"
                                });
                            }
                        }
                        else {
                            foreach (var y in years)
                            {
                                pyps.Add(new PlayerYearPosition
                                {
                                    Year = y,
                                    position = Utilities.GetPosition(db, mp.MlbId, y)
                                });
                            }
                        }

                        playerYearPositions.Add(mp.MlbId, pyps);

                        progressBar.Tick();
                    }
                }

                // Get intial value for every player
                var initial_pwa = db.Output_PlayerWarAggregation.Where(f => f.Year == 0)
                    .Join(db.Player, opwa => opwa.MlbId, p => p.MlbId, (opwa, p) => new PlayerWar{ 
                        MlbId = opwa.MlbId,
                        ModelName = opwa.ModelName,
                        War = Utilities.GetWar(opwa),
                        Month = p.SigningMonth.Value,
                        Year = p.SigningYear.Value,
                    });

                // Get ordered list of values for players
                List<List<PlayerWar>> playersWarList = new(initial_pwa.Count());
                using (ProgressBar progressBar = new ProgressBar(initial_pwa.Count(), "Creating PlayersWarList"))
                {
                    foreach (var pwa in initial_pwa)
                    {
                        List<PlayerWar> pw = [];
                        if (pwa.MlbId == 656941)
                            pw = [];
                        var opwas = db.Output_PlayerWarAggregation.Where(f => f.MlbId == pwa.MlbId && f.Year > 0 && f.ModelName.Equals(pwa.ModelName))
                            .OrderBy(f => f.Year).ThenBy(f => f.Month)
                            .Select(f => new PlayerWar
                            {
                                MlbId = f.MlbId,
                                ModelName = pwa.ModelName,
                                War = Utilities.GetWar(f),
                                Month = f.Month,
                                Year = f.Year,
                            }
                        );

                        // Add initial if month is different than last
                        if (opwas.Any())
                        {
                            if (pwa.Month != opwas.First().Month || pwa.Year != opwas.First().Year)
                                pw = [pwa];

                            pw.AddRange(opwas);
                        }
                        else
                        {
                            pw = [pwa];
                        }

                        // Add value at last month that should be considered if it doesn't exist
                        var mp = db.Model_Players.Where(f => f.MlbId == pwa.MlbId).Single();
                        if (mp.LastProspectYear != pw.Last().Year || mp.LastProspectMonth != pw.Last().Month)
                        {
                            var last = pw.Last();
                            // Check if player exhausted prospect status
                            if (mp.LastProspectYear <= endYear)
                            {
                                pw.Add(new PlayerWar
                                {
                                    MlbId = last.MlbId,
                                    ModelName = last.ModelName,
                                    War = last.War,
                                    Month = mp.LastProspectMonth,
                                    Year = mp.LastProspectYear,
                                });
                            }
                            else // Player is still a prospect
                            { 
                                if (last.Month != endMonth || last.Year != endYear)
                                {
                                    pw.Add(new PlayerWar
                                    {
                                        MlbId = last.MlbId,
                                        ModelName = last.ModelName,
                                        War = last.War,
                                        Month = endMonth,
                                        Year = endYear
                                    });
                                }
                            }
                            
                        }

                        playersWarList.Add(pw);
                        progressBar.Tick();
                    }
                }

                // Iterate through months
                int year = 2015;
                int month = 4;

                List<(int, int)> dates = new();
                while (year <= endYear || (year == endYear && month <= endMonth))
                {
                    dates.Add((month, year));
                    month++;
                    if (month > 9)
                    {
                        month = 4;
                        year++;
                    }
                }

                // Create JSON of dates for site
                JsonObject datesJson = new();
                datesJson.Add("endYear", endYear);
                datesJson.Add("endMonth", endMonth);
                datesJson.Add("startYear", 2015);
                using var fileStreamDates = new FileStream(Constants.SITE_ASSET_FOLDER + $"ranking/dates.json.gz", FileMode.Create);
                using var gzipStreamDates = new GZipStream(fileStreamDates, CompressionLevel.Optimal);
                using var writerDates = new Utf8JsonWriter(gzipStreamDates, new JsonWriterOptions { Indented = false });
                JsonSerializer.Serialize(writerDates, datesJson);

                // Go through all month combos, looking at all players
                using (ProgressBar progressBar = new ProgressBar(dates.Count(), "Generating Rankings"))
                {
                    foreach (var date in dates)
                    {
                        month = date.Item1;
                        year = date.Item2;

                        List<PlayerMonthWar> pmwList = new();
                        using (ChildProgressBar child = progressBar.Spawn(playersWarList.Count, $"Getting Players for {month}-{year}"))
                        {
                            foreach (var playerWarList in playersWarList)
                            {
                                child.Tick();
                                // Player not yet signed
                                var first = playerWarList.First();
                                if (first.Year > year || (first.Year == year && first.Month > month))
                                    continue;

                                // Player no longer a prospect
                                var last = playerWarList.Last();
                                if (last.Year < year || (last.Year == year && last.Month < month))
                                    continue;

                                // Player is prospect now
                                // Get first value that is <= currentDate
                                PlayerWar current = playerWarList.Where(f => f.Year < year || (f.Year == year && f.Month <= month))
                                    .OrderByDescending(f => f.Year).ThenByDescending(f => f.Month).First();


                                // Get previous team the player was on
                                var poms = db.Player_OrgMap.Where(f => f.MlbId == current.MlbId &&
                                        (f.Year < year || (f.Year == year && f.Month <= month)))
                                        .OrderByDescending(f => f.Year).ThenByDescending(f => f.Month);

                                if (!poms.Any()) // Missing some transaction data, time is before all data
                                {
                                    // Get unfiltered, first entry will be initial team
                                    poms = db.Player_OrgMap.Where(f => f.MlbId == current.MlbId)
                                        .OrderBy(f => f.Year).ThenBy(f => f.Month);
                                }

                                // Get Position
                                string position = "";
                                var pyps = playerYearPositions[current.MlbId];
                                if (!pyps.Any()) // No playing time, get stored from Mlb
                                    position = db.Site_PlayerBio.Single(f => f.Id == current.MlbId).Position;
                                else {
                                    var prevCurrentPyps = pyps.Where(f => f.Year <= year) // All values <= selected year
                                        .OrderByDescending(f => f.Year);
                                    if (prevCurrentPyps.Any())
                                        position = prevCurrentPyps.First().position; // Get most recent
                                    else
                                        position = pyps.OrderBy(f => f.Year).First().position; // Get next value (slight future bias)
                                }

                                pmwList.Add(new PlayerMonthWar
                                {
                                    MLbId = current.MlbId,
                                    ModelName = current.ModelName,
                                    War = current.War,
                                    ParentOrgId = poms.Any() ? poms // Few players only played on VSL teams that were multiple teams (no parent)
                                        .First().ParentOrgId : 0,
                                    position = position
                                });
                            }
                        }

                        // Create overall ranking
                        pmwList = pmwList.OrderByDescending(f => f.War).ToList();

                        // Log Ranking
                        using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
                        {
                            int rank = 1;
                            db_write.Ranking_Prospect.RemoveRange(db_write.Ranking_Prospect.Where(f => f.Month == month && f.Year == year));
                            db_write.SaveChanges();
                            foreach (var pmw in pmwList)
                            {
                                int r = rank;
                                db_write.Ranking_Prospect.Add(new Ranking_Prospect
                                {
                                    MlbId = pmw.MLbId,
                                    Year = year,
                                    Month = month,
                                    Model = pmw.ModelName,
                                    Rank = r
                                });
                                rank++;
                            }
                            db_write.SaveChanges();
                        }

                        JsonObject json = new();
                        JsonArray rankingJson = new();
                        foreach (var pmw in pmwList)
                        {
                            rankingJson.Add(GetJson(pmw, db));
                        }
                        json.Add("players", rankingJson);

                        using var fileStream = new FileStream(Constants.SITE_ASSET_FOLDER + $"ranking/{month}-{year}.json.gz", FileMode.Create);
                        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
                        using var writer = new Utf8JsonWriter(gzipStream, new JsonWriterOptions { Indented = false });
                        JsonSerializer.Serialize(writer, json);

                        // Create Team Rankings
                        var teamIds = db.Team_Parents.Select(f => f.Id);
                        foreach (var teamId in teamIds)
                        {
                            var teamList = pmwList.Where(f => f.ParentOrgId == teamId)
                                .OrderByDescending(f => f.War);

                            JsonObject teamJson = new();
                            JsonArray teamArray = new();
                            foreach (var pmw in teamList)
                            {
                                teamArray.Add(GetJson(pmw, db));
                            }
                            teamJson.Add("players", teamArray);

                            using var fileStreamTeam = new FileStream(Constants.SITE_ASSET_FOLDER + $"ranking/teams/{teamId}-{month}-{year}.json.gz", FileMode.Create);
                            using var gzipStreamTeam = new GZipStream(fileStreamTeam, CompressionLevel.Optimal);
                            using var writerTeam = new Utf8JsonWriter(gzipStreamTeam, new JsonWriterOptions { Indented = false });
                            JsonSerializer.Serialize(writerTeam, teamJson);
                        }

                        progressBar.Tick();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GenerateRankings");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
