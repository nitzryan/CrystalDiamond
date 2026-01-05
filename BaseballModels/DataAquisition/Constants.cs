using Db;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DataAquisition
{
    internal class Constants
    {
        public const int START_YEAR = 2005;
        public static ReadOnlyCollection<int> SPORT_IDS = new([1, 11, 12, 13, 14, 15, 16, 17]);
        public static ReadOnlyCollection<int> SPORT_IDS_MLBAPI = new([1, 11, 12, 13, 14, 15, 16, 5442]);
        public static ReadOnlyCollection<string> SPORT_ID_NAMES = new(["MLB", "AAA", "AA", "A+", "A", "A-", "Rk", "DSL"]);
        public static ReadOnlyCollection<string> SPORT_ID_NAMES_MLPAPI = new(["MLB", "AAA", "AA", "A+", "A", "A-", "Rk", "Rk2019"]);
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
        //public const int CURRENT_YEAR = 2024;
        //public const int CURRENT_MONTH = 9;

        // Positional adjustment
        //https://tht.fangraphs.com/re-examining-wars-defensive-spectrum/
        public const float POSITIONAL_ADJUSTMENT_C = 7.75f;
        public const float POSITIONAL_ADJUSTMENT_1B = -9.25f;
        public const float POSITIONAL_ADJUSTMENT_2B = 1.75f;
        public const float POSITIONAL_ADJUSTMENT_3B = 1.75f;
        public const float POSITIONAL_ADJUSTMENT_SS = 4.75f;
        public const float POSITIONAL_ADJUSTMENT_LF = -4.25f;
        public const float POSITIONAL_ADJUSTMENT_CF = 1.75f;
        public const float POSITIONAL_ADJUSTMENT_RF = -4.25f;
        public const float POSITIONAL_ADJUSTMENT_DH = -9.25f;

        // WAR Calculation Constants
        public const float REPLACEMENT_LEVEL_WIN_PERCENTAGE = 1000.0f / 2430;
        public const float HITTER_WAR_PERCENTAGE = 0.57f;
        public const float PITCHER_WAR_PERCENTAGE = 1.0f - HITTER_WAR_PERCENTAGE;

        // VALUATION
        public const float HITTER_WAR_INFLECTION = 2;
        public const float HITTER_WAR_LOWER_RATE = 6;
        public const float HITTER_WAR_UPPER_RATE = 9;
        public const float STARTER_WAR_INFLECTION = 2;
        public const float STARTER_WAR_LOWER_RATE = 6;
        public const float STARTER_WAR_UPPER_RATE = 9;
        public const float RELIEVER_WAR_RATE = 9;

        // Transaction ToIL Status
        public const int TL_SUSP = 1;
        public const int TL_INJ_ADD_SHORT = 2;
        public const int TL_INJ_ADD_LONG = 3;
        public const int TL_INJ_REM = 4;
        public const int TL_INJ_REHAB = 5;

        public static readonly DbContextOptions<SqliteDbContext> DB_OPTIONS = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite("Data Source=../../../../Db/BaseballStats.db;")
                //.EnableSensitiveDataLogging()
                //.LogTo(m => Console.WriteLine(m))
                .Options;
    }
}
