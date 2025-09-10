using Db;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SiteDb;

namespace SiteTesting
{
    [TestFixture]
    public class Tests
    {
        private IWebDriver driver;
        private SqliteDbContext db;
        private SiteDbContext siteDb;

        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

        public static readonly DbContextOptions<SiteDbContext> SITEDB_OPTIONS = new DbContextOptionsBuilder<SiteDbContext>()
        .UseSqlite("Data Source=../../../../SiteDb/Site.db;")
        .Options;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            db = new(DB_OPTIONS);
            siteDb = new(SITEDB_OPTIONS);
        }

        [Test]
        public void Player()
        {
            bool pageLoaded = SeleniumUtilities.WaitForPageLoad(driver, "http://127.0.0.1:3000/player?id=805805");
            Assert.That(pageLoaded, "Player not loaded");
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}