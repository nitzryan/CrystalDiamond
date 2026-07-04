using Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ShellProgressBar;

namespace DataAquisition.College
{
    internal class GetSingleClassData
    {
        private static IWebDriver? _driver;
        private const int HITTER_BATCH = 5;
        private const int PITCHER_BATCH = 5;

        private const string HITTER_URL = "https://www.thebaseballcube.com/content/research/college_batting.asp";
        private const string PITCHER_URL = "https://www.thebaseballcube.com/content/research/college_pitching.asp";

        public static bool GetData(int year)
        {
            try
            {
                if (!LetUserSignIn())
                    throw new Exception("User failed to sign in");


                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.College_HitterStats.Where(f => f.Year == year).ExecuteDelete();
                db.College_PitcherStats.Where(f => f.Year == year).ExecuteDelete();

                List<TeamData> teams = GetAllTeamData();

                var priorExpYears = db.College_HitterStats
                    .Select(h => new { h.TBCId, h.Year, h.ExpYears })
                    .AsEnumerable()
                    .Concat(db.College_PitcherStats
                        .Select(p => new { p.TBCId, p.Year, p.ExpYears })
                        .AsEnumerable())
                    .GroupBy(r => r.TBCId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Max(r => r.ExpYears + (year - r.Year)));

                return GetHitterData(year, teams, db, priorExpYears)
                    && GetPitcherData(year, teams, db, priorExpYears);
            }
            finally
            {
                _driver?.Quit();
                _driver = null;
            }
        }

        private static bool GetHitterData(int year, List<TeamData> teams, SqliteDbContext db, Dictionary<int, int> priorExpYears)
        {
            var confMap = db.College_ConfMap.ToList();
            var players = db.College_Player.ToDictionary(p => p.TBCId);

            List<College_HitterStats> hitterStats = ScrapePlayerData(
                HITTER_URL, HITTER_BATCH, teams, year,
                cells => ParseHitterRow(cells, year, confMap, priorExpYears, db, players),
                "Hitting");

            hitterStats = hitterStats
                .GroupBy(f => f.TBCId)
                .Select(g => g.OrderByDescending(x => x.AB).First())
                .ToList();
            db.College_HitterStats.AddRange(hitterStats);
            db.SaveChanges();

            return true;
        }

        private static bool GetPitcherData(int year, List<TeamData> teams, SqliteDbContext db, Dictionary<int, int> priorExpYears)
        {
            var confMap = db.College_ConfMap.ToList();
            var players = db.College_Player.ToDictionary(p => p.TBCId);

            List<College_PitcherStats> pitcherStats = ScrapePlayerData(
                PITCHER_URL, PITCHER_BATCH, teams, year,
                cells => ParsePitcherRow(cells, year, confMap, priorExpYears, db, players),
                "Pitching");

            pitcherStats = pitcherStats
                .GroupBy(f => f.TBCId)
                .Select(g => g.OrderByDescending(x => x.Outs).First())
                .ToList();
            db.College_PitcherStats.AddRange(pitcherStats);
            db.SaveChanges();

            return true;
        }

        private static bool LetUserSignIn()
        {
            _driver = new ChromeDriver();
            _driver.Navigate().GoToUrl("https://www.thebaseballcube.com/");

            Console.WriteLine("Sign in using the browser window that just opened.");
            Console.WriteLine("When finished, type 'y' and press Enter to confirm. Any other input reports failure.");
            
            string? input = Console.ReadLine();
            return string.Equals(input?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
        }

        private record TeamData(string Name, int TeamId, string ConferenceCode);
        private static List<TeamData> GetAllTeamData()
        {
            _driver!.Navigate().GoToUrl("https://www.thebaseballcube.com/content/schools/ncaa-1/");

            List <TeamData> teams = [];
            foreach (IWebElement row in _driver.FindElements(By.CssSelector("#grid1 tr.data-row")))
            {
                var cells = row.FindElements(By.TagName("td"));
                string name = cells[0].Text.Trim();

                int teamId = int.Parse(GetLastHrefSegment(cells[0].FindElement(By.TagName("a"))));
                string conferenceCode = GetLastHrefSegment(cells[2].FindElement(By.TagName("a")));

                teams.Add(new TeamData(name, teamId, conferenceCode));
            }
            return teams;
        }

        private static string GetLastHrefSegment(IWebElement anchor)
        {
            string href = anchor.GetAttribute("href")
                ?? throw new Exception("Anchor element has no href attribute");
            return href.TrimEnd('/').Split('/')[^1];
        }
        private static int GetTeamIdFromCell(IWebElement cell)
        {
            string segment = GetLastHrefSegment(cell.FindElement(By.TagName("a")));
            return int.Parse(segment.Split('~')[^1]);
        }
        private static float ParseRateStat(IWebElement cell)
        {
            string text = cell.Text.Trim();
            return string.IsNullOrEmpty(text) ? 0f : float.Parse(text);
        }

        private static void EnsureChecked(IWebElement checkbox)
        {
            if (!checkbox.Selected)
                checkbox.Click();
        }

        private static List<T> ScrapePlayerData<T>(
            string url,
            int batchSize,
            List<TeamData> teams,
            int year,
            Func<IReadOnlyList<IWebElement>, T?> parseRow,
            string statName) where T : class
        {
            List<T> results = [];

            int totalBatches = teams.Chunk(batchSize).Count();
            using (var progressBar = new ProgressBar(totalBatches, $"Processing College {statName} for {year}"))
            { 
                foreach (TeamData[] batch in teams.Chunk(batchSize))
                {
                    _driver!.Navigate().GoToUrl(url);

                    EnsureChecked(_driver.FindElement(By.Id("includebio")));

                    IWebElement input = _driver.FindElement(By.Id("colleges"));
                    input.Clear();
                    input.SendKeys(string.Join(",", batch.Select(t => t.Name)));

                    // Submit form
                    SetYearSelect("y1", year);
                    SetYearSelect("y2", year);
                    _driver.FindElement(By.CssSelector("#research-button input")).Click();

                    IWebElement totalsRow = _driver.FindElement(By.CssSelector("#grid1 tr.totals-row"));
                    int recordCount = int.Parse(totalsRow.FindElement(By.TagName("td")).Text.Trim().Split(' ')[0]);
                    if (recordCount >= 200)
                        throw new Exception($"Result cap hit: {recordCount} records for batch. Lower the batch size.");

                    HashSet<int> batchTeamIds = batch.Select(t => t.TeamId).ToHashSet();
                    foreach (IWebElement row in _driver.FindElements(By.CssSelector("#grid1 tr.data-row")))
                    {
                        var cells = row.FindElements(By.TagName("td"));
                        int rowTeamId = GetTeamIdFromCell(cells[5]);
                        if (!batchTeamIds.Contains(rowTeamId))
                            continue;

                        T? result = parseRow(cells);
                        if (result is not null)
                            results.Add(result);
                    }
                    progressBar.Tick();
                }
            }

            return results;
        }

        private static void SetYearSelect(string selectId, int year)
        {
            SelectElement select = new(_driver!.FindElement(By.Id(selectId)));
            try
            {
                select.SelectByText(year.ToString());
            }
            catch (NoSuchElementException)
            {
                throw new Exception($"Year {year} is not a valid option for select '{selectId}'");
            }
        }

        // Parsing Functions
        private static College_HitterStats? ParseHitterRow(
            IReadOnlyList<IWebElement> cells,
            int year,
            List<College_ConfMap> confMap,
            Dictionary<int, int> priorExpYears,
            SqliteDbContext db,
            Dictionary<int, College_Player> players)
        {
            string bornText = cells[44].Text.Trim();
            if (string.IsNullOrWhiteSpace(bornText))
                return null;

            string[] bornParts = bornText.Split('-');
            int birthYear = int.Parse(bornParts[0]);
            int birthMonth = int.Parse(bornParts[1]);
            int birthDay = int.Parse(bornParts[2]);

            int tbcId = int.Parse(GetLastHrefSegment(cells[1].FindElement(By.TagName("a"))));
            int teamId = GetTeamIdFromCell(cells[5]);
            string confAbvr = cells[6].Text.Trim();

            int expYears = priorExpYears.TryGetValue(tbcId, out int prior) ? prior : 1;

            UpsertPlayer(db, players,
                tbcId, cells[1].Text.Trim(),
                birthYear, birthMonth, birthDay,
                bats: cells[3].Text.Trim(), throws: cells[43].Text.Trim(),
                isHitter: true, year, expYears);

            return new College_HitterStats
            {
                TBCId = tbcId,
                Year = year,
                Level = ColUtilities.LevelStringToInt(cells[39].Text.Trim()),
                TeamId = teamId,
                ConfId = confMap.Single(f => f.Name.Equals(confAbvr)).ConfId,
                ExpYears = expYears,
                AB = int.Parse(cells[8].Text.Trim()),
                PA = int.Parse(cells[25].Text.Trim()),
                H = int.Parse(cells[10].Text.Trim()),
                H2B = int.Parse(cells[11].Text.Trim()),
                H3B = int.Parse(cells[12].Text.Trim()),
                HR = int.Parse(cells[13].Text.Trim()),
                SB = int.Parse(cells[15].Text.Trim()),
                CS = int.Parse(cells[16].Text.Trim()),
                BB = int.Parse(cells[17].Text.Trim()),
                IBB = int.Parse(cells[22].Text.Trim()),
                K = int.Parse(cells[18].Text.Trim()),
                HBP = int.Parse(cells[19].Text.Trim()),
                AVG = ParseRateStat(cells[28]),
                OBP = ParseRateStat(cells[29]),
                SLG = ParseRateStat(cells[30]),
                OPS = ParseRateStat(cells[31]),
                Age = ColUtilities.GetSeasonAge(year, birthDay, birthMonth, birthYear, expYears),
                Pos = ColUtilities.ParsePosString(cells[2].Text.Trim()),
                Height = ColUtilities.GetHeightInInches(cells[40].Text.Trim()),
                Weight = int.Parse(cells[41].Text.Trim()),
            };
        }

        private static College_PitcherStats? ParsePitcherRow(
            IReadOnlyList<IWebElement> cells,
            int year,
            List<College_ConfMap> confMap,
            Dictionary<int, int> priorExpYears,
            SqliteDbContext db,
            Dictionary<int, College_Player> players)
        {
            string bornText = cells[38].Text.Trim();
            if (string.IsNullOrWhiteSpace(bornText))
                return null;

            string[] bornParts = bornText.Split('-');
            int birthYear = int.Parse(bornParts[0]);
            int birthMonth = int.Parse(bornParts[1]);
            int birthDay = int.Parse(bornParts[2]);

            int tbcId = int.Parse(GetLastHrefSegment(cells[1].FindElement(By.TagName("a"))));
            int teamId = GetTeamIdFromCell(cells[5]);
            string confAbvr = cells[6].Text.Trim();

            int expYears = priorExpYears.TryGetValue(tbcId, out int prior) ? prior : 1;

            UpsertPlayer(db, players,
                tbcId, cells[1].Text.Trim(),
                birthYear, birthMonth, birthDay,
                bats: cells[36].Text.Trim(), throws: cells[3].Text.Trim(),
                isHitter: false, year, expYears);

            return new College_PitcherStats
            {
                TBCId = tbcId,
                Year = year,
                Level = ColUtilities.LevelStringToInt(cells[40].Text.Trim()),
                TeamId = teamId,
                ConfId = confMap.Single(f => f.Name.Equals(confAbvr)).ConfId,
                ExpYears = expYears,
                G = int.Parse(cells[10].Text.Trim()),
                GS = int.Parse(cells[11].Text.Trim()),
                R = int.Parse(cells[19].Text.Trim()),
                ER = int.Parse(cells[20].Text.Trim()),
                Outs = ColUtilities.IPStringToOuts(cells[17].Text.Trim()),
                H = int.Parse(cells[18].Text.Trim()),
                HR = int.Parse(cells[21].Text.Trim()),
                BB = int.Parse(cells[22].Text.Trim()),
                K = int.Parse(cells[23].Text.Trim()),
                HBP = int.Parse(cells[26].Text.Trim()),
                ERA = ParseRateStat(cells[9]),
                H9 = ParseRateStat(cells[28]),
                HR9 = ParseRateStat(cells[29]),
                BB9 = ParseRateStat(cells[30]),
                K9 = ParseRateStat(cells[31]),
                WHIP = ParseRateStat(cells[27]),
                Age = ColUtilities.GetSeasonAge(year, birthDay, birthMonth, birthYear, expYears),
                Height = ColUtilities.GetHeightInInches(cells[34].Text.Trim()),
                Weight = int.Parse(cells[35].Text.Trim()),
            };
        }

        // Update player if exists, insert if doesn't
        private static void UpsertPlayer(
            SqliteDbContext db,
            Dictionary<int, College_Player> players,
            int tbcId,
            string name,
            int birthYear, int birthMonth, int birthDay,
            string bats, string throws,
            bool isHitter,
            int year,
            int expYears)
        {
            if (players.TryGetValue(tbcId, out College_Player? existing))
            {
                if (isHitter)
                    existing.IsHitter = true;
                else
                    existing.IsPitcher = true;
                if (year > existing.LastYear)
                    existing.LastYear = year;
                return;
            }

            int split = name.IndexOf(' ');
            College_Player player = new()
            {
                TBCId = tbcId,
                MlbId = 0,
                FirstName = split < 0 ? name : name[..split],
                LastName = split < 0 ? "" : name[(split + 1)..],
                BirthYear = birthYear,
                BirthMonth = birthMonth,
                BirthDay = birthDay,
                DraftOvrHitter = 0,
                DraftOvrPitcher = 0,
                FirstYear = year,
                LastYear = year,
                Bats = bats,
                Throws = throws,
                IsPitcher = !isHitter,
                IsHitter = isHitter,
                IsEligible = Utilities.IsCollegePlayerElibible(db, 0, year),
            };
            db.College_Player.Add(player);
            players[tbcId] = player;
        }
    }
}
