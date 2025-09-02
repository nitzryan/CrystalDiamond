using Db;
using ShellProgressBar;
using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace SitePrep
{
    internal class PitcherPage
    {
        private static bool WritePlayerJson()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var players = db.Model_Players.Where(f => f.IsPitcher == 1)
                .Join(db.Site_PlayerBio, mp => mp.MlbId, sbi => sbi.Id, (mp, sbi) => new { mp, sbi }); ;
            using (ProgressBar progressBar = new ProgressBar(players.Count(), "Generating Pitcher Pages"))
            {
                foreach (var playerTuple in players)
                {
                    var player = playerTuple.mp;
                    var bio = playerTuple.sbi;

                    JsonObject json = new();

                    // Model Output
                    var opws = db.Output_PlayerWarAggregation.Where(f => f.MlbId == player.MlbId && f.ModelName.Equals("P")).OrderBy(f => f.Year).ThenBy(f => f.Month);
                    JsonArray modelOutput = new();
                    foreach (var opw in opws)
                    {
                        JsonObject obj = new();
                        obj["year"] = opw.Year;
                        obj["month"] = opw.Month;

                        JsonArray probs = [opw.Prob0, opw.Prob1, opw.Prob2, opw.Prob3, opw.Prob4, opw.Prob5, opw.Prob6];
                        obj.Add("probs", probs);

                        var ranks = db.Ranking_Prospect.Where(f => f.Year == opw.Year && f.Month == opw.Month && f.MlbId == opw.MlbId && f.Model.Equals(opw.ModelName));
                        if (!ranks.Any() && opw.Month == 0)
                            ranks = db.Ranking_Prospect.Where(f => f.MlbId == opw.MlbId && f.Model.Equals(opw.ModelName))
                                .OrderBy(f => f.Year).ThenBy(f => f.Month);
                        
                        if (ranks.Any())        
                            obj["rank"] = ranks.First().Rank;

                        modelOutput.Add(obj);
                    }
                    json.Add("model", modelOutput);

                    // Demographic Data
                    Db.Player p = db.Player.Where(f => f.MlbId == player.MlbId).Single();
                    json["birthYear"] = p.BirthYear;
                    json["birthMonth"] = p.BirthMonth;
                    json["birthDate"] = p.BirthDate;
                    json["startYear"] = p.SigningYear;

                    if (bio.DraftPick != -1)
                    {
                        JsonObject draftObj = new()
                        {
                            ["pick"] = bio.DraftPick,
                            ["round"] = bio.DraftRound,
                            ["bonus"] = bio.DraftBonus
                        };
                        json.Add("draft", draftObj);
                    }

                    json["position"] = bio.Position;
                    json["status"] = bio.Status;
                    json["firstName"] = p.UseFirstName;
                    json["lastName"] = p.UseLastName;

                    // Get most recent org
                    var poms = db.Player_OrgMap.Where(f => f.MlbId == player.MlbId).OrderByDescending(f => f.Year).ThenByDescending(f => f.Month).ThenByDescending(f => f.Day);
                    if (poms.Any())
                        json["orgId"] = poms.First().ParentOrgId;

                    // Annual Stats
                    var annualStats = db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == player.MlbId).OrderBy(f => f.Year).ThenByDescending(f => f.LevelId).ThenBy(f => f.TeamId);
                    JsonArray statsArray = new();
                    foreach (var stats in annualStats)
                    {
                        float hrRate = stats.Outs > 0 ? (float)(stats.HR) / stats.Outs * 27 : stats.HR * 27;
                        JsonObject obj = new();
                        obj["level"] = stats.LevelId;
                        obj["year"] = stats.Year;
                        obj["team"] = stats.TeamId;
                        obj["league"] = stats.LeagueId;
                        obj["IP"] = $"{stats.Outs / 3}.{stats.Outs % 3}";
                        obj["ERA"] = Math.Round(stats.ERA, 2);
                        obj["FIP"] = Math.Round(stats.FIP, 2);
                        obj["HR9"] = Math.Round(hrRate, 1);
                        obj["BB%"] = Math.Round(stats.BBPerc * 100, 1);
                        obj["K%"] = Math.Round(stats.KPerc * 100, 1);
                        obj["GO%"] = stats.GBRatio;

                        statsArray.Add(obj);
                    }
                    json.Add("stats", statsArray);

                    using var fileStream = new FileStream(Constants.SITE_ASSET_FOLDER + $"player/p{player.MlbId}.json.gz", FileMode.Create);
                    using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
                    using var writer = new Utf8JsonWriter(gzipStream, new JsonWriterOptions { Indented = false });
                    JsonSerializer.Serialize(writer, json);

                    progressBar.Tick();
                }
            }

            return true;
        }

        public static bool Main()
        {
            try
            {
                return WritePlayerJson();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in PitcherPage");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
