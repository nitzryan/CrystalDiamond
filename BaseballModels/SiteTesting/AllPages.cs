using Db;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SiteDb;

[assembly: LevelOfParallelism(Config.levels)]
namespace SiteTesting
{
    
    [Parallelizable(ParallelScope.All)]
    [TestFixture]
    public class Tests
    {
        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                    .UseSqlite("Data Source=../../../../Db/BaseballStats.db;")
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .Options;

        public static readonly DbContextOptions<SiteDbContext> SITEDB_OPTIONS = new DbContextOptionsBuilder<SiteDbContext>()
        .UseSqlite("Data Source=../../../../SiteDb/Site.db;")
        .Options;

        private static readonly Dictionary<string, ChromeDriver> driverDict = new();
        private static readonly object LockObject = new();
        private static SqliteDbContext db = new(DB_OPTIONS);
        private static SiteDbContext siteDb = new (SITEDB_OPTIONS);

        public const string DEFAULT_DRIVER_STR = "default";

        [OneTimeSetUp]
        public void Setup()
        {
            lock(LockObject)
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--headless");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--disable-extensions");
                for (var i = 0; i < Config.levels; i++)
                {
                    var workerIdString = $"ParallelWorker#{i + 1}";
                    if (!driverDict.ContainsKey(workerIdString))
                    {
                        driverDict[workerIdString] = new ChromeDriver(chromeOptions);
                    }
                }

                if (!driverDict.ContainsKey(DEFAULT_DRIVER_STR))
                    driverDict[DEFAULT_DRIVER_STR] = new ChromeDriver(chromeOptions);
            }
        }

        // Player

        [Test, TestCaseSource(nameof(AllPlayerIds))]
        public void LoadPlayersNoErrors(int id)
        {
            string url = $"http://127.0.0.1:3000/player?id={id}";
            var driver = driverDict[TestContext.CurrentContext.WorkerId];
            bool pageLoaded = SeleniumUtilities.WaitForPageLoad(driver, url);
            Assert.That(pageLoaded, $"{id} not loaded");
            var (errors, msg) = SeleniumUtilities.AnyErrors(driver);
            Assert.That(!errors, $"{id} error: {msg}");
        }

        static int[] AllPlayerIds()
        {
            return [.. siteDb.Player.Select(f => f.MlbId)];
        }

        // Ranking

        [Test]
        public void DefaultRanking()
        {
            var driver = driverDict[DEFAULT_DRIVER_STR];
            bool pageLoaded = SeleniumUtilities.WaitForPageLoad(driver, "http://127.0.0.1:3000/rankings");
            Assert.That(pageLoaded, "Player not loaded");
            var (errors, msg) = SeleniumUtilities.AnyErrors(driver);
            Assert.That(!errors, $"Ranking error: {msg}");
        }

        [Test, TestCaseSource(nameof(AllRanks))]
        public void LoadRanksNoErrors(int year, int month)
        {
            string url = $"http://127.0.0.1:3000/rankings?year={year}&month={month}";
            var driver = driverDict[TestContext.CurrentContext.WorkerId];
            bool pageLoaded = SeleniumUtilities.WaitForPageLoad(driver, url);
            Assert.That(pageLoaded, $"{year}-{month} not loaded");
            var (errors, msg) = SeleniumUtilities.AnyErrors(driver);
            Assert.That(!errors, $"{year}-{month} error: {msg}");
        }

        static IEnumerable<object> AllRanks()
        {
            var dates = siteDb.PlayerRank.Select(f => new { f.Year, f.Month }).Distinct();
            foreach (var d in dates)
                yield return new object[] { d.Year, d.Month };
        }

        // Teams
        [Test, TestCaseSource(nameof(AllTeamRanks))]
        public void LoadTeamRanksNoErrors(int year, int month, int teamId)
        {
            string url = $"http://127.0.0.1:3000/teams?year={year}&month={month}&team={teamId}";
            var driver = driverDict[TestContext.CurrentContext.WorkerId];
            bool pageLoaded = SeleniumUtilities.WaitForPageLoad(driver, url);
            Assert.That(pageLoaded, $"{year}-{month} team {teamId} not loaded");
            var (errors, msg) = SeleniumUtilities.AnyErrors(driver);
            Assert.That(!errors, $"{year}-{month} team {teamId} error: {msg}");
        }

        static IEnumerable<object> AllTeamRanks()
        {
            var dates = siteDb.PlayerRank.Select(f => new { f.Year, f.Month, f.TeamId }).Distinct().Where(f => f.TeamId != 0);
            foreach (var d in dates)
                yield return new object[] { d.Year, d.Month, d.TeamId };
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            lock (LockObject)
            {
                for (var i = 0; i < Config.levels; i++)
                {
                    var workerIdString = $"ParallelWorker#{i + 1}";
                    if (driverDict.ContainsKey(workerIdString))
                    {
                        driverDict[workerIdString]?.Quit();
                        driverDict[workerIdString]?.Dispose();
                        driverDict.Remove(workerIdString);
                    }
                }

                if (driverDict.ContainsKey(DEFAULT_DRIVER_STR))
                {
                    driverDict[DEFAULT_DRIVER_STR]?.Quit();
                    driverDict[DEFAULT_DRIVER_STR]?.Dispose();
                    driverDict.Remove(DEFAULT_DRIVER_STR);
                }
            }
        }
    }
}

public static class Config
{
    public const int levels = 2;
}