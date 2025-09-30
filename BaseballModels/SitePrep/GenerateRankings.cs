using Db;
using ShellProgressBar;
using SiteDb;
using System.IO.Compression;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SitePrep
{
    internal class GenerateRankings
    {
        internal class PlayerWar {
            public required int MlbId;
            public required int ModelId;
            public required float War;
            public required float Value;
            public required int Month;
            public required int Year;
            public required bool isHitter;
        }

        internal class PlayerMonthWar {
            public required int MLbId;
            public required int ModelId;
            public required bool isHitter;
            public required string position;
            public required float War;
            public required float Value;
            public required int ParentOrgId;
            public required int HighestLevel;
        }

        internal class PlayerYearPosition {
            public required int Year;
            public required string position;
            public required bool isHitter;
        }

        public static bool Main(int endYear, int endMonth)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
                siteDb.PlayerRank.RemoveRange(siteDb.PlayerRank);
                siteDb.Models.RemoveRange(siteDb.Models);
                siteDb.SaveChanges();
                siteDb.ChangeTracker.Clear();

                // Move models from data DB to site DB
                foreach (var model in db.ModelIdx)
                {
                    siteDb.Models.Add(new Models
                    {
                        ModelId = model.Id,
                        Name = model.ModelName
                    });
                }
                siteDb.SaveChanges();

                // Create Position List
                Dictionary<int, List<PlayerYearPosition>> playerYearPositions = new();
                using (ProgressBar progressBar = new ProgressBar(db.Model_Players.Count(), "Creating playerYearPositions"))
                {
                    foreach (var mp in db.Model_Players)
                    {
                        List<PlayerYearPosition> pyps = new();

                        IEnumerable<int> hitterYears = mp.IsHitter == 1 ? db.Player_Hitter_YearAdvanced.Where(f => f.MlbId == mp.MlbId).Select(f => f.Year).OrderBy(f => f) : [];
                        IEnumerable<int> pitcherYears = mp.IsPitcher == 1 ? db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == mp.MlbId).Select(f => f.Year).OrderBy(f => f) : [];

                        if (mp.IsPitcher == 1)
                        {
                            foreach (var y in pitcherYears)
                            {
                                pyps.Add(new PlayerYearPosition
                                {
                                    Year = y,
                                    position = "P",
                                    isHitter = false,
                                });
                            }
                        }
                        if (mp.IsHitter == 1)
                        {
                            foreach (var y in hitterYears)
                            {
                                pyps.Add(new PlayerYearPosition
                                {
                                    Year = y,
                                    position = Utilities.GetPosition(db, mp.MlbId, y),
                                    isHitter = true,
                                });
                            }
                        }

                        playerYearPositions.Add(mp.MlbId, pyps);

                        progressBar.Tick();
                    }
                }

                // Iterate through months
                int year = 2015;
                int month = 4;

                List<(int, int)> dates = new();
                while (year < endYear || (year == endYear && month <= endMonth))
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
                using var fileStreamDates = new FileStream(Constants.SITE_ASSET_FOLDER + $"dates.json.gz", FileMode.Create);
                using var gzipStreamDates = new GZipStream(fileStreamDates, CompressionLevel.Optimal);
                using var writerDates = new Utf8JsonWriter(gzipStreamDates, new JsonWriterOptions { Indented = false });
                JsonSerializer.Serialize(writerDates, datesJson);

                using (ProgressBar progressBar = new ProgressBar(db.ModelIdx.Count(), "Generating Rankings for Models"))
                {
                    foreach (var model in db.ModelIdx)
                    {
                        // Get intial value for every player
                        var initial_pwa = db.Output_PlayerWarAggregation.Where(f => f.Year == 0 && f.Model == model.Id)
                            .Join(db.Player, opwa => opwa.MlbId, p => p.MlbId, (opwa, p) => new PlayerWar
                            {
                                MlbId = opwa.MlbId,
                                ModelId = opwa.Model,
                                War = opwa.War,
                                Value=opwa.Value,
                                Month = p.SigningMonth.Value,
                                Year = p.SigningYear.Value,
                                isHitter = opwa.IsHitter == 1,
                            });

                        // Get ordered list of values for players
                        List<List<PlayerWar>> playersWarList = new(initial_pwa.Count());
                        using (ChildProgressBar topChild = progressBar.Spawn(initial_pwa.Count(), "Creating PlayersWarList"))
                        {
                            foreach (var pwa in initial_pwa)
                            {
                                List<PlayerWar> pw = [];
                                var opwas = db.Output_PlayerWarAggregation.Where(f => f.MlbId == pwa.MlbId && f.Year > 0 && f.Model == pwa.ModelId && f.IsHitter == (pwa.isHitter ? 1 : 0))
                                    .OrderBy(f => f.Year).ThenBy(f => f.Month)
                                    .Select(f => new PlayerWar
                                    {
                                        MlbId = f.MlbId,
                                        ModelId = pwa.ModelId,
                                        War = f.War,
                                        Value = f.Value,
                                        Month = f.Month,
                                        Year = f.Year,
                                        isHitter = pwa.isHitter
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
                                    var last = pw.Where(f => f.isHitter == pwa.isHitter).Last();
                                    // Check if player exhausted prospect status
                                    if (mp.LastProspectYear <= endYear)
                                    {
                                        pw.Add(new PlayerWar
                                        {
                                            MlbId = last.MlbId,
                                            ModelId = last.ModelId,
                                            War = last.War,
                                            Value = last.Value,
                                            Month = mp.LastProspectMonth,
                                            Year = mp.LastProspectYear,
                                            isHitter = pwa.isHitter,
                                        });
                                    }
                                    else // Player is still a prospect
                                    {
                                        if (last.Month != endMonth || last.Year != endYear)
                                        {
                                            pw.Add(new PlayerWar
                                            {
                                                MlbId = last.MlbId,
                                                ModelId = last.ModelId,
                                                War = last.War,
                                                Value = last.Value,
                                                Month = endMonth,
                                                Year = endYear,
                                                isHitter = pwa.isHitter,
                                            });
                                        }
                                    }

                                }

                                playersWarList.Add(pw);
                                topChild.Tick();
                            }
                        }

                        // Go through all month combos, looking at all players
                        using (ChildProgressBar topChild = progressBar.Spawn(dates.Count(), "Generating Rankings"))
                        {
                            foreach (var date in dates)
                            {
                                month = date.Item1;
                                year = date.Item2;

                                List<PlayerMonthWar> pmwList = new();
                                using (ChildProgressBar child = topChild.Spawn(playersWarList.Count, $"Getting Players for {month}-{year}"))
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
                                        PlayerWar current = playerWarList.Where(f => (f.isHitter == first.isHitter) && (f.Year < year || (f.Year == year && f.Month <= month)))
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
                                        var pyps = playerYearPositions[current.MlbId].Where(f => f.isHitter == playerWarList.First().isHitter);
                                        if (!pyps.Any()) // No playing time, get stored from Mlb
                                            position = db.Site_PlayerBio.Single(f => f.Id == current.MlbId).Position;
                                        else
                                        {
                                            var prevCurrentPyps = pyps.Where(f => f.Year <= year) // All values <= selected year
                                                .OrderByDescending(f => f.Year);
                                            if (prevCurrentPyps.Any())
                                                position = prevCurrentPyps.First().position; // Get most recent
                                            else
                                                position = pyps.OrderBy(f => f.Year).First().position; // Get next value (slight future bias)
                                        }

                                        // Get highest level player has reached this far
                                        int highestLevel = 20;
                                        if (playerWarList.First().isHitter)
                                        {
                                            var hitterLevels = db.Player_Hitter_MonthStats.Where(f => f.MlbId == current.MlbId && (f.Year < year || (f.Year == year && f.Month <= month))).Select(f => f.LevelId).Distinct().OrderBy(f => f);
                                            if (hitterLevels.Any())
                                                highestLevel = hitterLevels.First();
                                        }
                                        else
                                        {
                                            var pitcherLevels = db.Player_Pitcher_MonthStats.Where(f => f.MlbId == current.MlbId && (f.Year < year || (f.Year == year && f.Month <= month))).Select(f => f.LevelId).Distinct().OrderBy(f => f);
                                            if (pitcherLevels.Any())
                                                highestLevel = pitcherLevels.First();
                                        }


                                        pmwList.Add(new PlayerMonthWar
                                        {
                                            MLbId = current.MlbId,
                                            ModelId = current.ModelId,
                                            isHitter = current.isHitter,
                                            War = current.War,
                                            Value = current.Value,
                                            ParentOrgId = poms.Any() ? poms // Few players only played on VSL teams that were multiple teams (no parent)
                                                .First().ParentOrgId : 0,
                                            position = position,
                                            HighestLevel = highestLevel,
                                        });
                                    }
                                }

                                // Create overall ranking
                                pmwList = pmwList.OrderByDescending(f => f.War).ToList();

                                // Create Rankings
                                List<SiteDb.PlayerRank> ranks = new(pmwList.Count);
                                int rank = 1;
                                foreach (var pmw in pmwList)
                                {
                                    int r = rank;
                                    ranks.Add(new PlayerRank
                                    {
                                        MlbId = pmw.MLbId,
                                        Year = year,
                                        Month = month,
                                        ModelId = pmw.ModelId,
                                        IsHitter = pmw.isHitter ? 1 : 0,
                                        RankWar = r,
                                        War = pmw.War,
                                        Position = pmw.position,
                                        TeamId = pmw.ParentOrgId,
                                        TeamRankWar = -1,
                                        HighestLevel = pmw.HighestLevel,
                                        Value = pmw.Value,
                                        RankValue = -1,
                                        TeamRankValue = -1
                                    });
                                    rank++;
                                }

                                // Modify to include team rankings
                                var teamIds = db.Team_Parents.Select(f => f.Id);
                                foreach (var teamId in teamIds)
                                {
                                    var teamRanks = ranks.Where(f => f.TeamId == teamId)
                                        .OrderBy(f => f.RankWar);

                                    int teamRank = 1;
                                    foreach (var tr in teamRanks)
                                    {
                                        int r = teamRank;
                                        tr.TeamRankWar = r;
                                        teamRank++;
                                    }
                                }

                                // Rank by value
                                ranks = ranks.OrderByDescending(f => f.Value).ToList();
                                rank = 1;
                                foreach (var rnk in ranks)
                                {
                                    int r = rank;
                                    rnk.RankValue = r;
                                    rank++;
                                }

                                // Rank by team values
                                foreach (var teamId in teamIds)
                                {
                                    var teamRanks = ranks.Where(f => f.TeamId == teamId)
                                        .OrderBy(f => f.RankValue);
                                    int teamRank = 1;
                                    foreach (var tr in teamRanks)
                                    {
                                        int r = teamRank;
                                        tr.TeamRankValue = r;
                                        teamRank++;
                                    }
                                }

                                siteDb.AddRange(ranks);
                                siteDb.SaveChanges();
                                siteDb.ChangeTracker.Clear();

                                topChild.Tick();
                            }
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
