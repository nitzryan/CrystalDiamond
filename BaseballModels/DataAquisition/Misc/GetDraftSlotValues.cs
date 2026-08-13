using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Text;

namespace DataAquisition.Misc
{
    internal class GetDraftSlotValues
    {
        private static IWebDriver _driver;

        public static void Update(List<int> years)
        {
            // Do not do pre-2012, different draft system
            years = years.Where(f => f >= 2012).ToList();

            // Get Driver
            var options = new ChromeOptions
            {
                BinaryLocation = @"C:\chrome-win64_150\chrome.exe"
            };
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--remote-allow-origins=*");
            options.AddArgument("--disable-features=IsolateOrigins,site-per-process");
            options.AddArgument("--user-data-dir=C:\\SeleniumChromeProfile");
            _driver = new ChromeDriver("chromedriver.exe", options);
            try
            {
                foreach (int year in years)
                {
                    // Get values for year
                    string url = $"https://www.spotrac.com/mlb/draft/_/year/{year}/sort/pick";
                    _driver.Navigate().GoToUrl(url);

                    // Wait for tables to load
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                    wait.Until(d => d.FindElements(By.CssSelector("table.draft-results-table")).Count > 0);

                    // Tables may load at different times; wait extra to be safe
                    System.Threading.Thread.Sleep(3000);

                    var results = new List<(int Year, int Pick, long Slot)>();
                    
                    var tables = _driver.FindElements(By.CssSelector("table.draft-results-table"));
                    foreach (var table in tables)
                    {
                        // Only rows in tbody, skipping header rows
                        var rows = table.FindElements(By.CssSelector("tbody tr"));
                        foreach (var row in rows)
                        {
                            // Skip any hidden rows
                            if (!row.Displayed)
                                continue;

                            var cells = row.FindElements(By.TagName("td"));

                            if (cells.Count < 3)
                            {
                                Console.WriteLine($"Skipping row ({year}): only {cells.Count} column(s)");
                                continue;
                            }

                            // Forfeited picks are expected; skip
                            if (cells[2].Text.Trim().Equals("Forfeit", StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (cells.Count < 9)
                            {
                                Console.WriteLine($"Skipping row ({year}): col1='{cells[1].Text.Trim()}, 'col2='{cells[2].Text.Trim()}', only {cells.Count} columns");
                                continue;
                            }

                            // Pick number is in a span; cell may also contain a div with extra pick
                            // info (e.g., "CB-B") that we don't want
                            string pickText;
                            var pickSpans = cells[0].FindElements(By.TagName("span"));
                            if (pickSpans.Count > 0)
                            {
                                pickText = pickSpans[0].Text.Trim();
                            }
                            else
                            {
                                Console.WriteLine($"Skipping row ({year}): col2='{cells[1].Text.Trim()}', pick {cells[0].Text.Trim()} not in a span");
                                continue;
                            }
                            string slotText = cells[8].Text.Trim()
                                .Replace("$", "")
                                .Replace(",", "");

                            if (!int.TryParse(pickText, out int pick))
                            {
                                Console.WriteLine($"Skipping row ({year}): col2='{cells[1].Text.Trim()}', unparseable pick '{pickText}'");
                                continue;
                            }

                            if (!long.TryParse(slotText, NumberStyles.Any, CultureInfo.InvariantCulture, out long slot))
                            {
                                Console.WriteLine($"Skipping row ({year}): col2='{cells[1].Text.Trim()}', unparseable slot '{cells[8].Text.Trim()}'");
                                continue;
                            }

                            results.Add((year, pick, slot));
                        }
                    }
                    // Log into "../../../OutputFiles/DraftSlots/slots<year>.csv"
                    string outputPath = Path.Combine("..", "..", "..", "OutputFiles", "DraftSlots", $"slots{year}.csv");
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    
                    using (var writer = new StreamWriter(outputPath, false))
                    {
                        foreach (var (y, pick, slot) in results)
                        {
                            writer.WriteLine($"{y},{pick},{slot}");
                        }
                    }
                    Console.WriteLine($"Wrote {results.Count} rows for {year} to {outputPath}");
                }
            }
            finally
            {
                _driver.Quit();
                _driver.Dispose();
            }
        }
    }
}
