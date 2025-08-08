using Db;
using ShellProgressBar;
using System.Security.Claims;
using System.Text.Json;

namespace DataAquisition
{
    internal class TransactionLog
    {
        private const int NUM_THREADS = 16;
        private static int progress_bar_thread = 0;
        private static List<int> thread_counts;

        private static async Task<(IEnumerable<Transaction_Log>, IEnumerable<string>)> GetTransaction_Logs(IEnumerable<int> ids, int thread_idx, ProgressBar progressBar, int progressSum)
        {
            List<string> errors = new();
            List<Transaction_Log> logs = new(capacity: ids.Count());
            HttpClient httpClient = new();
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            IProgress<float> progress = progressBar.AsProgress<float>();

            foreach (var id in ids)
            {
                try
                {
                    // Get transactions for players
                    HttpResponseMessage response = await httpClient.GetAsync($"https://statsapi.mlb.com/api/v1/transactions?playerId={id}");
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"HTTP Error for {id}: {response.StatusCode}");
                    }
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JsonDocument json = JsonDocument.Parse(responseBody);
                    var transactions = json.RootElement.GetProperty("transactions").EnumerateArray();
                    foreach (var t in transactions)
                    {
                        // Determine what the transaction is
                        string code = t.GetProperty("typeCode").GetString() ?? throw new Exception($"Failed to get code for {id}");

                        // Get Date
                        string birthdateFormatted = t.GetProperty("date").GetString() ?? throw new Exception("No birthDate found");
                        int[] birthdate = Array.ConvertAll(birthdateFormatted.Split("-"), Convert.ToInt32);

                        int teamId = -1;
                        int toIL = 0; // 1 to IL, -1 Activated From IL
                        if (code == "DES" || code == "DFA" || code == "RET" || code == "REL" || code == "LON" || code == "RES" || code == "WA" || code == "DEC")
                            teamId = 0;
                        else if (code == "SC" || code == "OUT" || code == "SE" || code == "ASG" || code == "TR" || code == "SFA" || code == "SGN" || code == "CLW" || code == "PUR" || code == "OPT" || code == "RE" || code == "TRN" || code == "CU" || code == "RTN" || code == "SU" || code == "CP" || code == "DR" || code == "DEI" || code == "R5M" || code == "R5")
                            teamId = t.GetProperty("toTeam").GetProperty("id").GetInt32();
                        else if (code == "NUM") // Number Change
                            continue;
                        else
                            throw new Exception($"Unexpected code for {id}: {code}");

                        if (code == "SU")
                            toIL = Constants.TL_SUSP;

                        if (code == "SC")
                        {
                            if (t.TryGetProperty("description", out var el))
                            {
                                string description = el.ToString();
                                if (description.Contains("injured") || description.Contains("disabled"))
                                {
                                    if (description.Contains("placed"))
                                    {
                                        if (description.Contains("60"))
                                            toIL = Constants.TL_INJ_ADD_LONG;
                                        else
                                            toIL = Constants.TL_INJ_ADD_SHORT;
                                    }
                                    else if (description.Contains("activated"))
                                        toIL = Constants.TL_INJ_REM;
                                    else if (description.Contains("transferred"))
                                        toIL = Constants.TL_INJ_ADD_LONG;
                                    else
                                        errors.Add($"Invalid injured description for {id}: {description}");
                                }
                            }
                        }

                        int parentOrgId = teamId == 0 ? teamId : Utilities.GetParentOrgId(teamId, birthdate[0], db);
                        if (parentOrgId == -2) // Not moved to a valid team (often for pre-draft or AFL transactions
                            continue;

                        logs.Add(new Transaction_Log
                        {
                            MlbId = id,
                            Year = birthdate[0],
                            Month = birthdate[1],
                            Day = birthdate[2],
                            ToIL = toIL,
                            ParentOrgId = parentOrgId
                        });
                    }
                }
                catch (Exception e)
                {
                    errors.Add($"Id={id}: " + e.Message + " " + e.InnerException?.Message);
                }

                thread_counts[thread_idx]++; // Allows for tracking progress

                if (thread_idx == progress_bar_thread)
                {
                    int count = thread_counts.Sum();
                    progress.Report(Convert.ToSingle(count) / progressSum);
                }
            }

            return (logs, errors);
        }

        public static async Task<bool> Main()
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Transaction_Log.RemoveRange(db.Transaction_Log);
                db.SaveChanges();

                HttpClient httpClient = new();
                StreamWriter file = File.CreateText(Constants.DATA_AQ_DIRECTORY + $"Logs/PlayerOrgMap.txt");

                IEnumerable<int> ids = db.Player_CareerStatus.Select(f => f.MlbId);
                // Split ids into groups for tasks
                int j = 0;
                IEnumerable<IEnumerable<int>> id_partitions = from item in ids
                                                              group item by j++ % NUM_THREADS into part
                                                              select part.AsEnumerable();

                List<Task<(IEnumerable<Transaction_Log>, IEnumerable<string>)>> tasks = new(NUM_THREADS);
                using (ProgressBar progressBar = new ProgressBar(ids.Count(), $"Generating Player Org Map"))
                {
                    progress_bar_thread = 0;
                    thread_counts = [.. Enumerable.Repeat(0, NUM_THREADS)];

                    for (int i = 0; i < NUM_THREADS; i++)
                    {
                        int idx = i;
                        Task<(IEnumerable<Transaction_Log>, IEnumerable<string>)> task = GetTransaction_Logs(id_partitions.ElementAt(i), idx, progressBar, ids.Count());
                        tasks.Add(task);
                    }

                    foreach (var task in tasks)
                    {
                        var logs = await task;
                        db.Transaction_Log.AddRange(logs.Item1);
                        foreach (var line in logs.Item2)
                            file.WriteLine(line);
                        progress_bar_thread++; // If thread N completes, move updating progress bar to thread N+1
                    }
                }
                db.SaveChanges();
                file.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in PlayerOrgMap");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
