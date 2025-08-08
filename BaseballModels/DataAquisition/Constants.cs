using Db;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DataAquisition
{
    internal class Constants
    {
        public const int START_YEAR = 2005;
        public static ReadOnlyCollection<int> SPORT_IDS = new([1, 11, 12, 13, 14, 15, 16, 17]);
        public static ReadOnlyCollection<string> SPORT_ID_NAMES = new(["MLB", "AAA", "AA", "A+", "A", "A-", "Rk", "DSL"]);
        public const int MEXICAN_LEAGUE_ID = 125;
        public const int DSL_LEAGUE_ID = 130;
        public const int VSL_LEAGUE_ID = 134;
        public const string SCRIPT_FOLDER = "../../../../DataAquisition/Scripts";
        public const string IGNORE_PLAYERS_FILE = "../../../../DataAquisition/Scripts/IgnorePlayers.txt";
        public const string SERVICE_TIME_FILE = "../../../../DataAquisition/ServiceTime.csv";
        public const string PRE_05_FILE = "../../../../DataAquisition/Pre05Players.csv";
        public const string DATA_AQ_DIRECTORY = "../../../../DataAquisition/";
        public const int SERVICE_TIME_CUTOFF = 6;
        public const int AGED_OUT_AGE = 27;
        public const int STOP_YEAR = 34;

        // Set these to set start/stop
        public const int CURRENT_YEAR = 2024;
        public const int CURRENT_MONTH = 9;

        // Transaction ToIL Status
        public const int TL_SUSP = 1;
        public const int TL_INJ_ADD_SHORT = 2;
        public const int TL_INJ_ADD_LONG = 3;
        public const int TL_INJ_REM = 4;

        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db")
                .Options;
    }
}
