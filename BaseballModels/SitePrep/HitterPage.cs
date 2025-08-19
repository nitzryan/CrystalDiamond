using Db;
using System.Text.Json.Nodes;
using System.Text.Json;
using ShellProgressBar;
using System.IO.Compression;

namespace SitePrep
{
    internal class HitterPage
    {
        private static bool WritePlayerJson()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var players = db.Model_Players.Where(f => f.IsHitter == 1);
            using (ProgressBar progressBar = new ProgressBar(players.Count(), "Generating Hitter Pages"))
            {
                foreach (var player in players)
                {
                    JsonObject json = new();

                    // Model Output
                    var opws = db.Output_PlayerWar.Where(f => f.MlbId == player.MlbId).OrderBy(f => f.Year).ThenBy(f => f.Month);
                    JsonArray modelOutput = new();
                    foreach (var opw in opws)
                    {
                        JsonObject obj = new();
                        obj["year"] = opw.Year;
                        obj["month"] = opw.Month;
                        obj["p0"] = opw.Prob0;
                        obj["p1"] = opw.Prob1;
                        obj["p2"] = opw.Prob2;
                        obj["p3"] = opw.Prob3;
                        obj["p4"] = opw.Prob4;
                        obj["p5"] = opw.Prob5;
                        obj["p6"] = opw.Prob6;

                        modelOutput.Add(obj);
                    }
                    json.Add("model", modelOutput);

                    // Demographic Data
                    Db.Player p = db.Player.Where(f => f.MlbId == player.MlbId).Single();
                    json["birthYear"] = p.BirthYear;
                    json["birthMonth"] = p.BirthMonth;
                    json["birthDate"] = p.BirthDate;
                    json["startYear"] = p.SigningYear;
                    if (p.DraftPick != null)
                        json["draftPick"] = p.DraftPick.Value;
                    json["firstName"] = p.UseFirstName;
                    json["lastName"] = p.UseLastName;

                    // Get most recent org (TODO: Change how this is tracked)
                    var poms = db.Player_OrgMap.Where(f => f.MlbId == player.MlbId).OrderByDescending(f => f.Year).ThenByDescending(f => f.Month);
                    if (!poms.Any())
                        json["orgId"] = 0;
                    else
                        json["orgId"] = poms.First().ParentOrgId;

                    // Annual Stats
                    var annualStats = db.Player_Hitter_YearAdvanced.Where(f => f.MlbId == player.MlbId).OrderBy(f => f.Year).ThenBy(f => f.LevelId).ThenBy(f => f.TeamId);
                    JsonArray statsArray = new();
                    foreach (var stats in annualStats)
                    {
                        JsonObject obj = new();
                        obj["level"] = stats.LevelId;
                        obj["year"] = stats.Year;
                        obj["team"] = stats.TeamId;
                        obj["league"] = stats.LeagueId;
                        obj["PA"] = stats.PA;
                        obj["AVG"] = stats.AVG;
                        obj["OBP"] = stats.OBP;
                        obj["SLG"] = stats.SLG;
                        obj["ISO"] = stats.ISO;
                        obj["wrc"] = stats.WRC;
                        obj["HR"] = stats.HR;
                        obj["BB%"] = stats.BBPerc;
                        obj["K%"] = stats.KPerc;
                        obj["SB"] = stats.SB;
                        obj["CS"] = stats.CS;

                        statsArray.Add(obj);
                    }
                    json.Add("stats", statsArray);

                    using var fileStream = new FileStream(Constants.SITE_DATA_FOLDER + $"player/h{player.MlbId}.json.gz", FileMode.Create);
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
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
