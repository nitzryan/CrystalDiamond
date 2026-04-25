using Db;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;
using ShellProgressBar;
using static Db.DbEnums;
using EFCore.BulkExtensions;

namespace DataAquisition
{
    internal class College
    {
        private static int DEFAULT_HEIGHT = 72;
        private static float UKNOWN_START_AGE = 18.5f;

        private static int PRO_YEARS_FOR_STATS = 8;

        // Maps team name to same as BaseballCube so lookup can occur
        private static string BoydToBaseballCubeTeamName(string boydName)
        {
            switch (boydName)
            {
                case "Arkansas-Little Rock":
                    return "Little Rock";
                case "Austin Peay State":
                    return "Austin Peay";
                case "Bethune-Cookman":
                    return "Bethune Cookman";
                case "Bowling Green State":
                    return "Bowling Green";
                case "Cal State Sacramento":
                    return "Sacramento State";
                case "CSU Bakersfield":
                    return "Cal State Bakersfield";
                case "Centenary":
                    return "Centenary-LA";
                case "Central Connecticut State":
                    return "Central Connecticut";
                case "College of Charleston":
                    return "College Of Charleston";
                case "East Tennessee state":
                    return "East Tennessee State";
                case "Florida International":
                    return "Florida Intl";
                case "Houston Baptist":
                    return "Houston Christian";
                case "Indiana-Purdue FW":
                case "IPFW":
                    return "Purdue Fort Wayne";
                case "LaSalle":
                    return "La Salle";
                case "LeMoyne":
                    return "Le Moyne";
                case "LIU Brooklyn":
                    return "Long Island";
                case "Louisiana State":
                    return "LSU";
                case "Louisiana-Lafayette":
                    return "Louisiana";
                case "Maryland-Baltimore County":
                    return "UMBC";
                case "Massachusetts-Lowell":
                    return "Mass-Lowell";
                case "Miami, Florida":
                    return "Miami";
                case "Miami, Ohio":
                    return "Miami-Ohio";
                case "Middle Tennessee State":
                    return "Middle Tennessee";
                case "Mississippi":
                    return "Ole Miss";
                case "Mississippi Valley State":
                    return "Mississippi Valley";
                case "Monmouth":
                    return "Monmouth NJ";
                case "Nevada-Las Vegas":
                    return "UNLV";
                case "North Carolina-Asheville":
                    return "UNC Asheville";
                case "North Carolina-Charlotte":
                    return "UNC Charlotte";
                case "North Carolina-Greensboro":
                case "UNC-Greensboro":
                    return "UNC Greensboro";
                case "North Carolina-Wilmington":
                    return "UNC Wilmington";
                case "Pennsylvania":
                    return "Penn";
                case "Southeast Missouri State":
                    return "SE Missouri State";
                case "Southeastern Louisiana":
                    return "SE Louisiana";
                case "Southern California":
                    return "USC";
                case "Southern Illinois-Edwardsville":
                    return "SIU Edwardsville";
                case "Southern Mississippi":
                    return "Southern Miss";
                case "Southwest Missouri State":
                    return "Missouri State";
                case "South Carolina-Upstate":
                    return "USC Upstate";
                case "St. Francis":
                    return "St. Francis NY";
                case "St. Joseph's":
                    return "Saint Joseph's";
                case "St. Louis":
                    return "Saint Louis";
                case "St. Mary's":
                    return "St. Mary's CA";
                case "St. Peter's":
                    return "Saint Peter's";
                case "Stephen F. Austin State":
                    return "Stephen F. Austin";
                case "Texas A&M-Corpus Christi":
                    return "Texas A&M Corpus Christi";
                case "Texas-Arlington":
                    return "UT Arlington";
                case "Texas-Pan American":
                case "Texas-Rio Grande Valley":
                    return "UTRGV";
                case "Texas-San Antonio":
                    return "UT San Antonio";
                case "The Citadel":
                    return "Citadel";
                case "Troy State":
                    return "Troy";
                case "Utah Valley State":
                    return "Utah Valley";
                case "Virginia Commonwealth":
                    return "VCU";
                case "VMI":
                    return "Virginia Military";
                case "William and Mary":
                    return "William And Mary";
                case "Wisconsin-Milwaukee":
                    return "Milwaukee";
                case "Southwest Texas State":
                    return "Texas State";
                default:
                    return boydName;
            }
        }

        // On team name changes, Boyd incorrectly includes the wrong one
        private static bool BoydYearSkip(string boydTeamName, int year)
        {
            if (boydTeamName == "Cal State Sacramento" && year >= 2016)
                return true;
            if (boydTeamName == "Texas-Pan American" && year >= 2016)
                return true;
            if (boydTeamName == "Houston Baptist" && year >= 2024)
                return true;

            return false;
        }

        public static bool UpdateConfStrength(int year)
        {
            // Remove current data
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.College_ConferenceRank.Where(f => f.Year == year).ExecuteDelete();

            // Read Stats File
            List<(int rank, string conference)> teamRanks = new();

            string filepath = Constants.DATA_AQ_DIRECTORY + $"/TBC/RPI/confRpi_{year}.csv";
            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line;
                reader.ReadLine(); // Skip first line
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] columns = line.Split(',');

                    teamRanks.Add(new()
                    {
                        rank = Int32.Parse(columns[1]),
                        conference = columns[3]
                    });
                }
            }

            var conferenceRankings = teamRanks.GroupBy(f => f.conference);
            foreach (var conf in conferenceRankings)
            {
                float avgRank = (float)(conf.Sum(f => f.rank)) / conf.Count();
                College_ConfMap dbConf = db.College_ConfMap.Where(f => f.Name == conf.Key).Single();
                db.College_ConferenceRank.Add(new College_ConferenceRank
                {
                    Year = year,
                    ConfId = dbConf.ConfId,
                    AvgRPI = avgRank
                });
            }

            // Need to add 2018 Independents
            if (year == 2018)
            {
                db.College_ConferenceRank.Add(new College_ConferenceRank
                {
                    Year = 2018,
                    ConfId = 21,
                    AvgRPI = 300
                });
            }
            
            // Need to set Oregon State (2025 Independent) to a higher RPI
            if (year >= 2025)
            {
                db.SaveChanges();
                db.College_ConferenceRank.Where(f => f.Year == year && f.ConfId == 21).Single().AvgRPI = 40;
            }
            db.SaveChanges();

            return true;
        }

        private static int LevelStringToInt(string lvl)
        {
            switch(lvl)
            {
                case "NCAA-1":
                    return 1;
                default:
                    throw new Exception($"Unexpected Level String: {lvl}");
            }
        }

        private static int ExpStringToYears(string exp)
        {
            switch (exp.ToLower())
            {
                // Regular
                case "fr":
                case "fr.":
                case "fr ":
                case "cr":
                    return 1;

                case "so":
                case "so.":
                case "so ":
                case "s":
                    return 2;

                case "jr":
                case "jr.":
                case "jr ":
                    return 3;

                case "senior":
                case "sr":
                case "sr.":
                case "sr ":
                    return 4;

                case "5s":
                case "5th yr":
                case "sr+":
                    return 5;

                case "gr":
                case "gr.":
                case "6th":
                    return 6;

                // Redshirt
                case "rs":
                case "redsh":
                    return 1;

                case "rf":
                case "r-fr.":
                case "rs-rf":
                case "rsfr.":
                case "fr-r":
                case "rs fr":
                case "rs-fr":
                case "rsfr":
                    return 2;

                case "rs so":
                case "rs-so":
                case "rsso":
                case "r-so.":
                    return 3;

                case "jr-r":
                case "r-jr.":
                case "rs jr":
                case "rs-jr":
                case "rsjr.":
                case "rsjr":
                    return 4;

                case "rs sr":
                case "r-sr.":
                case "rs-sr":
                case "rssr.":
                case "rs-tr":
                case "sr-r":
                    return 5;

                case "rs-5s":
                case "rs-gr":
                case "5s+":
                case "6s":
                case "gr+":
                    return 6;

                // Invalid Cases
                case "0":
                case "b":
                case "basket":
                case "d":
                case "f":
                case "footbl":
                case "fy":
                case "null":
                case "x":
                    return 1;
                
                case "15":
                case "25":
                    return 2;

                case "23":
                case "-":
                case "":
                    return 3;

                case "33":
                case "35":
                case "srn47":
                    return 4;

                case "45":
                case "fifth":
                case "4.6":
                    return 5;

                default:
                    throw new Exception($"Unexpected Exp String: {exp}");
            }
        }

        private static float GetSeasonAge(int year, int birthDay, int birthMonth, int birthYear, int expYears)
        {
            float age = Utilities.GetAge1MinusAge0(year, 5, 1, birthYear, birthMonth, birthDay);
            
            // Check that age is valid, and not missing/incorrect data
            if (age < 100 && age >= 16)
                return age;

            // Estimate age for missing players
            return 18.5f + expYears;
        }

        private static int GetHeightInInches(string height)
        {
            if (height == "'" || height == "'-" || height == "")
                return 0;
            if (height == "'-59" || height == "5-91" || height == "5-96" || height == "5-99")
                return (5 * 12) + 9;
            if (height == ("'6-+0") || height == "'60-" || height == "'6" || height == "'6-")
                return 6 * 12;
            if (height == "'6-21")
                return (6 * 12) + 2;
        
            int[] heightParts = height.Split("'").Last().Split('-').Select(f => Int32.Parse(f)).ToArray();

            return (heightParts[0] * 12) + heightParts[1];
        }

        private static int IPStringToOuts(string ip)
        {
            int[] parts = ip.Split('.').Select(f => Int32.Parse(f)).ToArray();

            int outs = parts[0] * 3;
            if (parts.Length == 2)
                outs += parts[1];

            return outs;
        }

        private static DbEnums.CollegePosition ParsePosString(string pos)
        {
            DbEnums.CollegePosition position = DbEnums.CollegePosition.None;

            string[] positions = pos.Split(['-', '/', '=', '0']).Select(f => f.Replace(" ", "")).ToArray();
            foreach (string p in positions)
            {
                switch (p.ToUpper())
                {
                    case "C":
                    case "2":
                        position |= DbEnums.CollegePosition.C;
                        break;
                    case "1B":
                    case "1B`":
                    case "3:":
                        position |= DbEnums.CollegePosition.B1;
                        break;
                    case "2B":
                    case "2b":
                    case "4":
                        position |= DbEnums.CollegePosition.B2;
                        break;
                    case "3B":
                    case "3b":
                    case "5":
                    case "CIF":
                        position |= DbEnums.CollegePosition.B3;
                        break;
                    case "SS":
                        position |= DbEnums.CollegePosition.SS;
                        break;
                    case "LF":
                    case "L":
                        position |= DbEnums.CollegePosition.LF;
                        break;
                    case "CF":
                        position |= DbEnums.CollegePosition.CF;
                        break;
                    case "RF":
                    case "R":
                        position |= DbEnums.CollegePosition.RF;
                        break;
                    case "DH":
                    case "H":
                    case "COACH":
                    case "B":
                    case "UMP":
                    case "KF":
                    case "":
                        position |= DbEnums.CollegePosition.DH;
                        break;
                    case "IF":
                    case "IFF":
                    case "IB":
                    case "INF":
                    case "PF":
                    case "UF":
                    case "UT":
                    case "UTI":
                    case "UTL":
                    case "UTIL":
                    case "F":
                    case "I":
                        position |= DbEnums.CollegePosition.IF;
                        break;
                    case "OF":
                    case "O":
                        position |= DbEnums.CollegePosition.OF;
                        break;
                    case "P":
                        position |= DbEnums.CollegePosition.P;
                        break;
                    default:
                        throw new Exception($"Unexpected Pos String: {p}");
                }
            }

            return position;
        }

        public static bool InsertCollegeHitterStats()
        {
            // Remove current data
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.College_HitterStats.ExecuteDelete();
            db.College_Player.ExecuteDelete();
            db.College_TeamMap.ExecuteDelete();
            db.College_ConfMap.ExecuteDelete();

            // Read Stats File
            List<(int tbcId, 
                int year, 
                string school, 
                string confAbvr, 
                string level,
                string lastname,
                string firstname,
                string exp,
                int G,
                int AB,
                int R,
                int H,
                int H2B,
                int H3B,
                int HR,
                int SB,
                int CS,
                int BB,
                int IBB,
                int K,
                int PA,
                int HBP,
                float AVG,
                float SLG,
                float OBP,
                float OPS,
                string height,
                int weight,
                string bats,
                string throws,
                string position,
                string borndate,
                int mlbId,
                int draftYear,
                int draftOvr,
                int teamId) > hitterData = new();

            string filepath = Constants.DATA_AQ_DIRECTORY + "/TBC/hitterStats.csv";
            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line;
                reader.ReadLine(); // Skip first line
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Need to handle that some columns have commas in them, so split of "," and manually remove first and last "
                    line = line.Substring(1, line.Length - 2);
                    string[] columns = line.Split("\",\"");

                    int tbcId = Int32.Parse(columns[0]);
                    int year = Int32.Parse(columns[1]);

                    // Need to filter out a couple of players with bad data
                    int h = Int32.Parse(columns[12]);
                    int hit2B = Int32.Parse(columns[13]);
                    int hit3B = Int32.Parse(columns[14]);
                    int hr = Int32.Parse(columns[15]);
                    if (hr > h || hit2B > h || hit3B > h)
                    {
                        continue;
                    }

                    hitterData.Add(new() {
                        tbcId = tbcId,
                        year = year,
                        school = columns[2],
                        confAbvr = columns[3],
                        level = columns[5],
                        lastname = columns[6],
                        firstname = columns[7],
                        exp = columns[8],
                        G = Int32.Parse(columns[9]),
                        AB = Int32.Parse(columns[10]),
                        R = Int32.Parse(columns[11]),
                        H = h,
                        H2B = hit2B,
                        H3B = hit3B,
                        HR = hr,
                        SB = Int32.Parse(columns[17]),
                        CS = Int32.Parse(columns[18]),
                        BB = Int32.Parse(columns[19]),
                        IBB = Int32.Parse(columns[20]),
                        K = Int32.Parse(columns[21]),
                        PA = Int32.Parse(columns[10]) +
                            Int32.Parse(columns[22]) +
                            Int32.Parse(columns[23]) +
                            Int32.Parse(columns[24]),
                        HBP = Int32.Parse(columns[24]),
                        AVG = float.Parse(columns[26]),
                        SLG = float.Parse(columns[27]),
                        OBP = float.Parse(columns[28]),
                        OPS = float.Parse(columns[29]),
                        height = columns[31],
                        weight = int.Parse(columns[32]),
                        bats = columns[33],
                        throws = columns[34],
                        position = columns[35],
                        borndate = columns[36],
                        mlbId = Int32.Parse(columns[40]),
                        draftYear = Int32.Parse(columns[41]),
                        draftOvr = Int32.Parse(columns[43]),
                        teamId = Int32.Parse(columns[45])
                    });
                }
            }

            // Conf and Team Maps
            string[] conferences = hitterData.Select(f => f.confAbvr).Distinct().ToArray();
            List<College_ConfMap> confMap = new();
            for (var i = 0; i < conferences.Length; i++)
            {
                confMap.Add(new College_ConfMap
                {
                    ConfId = i + 1,
                    Name = conferences[i]
                });
            }
            db.College_ConfMap.AddRange(confMap);

            var teams = hitterData.GroupBy(f => f.teamId).Select(f => f.MaxBy(g => g.year));
            foreach (var t in teams)
            {
                // Make sure first letter is capitalized
                var name = t.school.Split(' ');
                for (int i = 0; i < name.Length; i++)
                {
                    if (!string.IsNullOrEmpty(name[i]))
                    {
                        char firstChar = char.ToUpper(name[i][0]);
                        name[i] = firstChar + name[i].Substring(1);
                    }
                }
                db.College_TeamMap.Add(new College_TeamMap
                {
                    TeamId = t.teamId,
                    Name = string.Join(" ", name)
                });
            }

            // Iterate through players
            var hitterGroupings = hitterData.GroupBy(f => f.tbcId);
            List<College_Player> players = new();
            List<College_HitterStats> hitterStats = new();
            players.Capacity = hitterGroupings.Count();
            hitterStats.Capacity = hitterData.Count();
            
            using (ProgressBar progressBar = new(hitterGroupings.Count(), $"Parsing College Hitter Stats"))
            {
                foreach (var player in hitterGroupings)
                {
                    progressBar.Tick();
                    // Check to make sure that player is not a pure-pitcher that had some ABs
                    if (player.All(f => f.position == "P" && f.AB < 50))
                        continue;

                    var firstStats = player.First();
                    int[] birthDateValues = firstStats.borndate == "" ? 
                        [0, 0, 0] : firstStats.borndate.Split('/').Select(f => Int32.Parse(f)).ToArray();

                    int BirthYear = birthDateValues[2];
                    int BirthMonth = birthDateValues[0];
                    int BirthDay = birthDateValues[1];

                    // Zero-out draft data if they played after drafted (means they did not sign)
                    int draftPick = player.First().draftOvr;
                    int lastYear = player.Max(f => f.year);
                    if (lastYear != player.First().draftYear)
                        draftPick = 0;

                    // Player
                    players.Add(new College_Player
                    {
                        TBCId = player.Key,
                        MlbId = player.First().mlbId,
                        FirstName = firstStats.firstname,
                        LastName = firstStats.lastname,
                        BirthYear = BirthYear,
                        BirthMonth = BirthMonth,
                        BirthDay = BirthDay,
                        DraftOvrHitter = draftPick,
                        DraftOvrPitcher = 0,
                        FirstYear = player.Min(f => f.year),
                        LastYear = lastYear,
                        Bats = firstStats.bats,
                        Throws = firstStats.throws,
                        IsPitcher = false,
                        IsHitter = true,
                    });

                    // Stats
                    foreach (var p in player)
                    {
                        // Don't log empty stats
                        if (p.G == 0)
                            continue;

                        // For duplicates, take column with more stats
                        if (player.Count(f => f.year == p.year) > 1)
                        {
                            int maxGames = player.Where(f => f.year == p.year).Max(f => f.G);
                            if (p.G < maxGames)
                                continue;

                            // Exact duplicate already entered
                            if (hitterStats.Any(f => f.TBCId == p.tbcId && f.Year == p.year))
                                continue;
                        }

                        int expYears = ExpStringToYears(p.exp);
                        hitterStats.Add(new College_HitterStats
                        {
                            TBCId = p.tbcId,
                            Year = p.year,
                            Level = LevelStringToInt(p.level),
                            TeamId = p.teamId,
                            ConfId = confMap.Where(f => f.Name.Equals(p.confAbvr)).Single().ConfId,
                            ExpYears = expYears,
                            AB = p.AB,
                            PA = p.PA,
                            H = p.H,
                            H2B = p.H2B,
                            H3B = p.H3B,
                            HR = p.HR,
                            SB = p.SB,
                            CS = p.CS,
                            BB = p.BB,
                            IBB = p.IBB,
                            K = p.K,
                            HBP = p.HBP,
                            AVG = p.AVG,
                            OBP = p.OBP,
                            SLG = p.SLG,
                            OPS = p.OPS,
                            Age = GetSeasonAge(p.year, BirthDay, BirthMonth, BirthYear, expYears),
                            Pos = ParsePosString(p.position),
                            Height = GetHeightInInches(p.height),
                            Weight = p.weight,
                        });
                    }
                }
            }

            db.College_Player.AddRange(players);
            db.College_HitterStats.AddRange(hitterStats);
            db.SaveChanges();

            return true;
        }

        public static bool InsertCollegePitcherStats()
        {
            // Remove current data
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.College_PitcherStats.ExecuteDelete();

            // Read Stats File
            List<(int tbcId,
                int year,
                string school,
                string confAbvr,
                string level,
                string lastname,
                string firstname,
                string exp,
                int G,
                int GS,
                string IP,
                int H,
                int HR,
                int R,
                int ER,
                int BB,
                int IBB,
                int K,
                int HBP,
                float ERA,
                float H9,
                float HR9,
                float BB9,
                float K9,
                float WHIP,
                string height,
                int weight,
                string bats,
                string throws,
                string position,
                string borndate,
                int mlbId,
                int draftYear,
                int draftOvr,
                int teamId)> pitcherData = new();

            string filepath = Constants.DATA_AQ_DIRECTORY + "/TBC/pitcherStats.csv";
            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line;
                reader.ReadLine(); // Skip first line
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Need to handle that some columns have commas in them, so split of "," and manually remove first and last "
                    line = line.Substring(1, line.Length - 2);
                    string[] columns = line.Split("\",\"");

                    int tbcId = Int32.Parse(columns[0]);
                    int year = Int32.Parse(columns[1]);

                    // John Luke Marlin only has 1 IP listed, but stats are for more than 1IP so skip
                    if (tbcId == 262940 && year == 2024)
                        continue;

                    pitcherData.Add(new()
                    {
                        tbcId = tbcId,
                        year = year,
                        school = columns[2],
                        confAbvr = columns[3],
                        level = columns[5],
                        lastname = columns[7],
                        firstname = columns[8],
                        exp = columns[6],
                        G = Int32.Parse(columns[11]),
                        GS = Int32.Parse(columns[12]),
                        IP = columns[17],
                        H = Int32.Parse(columns[18]),
                        HR = Int32.Parse(columns[19]),
                        R = Int32.Parse(columns[20]),
                        ER = Int32.Parse(columns[21]),
                        BB = Int32.Parse(columns[22]),
                        IBB = Int32.Parse(columns[23]),
                        K = Int32.Parse(columns[24]),
                        HBP = Int32.Parse(columns[27]),
                        ERA = float.Parse(columns[28]),
                        H9 = float.Parse(columns[29]),
                        HR9 = float.Parse(columns[30]),
                        BB9 = float.Parse(columns[31]),
                        K9 = float.Parse(columns[32]),
                        WHIP = float.Parse(columns[33]),
                        height = columns[35],
                        weight = int.Parse(columns[36]),
                        bats = columns[37],
                        throws = columns[38],
                        position = columns[39],
                        borndate = columns[40],
                        mlbId = Int32.Parse(columns[44]),
                        draftYear = Int32.Parse(columns[45]),
                        draftOvr = Int32.Parse(columns[47]),
                        teamId = Int32.Parse(columns[49])
                    });
                }
            }

            List<College_ConfMap> confMap = db.College_ConfMap.ToList();

            // Iterate through players
            var pitcherGroupings = pitcherData.GroupBy(f => f.tbcId);
            List<College_Player> players = new();
            List<College_PitcherStats> pitcherStats = new();
            players.Capacity = pitcherGroupings.Count();
            pitcherStats.Capacity = pitcherData.Count();
            
            using (ProgressBar progressBar = new(pitcherGroupings.Count(), $"Parsing College Pitcher Stats"))
            {
                foreach (var player in pitcherGroupings)
                {
                    progressBar.Tick();
                    // Check to make sure that player is not a pure-hitter that had some pitching
                    if (player.All(f => !f.position.Contains('P')))
                        continue;

                    var firstStats = player.First();
                    int[] birthDateValues = firstStats.borndate == "" ?
                        [0, 0, 0] : firstStats.borndate.Split('/').Select(f => Int32.Parse(f)).ToArray();

                    int BirthYear = birthDateValues[2];
                    int BirthMonth = birthDateValues[0];
                    int BirthDay = birthDateValues[1];

                    // Zero-out draft data if they played after drafted (means they did not sign)
                    int draftPick = player.First().draftOvr;
                    int lastYear = player.Max(f => f.year);
                    if (lastYear != player.First().draftYear)
                        draftPick = 0;

                    // Player
                    // Check if they already exist
                    if (db.College_Player.Any(f => f.TBCId == player.Key))
                    {
                        var dbPlayer = db.College_Player.Where(f => f.TBCId == player.Key).Single();
                        dbPlayer.IsPitcher = true;
                        dbPlayer.DraftOvrPitcher = dbPlayer.DraftOvrHitter;
                    }
                    else // Add
                    {
                        players.Add(new College_Player
                        {
                            TBCId = player.Key,
                            MlbId = player.First().mlbId,
                            FirstName = firstStats.firstname,
                            LastName = firstStats.lastname,
                            BirthYear = BirthYear,
                            BirthMonth = BirthMonth,
                            BirthDay = BirthDay,
                            DraftOvrHitter = 0,
                            DraftOvrPitcher = draftPick,
                            FirstYear = player.Min(f => f.year),
                            LastYear = lastYear,
                            Bats = firstStats.bats,
                            Throws = firstStats.throws,
                            IsPitcher = true,
                            IsHitter = false,
                        });
                    }


                    // Stats
                    foreach (var p in player)
                    {
                        // Don't log empty stats
                        if (p.G == 0)
                            continue;

                        // For duplicates, take column with more stats
                        if (player.Count(f => f.year == p.year) > 1)
                        {
                            int maxGames = player.Where(f => f.year == p.year).Max(f => f.G);
                            if (p.G < maxGames)
                                continue;

                            // Exact duplicate already entered
                            if (pitcherStats.Any(f => f.TBCId == p.tbcId && f.Year == p.year))
                                continue;
                        }

                        int expYears = ExpStringToYears(p.exp);
                        pitcherStats.Add(new College_PitcherStats
                        {
                            TBCId = p.tbcId,
                            Year = p.year,
                            Level = LevelStringToInt(p.level),
                            TeamId = p.teamId,
                            ConfId = confMap.Where(f => f.Name.Equals(p.confAbvr)).Single().ConfId,
                            ExpYears = expYears,
                            G = p.G,
                            GS = p.GS,
                            R = p.R,
                            ER = p.ER,
                            Outs = IPStringToOuts(p.IP),
                            H = p.H,
                            HR = p.HR,
                            BB = p.BB,
                            K = p.K,
                            HBP = p.HBP,
                            ERA = p.ERA,
                            H9 = p.H9,
                            HR9 = p.HR9,
                            BB9 = p.BB9,
                            K9 = p.K9,
                            WHIP = p.WHIP,
                            Age = GetSeasonAge(p.year, BirthDay, BirthMonth, BirthYear, expYears),
                            Height = GetHeightInInches(p.height),
                            Weight = p.weight,
                        });
                    }
                }
            }

            db.College_Player.AddRange(players);
            db.College_PitcherStats.AddRange(pitcherStats);
            db.SaveChanges();

            return true;
        }

        // Look up drafted player  to get the MLBId for some players
        public static bool FixDraftedMissingMLBIds()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var missingPlayers = db.College_Player.Where(f => f.MlbId == 0 && (f.DraftOvrHitter > 0 || f.DraftOvrPitcher > 0));

            foreach (var p in missingPlayers)
            {
                var dbPlayer = db.Player.Where(f => (f.DraftPick == p.DraftOvrHitter || f.DraftPick == p.DraftOvrPitcher) && f.SigningYear == p.LastYear);

                if (dbPlayer.Any())
                {
                    Player player = dbPlayer.Single();

                    // Make sure that last names match (a player might have been drafted but not signed
                    if (p.LastName.Replace(" ", "").ToLower() != player.UseLastName.Replace(" ", "").ToLower() &&
                        ((p.LastName + " Jr.").ToLower() != player.UseLastName.ToLower()))
                    {
                        throw new Exception($"Last Names did not match: TBC {p.LastName} vs MLB {player.UseLastName}");
                    }

                    p.MlbId = player.MlbId;
                }
            }

            db.SaveChanges();
            return true;
        }

        // Assign drafted players to either only be drafted as a hitter or pitcher, except in case of TWP
        // This way, the model can handle that a bad hitter could be drafted high as a pitcher and visa versa
        public static bool HandleTwoWayDraftedPlayers()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var twoWayDraftedPlayers = db.College_Player.Where(f => f.IsHitter && f.IsPitcher && f.DraftOvrHitter > 0)
                .Join(db.Player, cp => cp.MlbId, p => p.MlbId, (cp, p) => new { cp, p });

            using (ProgressBar progressBar = new(twoWayDraftedPlayers.Count(), $"Assigning Two-Way drafted players to hitter or pitcher"))
            {
                foreach (var p in twoWayDraftedPlayers)
                {
                    Player player = p.p;
                    College_Player collegePlayer = p.cp;

                    if (player.Position == "H")
                        collegePlayer.DraftOvrPitcher = 0;
                    else if (player.Position == "P")
                        collegePlayer.DraftOvrHitter = 0;

                    progressBar.Tick();
                }
            }

            db.SaveChanges();
            return true;
        }

        public static bool CreateConfAverages(int year)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.College_ConfHitterAvg.Where(f => f.Year == year).ExecuteDelete();
            db.College_ConfPitcherAvg.Where(f => f.Year == year).ExecuteDelete();

            var hitterConfStats = db.College_HitterStats.Where(f => f.Year == year)
                .GroupBy(f => f.ConfId);
            using (ProgressBar progressBar = new(hitterConfStats.Count(), $"Calculating Hitter Conference Stats for {year}"))
            {
                foreach (var stats in hitterConfStats)
                {
                    int pa = stats.Sum(f => f.PA);
                    int ab = stats.Sum(f => f.AB);
                    int hit2B = stats.Sum(f => f.H2B);
                    int hit3B = stats.Sum(f => f.H3B);
                    int hr = stats.Sum(f => f.HR);
                    int bb = stats.Sum(f => f.BB);
                    int hbp = stats.Sum(f => f.HBP);
                    int h = stats.Sum(f => f.H);
                    int hit1B = h - hit2B - hit3B - hr;

                    float obp = (float)(h + bb + hbp) / pa;
                    float slg = (float)(hit1B + (2 * hit2B) + (3 * hit3B) + (4 * hr)) / ab;

                    db.College_ConfHitterAvg.Add(new College_ConfHitterAvg
                    {
                        ConfId = stats.Key,
                        Year = year,
                        H = (float)h / ab,
                        H2B = (float)(hit2B) / ab,
                        H3B = (float)(hit3B) / ab,
                        HR = (float)(hr) / ab,
                        SB = (float)stats.Sum(f => f.SB) / pa,
                        CS = (float)stats.Sum(f => f.CS) / pa,
                        BB = (float)bb / pa,
                        IBB = (float)stats.Sum(f => f.IBB) / pa,
                        K = (float)stats.Sum(f => f.K) / pa,
                        HBP = (float)hbp/pa,
                        AVG = (float)h / ab,
                        OBP = obp,
                        SLG = slg,
                        OPS = obp + slg,
                    });

                    progressBar.Tick();
                }
            }

            var pitcherConfStats = db.College_PitcherStats.Where(f => f.Year == year)
                .GroupBy(f => f.ConfId);
            using (ProgressBar progressBar = new(pitcherConfStats.Count(), $"Calculating Pitcher Conference Stats for {year}"))
            {
                foreach (var stats in pitcherConfStats)
                {
                    int outs = stats.Sum(f => f.Outs);
                    int hr = stats.Sum(f => f.HR);
                    int bb = stats.Sum(f => f.BB);
                    int hbp = stats.Sum(f => f.HBP);
                    int h = stats.Sum(f => f.H);
                    int k = stats.Sum(f => f.K);
                    int er = stats.Sum(f => f.ER);

                    db.College_ConfPitcherAvg.Add(new College_ConfPitcherAvg
                    {
                        ConfId = stats.Key,
                        Year = year,
                        ERA = (float)(er * 27) / outs,
                        H9 = (float)(h * 27) / outs,
                        HR9 = (float)(hr * 27) / outs,
                        BB9 = (float)(bb * 27) / outs,
                        K9 = (float)(k * 27) / outs,
                        WHIP = (float)((h + bb + hbp) * 3) / outs,
                    });

                    progressBar.Tick();
                }
            }
            db.SaveChanges();

            return true;
        }

        private static float NormalizeStat(int stat, int opp, float confAvg)
        {
            if (confAvg == 0 || opp == 0)
                return 1;
            return ((float)(stat) / opp) / confAvg;
        }

        public static bool CreateHitterModelStats()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.Model_College_HitterYear.ExecuteDelete();

            // Construct dictionary to lookup conf stats
            Dictionary<(int confId, int year), College_ConfHitterAvg> confLookup = new();
            foreach (var c in db.College_ConfHitterAvg)
            {
                confLookup[(c.ConfId, c.Year)] = c;
            }

            // Construct dictionary of Conference Strengths
            Dictionary<(int confId, int year), float> confRankLookup = new();
            foreach (var c in db.College_ConferenceRank)
            {
                confRankLookup[(c.ConfId, c.Year)] = c.AvgRPI;
            }

            // Construct dictionary to lookup park factors
            Dictionary<(int teamId, int year), float> parkLookup = new();
            foreach (var pf in db.College_ParkFactors)
            {
                parkLookup[(pf.TeamId, pf.Year)] = pf.RunFactor;
            }

            // Iterate through all players
            List<Model_College_HitterYear> modelStats = new();
            modelStats.Capacity = db.College_HitterStats.Count();
            using (ProgressBar progressBar = new(db.College_HitterStats.Count(), $"Calculating College Hitter Model Stats"))
            {
                foreach (var stat in db.College_HitterStats)
                {
                    progressBar.Tick();

                    if (stat.PA == 0)
                        continue;

                    // Skip covid year
                    if (stat.Year == 2020) 
                        continue;
                        
                    College_ConfHitterAvg confAvg = confLookup[(stat.ConfId, stat.Year)];


                    float parkFactor = 1;
                    try
                    {
                        parkFactor = parkLookup[(stat.TeamId, stat.Year)];
                    }
                    catch (Exception)
                    {
                        // Doesn't exist
                    }
                    float confRank = confRankLookup[(stat.ConfId, stat.Year)];

                    // Cap invalid datapoints
                    int height = stat.Height > 0 ? stat.Height : DEFAULT_HEIGHT;
                    float age = stat.Age;
                    if (stat.Age > 100 || stat.Age < 16 + stat.ExpYears)
                        age = UKNOWN_START_AGE + stat.ExpYears;

                    modelStats.Add(new Model_College_HitterYear
                    {
                        TBCId = stat.TBCId,
                        Level = stat.Level,
                        Year = stat.Year,
                        ExpYears = stat.ExpYears,
                        ParkRunFactor = parkFactor,
                        ConfScore = confRank,
                        PA = stat.PA,
                        H = NormalizeStat(stat.H, stat.AB, confAvg.H),
                        H2B = NormalizeStat(stat.H2B, stat.AB, confAvg.H2B),
                        H3B = NormalizeStat(stat.H3B, stat.AB, confAvg.H3B),
                        HR = NormalizeStat(stat.HR, stat.AB, confAvg.HR),
                        SB = NormalizeStat(stat.SB, stat.PA, confAvg.SB),
                        CS = NormalizeStat(stat.CS, stat.PA, confAvg.CS),
                        BB = NormalizeStat(stat.BB, stat.PA, confAvg.BB),
                        K = NormalizeStat(stat.K, stat.PA, confAvg.K),
                        HBP = NormalizeStat(stat.HBP, stat.PA, confAvg.HBP),
                        AVG = stat.AVG / confAvg.AVG,
                        OBP = stat.OBP / confAvg.OBP,
                        SLG = stat.SLG / confAvg.SLG,
                        OPS = stat.OPS / confAvg.OPS,
                        Age = stat.Age,
                        Pos = stat.Pos,
                        Height = height,
                        Weight = stat.Weight,
                    });
                }
            }

            db.Model_College_HitterYear.AddRange(modelStats);
            db.SaveChanges();

            return true;
        }

        public static bool CreatePitcherModelStats()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.Model_College_PitcherYear.ExecuteDelete();

            // Construct dictionary to lookup conf stats
            Dictionary<(int confId, int year), College_ConfPitcherAvg> confLookup = new();
            foreach (var c in db.College_ConfPitcherAvg)
            {
                confLookup[(c.ConfId, c.Year)] = c;
            }

            // Construct dictionary of Conference Strengths
            Dictionary<(int confId, int year), float> confRankLookup = new();
            foreach (var c in db.College_ConferenceRank)
            {
                confRankLookup[(c.ConfId, c.Year)] = c.AvgRPI;
            }

            // Construct dictionary to lookup park factors
            Dictionary<(int teamId, int year), float> parkLookup = new();
            foreach (var pf in db.College_ParkFactors)
            {
                parkLookup[(pf.TeamId, pf.Year)] = pf.RunFactor;
            }

            // Iterate through all players
            List<Model_College_PitcherYear> modelStats = new();
            modelStats.Capacity = db.College_PitcherStats.Count();
            using (ProgressBar progressBar = new(db.College_PitcherStats.Count(), $"Calculating College Pitcher Model Stats"))
            {
                foreach (var stat in db.College_PitcherStats)
                {
                    progressBar.Tick();

                    if (stat.G == 0)
                        continue;

                    // Skip covid year
                    if (stat.Year == 2020)
                        continue;

                    College_ConfPitcherAvg confAvg = confLookup[(stat.ConfId, stat.Year)];

                    float parkFactor = 1;
                    try
                    {
                        parkFactor = parkLookup[(stat.TeamId, stat.Year)];
                    }
                    catch (Exception)
                    {
                        // Doesn't exist
                    }
                    float confRank = confRankLookup[(stat.ConfId, stat.Year)];

                    // Cap invalid datapoints
                    int height = stat.Height > 0 ? stat.Height : DEFAULT_HEIGHT;
                    float age = stat.Age;
                    if (stat.Age > 100 || stat.Age < 16 + stat.ExpYears)
                        age = UKNOWN_START_AGE + stat.ExpYears;

                    modelStats.Add(new Model_College_PitcherYear
                    {
                        TBCId = stat.TBCId,
                        Level = stat.Level,
                        Year = stat.Year,
                        ExpYears = stat.ExpYears,
                        ParkRunFactor = parkFactor,
                        ConfScore = confRank,
                        G = stat.G,
                        GS = stat.GS,
                        Outs = stat.Outs,
                        ERA = Utilities.SafeDivide(stat.ERA , confAvg.ERA),
                        H9 = Utilities.SafeDivide(stat.H9 , confAvg.H9),
                        HR9 = Utilities.SafeDivide(stat.HR9 , confAvg.HR9),
                        BB9 = Utilities.SafeDivide(stat.BB9 , confAvg.BB9),
                        K9 = Utilities.SafeDivide(stat.K9 , confAvg.K9),
                        WHIP = Utilities.SafeDivide(stat.WHIP , confAvg.WHIP),
                        Age = stat.Age,
                        Height = height,
                        Weight = stat.Weight,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_PitcherYear.AddRange(modelStats);
            db.SaveChanges();

            return true;
        }

        public async static Task<bool> GetParkFactors(int year)
        {
            // Remove old data
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.College_ParkFactors.Where(f => f.Year == year).ExecuteDelete();

            List<College_ParkFactors> parkFactors = new();
            // Covid year caused no data for these years
            if (year >= 2020 && year <= 2023)
            {
                var data2019 = db.College_ParkFactors.Where(f => f.Year == 2019);
                foreach (var d in data2019)
                {
                    parkFactors.Add(new College_ParkFactors
                    {
                        TeamId = d.TeamId,
                        Year = year,
                        RunFactor = d.RunFactor,
                    });
                }

                db.College_ParkFactors.AddRange(parkFactors);
                return true;
            }

            // Currently doesn't have a 2025 park factors, fallback to 2024
            if (year >= 2025)
            {
                var data2024 = db.College_ParkFactors.Where(f => f.Year == 2024);
                db.College_ParkFactors.Where(f => f.Year == 2019);
                foreach (var d in data2024)
                {
                    parkFactors.Add(new College_ParkFactors
                    {
                        TeamId = d.TeamId,
                        Year = year,
                        RunFactor = d.RunFactor,
                    });
                }

                db.College_ParkFactors.AddRange(parkFactors);
                return true;
            }

            // Make GET request
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36"
    );
            int reqYear = year == 2002 ? 2003 : year; // Different req for 2002
            string url = $"http://www.boydsworld.com/data/pf{reqYear}.html";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Getting College Park Factors for Year={year}: {response.StatusCode}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();

            // Get <pre> contents
            var doc = new HtmlDocument();
            doc.LoadHtml(responseBody);
            var preNode = doc.DocumentNode.SelectSingleNode("//pre") ?? throw new Exception($"Failed to find <pre> for College Park Factors for Year={year}");
            string contents = preNode.InnerText;

            // Read through line by line
            List<(int id, float pf)> teamParkFactors = new();

            var lines = contents.Split(new[] { '\r', '\n' });
            for (int i = 3; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int parkFactor))
                    {
                        // Everything after the first two numbers is the team name
                        string boydTeamName = string.Join(" ", parts, 2, parts.Length - 2);

                        // Boyd has some duplicates (multiple names for same team)(
                        if (BoydYearSkip(boydTeamName, year))
                            continue;

                        string teamName = BoydToBaseballCubeTeamName(boydTeamName);
                        
                        try
                        {
                            int teamId = db.College_TeamMap.Where(f => f.Name.Equals(teamName)).Single().TeamId;

                            teamParkFactors.Add((teamId, parkFactor));
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Skipped Park Factors for {teamName}");
                        }
                    }
                }
            }

            // Get all teams in a conference for a given year
            var conferenceTeams = db.College_HitterStats.Where(f => f.Year == year)
                .Select(f => new { f.TeamId, f.ConfId })
                .Distinct()
                .GroupBy(f => f.ConfId, f => f.TeamId);

            foreach (var conf in conferenceTeams)
            {
                // Get mean PF for conference
                var confParkFactors = teamParkFactors.Where(f => conf.Contains(f.id));
                float pfPerTeam = confParkFactors.Sum(f => f.pf) / confParkFactors.Count();

                // Add conf-adjusted park factor
                foreach (var team in confParkFactors)
                {
                    parkFactors.Add(new College_ParkFactors
                    {
                        TeamId = team.id,
                        Year = year,
                        RunFactor = team.pf / pfPerTeam,
                    });
                }
            }

            db.College_ParkFactors.AddRange(parkFactors);
            db.SaveChanges();

            return true;
        }

        public static bool CreatePlayerGaps()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            // Hitter
            var hitters = db.Model_College_HitterYear.GroupBy(f => f.TBCId);
            foreach (var hitter in hitters)
            {
                var orderedStats = hitter.OrderBy(f => f.Year);
                int year = orderedStats.First().Year;
                int expYear = orderedStats.First().ExpYears;
                float age = orderedStats.First().Age;

                foreach (var stat in orderedStats)
                {
                    while ((stat.Year - 1) > year)
                    {
                        year++;
                        expYear++;
                        age += 1;

                        // No COVID stats
                        if (year == 2020)
                        {
                            expYear--;
                            continue;
                        }

                        int height = stat.Height > 0 ? stat.Height : DEFAULT_HEIGHT;

                        db.Model_College_HitterYear.Add(new Model_College_HitterYear
                        {
                            TBCId = stat.TBCId,
                            Level = stat.Level,
                            Year = year,
                            ExpYears = expYear,
                            ParkRunFactor = 1,
                            ConfScore = 1,
                            PA = 0,
                            H = 1,
                            H2B = 1,
                            H3B = 1,
                            HR = 1,
                            SB = 1,
                            CS = 1,
                            BB = 1,
                            K = 1,
                            HBP = 1,
                            AVG = 1,
                            OBP = 1,
                            SLG = 1,
                            OPS = 1,
                            Age = age,
                            Pos = stat.Pos,
                            Height = height,
                            Weight = stat.Weight,
                        });
                    }

                    year = stat.Year;
                    expYear = stat.ExpYears;
                    age = stat.Age;
                }
            }

            // Pitcher
            var pitchers = db.Model_College_PitcherYear.GroupBy(f => f.TBCId);
            foreach (var pitcher in pitchers)
            {
                var orderedStats = pitcher.OrderBy(f => f.Year);
                int year = orderedStats.First().Year;
                int expYear = orderedStats.First().ExpYears;
                float age = orderedStats.First().Age;

                foreach (var stat in orderedStats)
                {
                    while ((stat.Year - 1) > year)
                    {
                        year++;
                        expYear++;
                        age += 1;

                        // No COVID stats
                        if (year == 2020)
                        {
                            expYear--;
                            continue;
                        }


                        int height = stat.Height > 0 ? stat.Height : DEFAULT_HEIGHT;

                        db.Model_College_PitcherYear.Add(new Model_College_PitcherYear
                        {
                            TBCId = stat.TBCId,
                            Level = stat.Level,
                            Year = year,
                            ExpYears = expYear,
                            ParkRunFactor = 1,
                            ConfScore = 1,
                            G = 0,
                            GS = 0,
                            Outs = 0,
                            ERA = 1,
                            H9 = 1,
                            HR9 = 1,
                            BB9 = 1,
                            K9 = 1,
                            WHIP = 1,
                            Age = age,
                            Height = height,
                            Weight = stat.Weight,
                        });
                    }

                    year = stat.Year;
                    expYear = stat.ExpYears;
                    age = stat.Age;
                }
            }

            db.SaveChanges();

            return true;
        }

        public static void DataCleanup()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            // Josh Morgan has wrong draft data
            db.College_Player.Where(f => f.TBCId == 38027).Single().DraftOvrHitter = 605;

            // The wrong Tyler Cox was listed as drafted 1044
            var tylerCox = db.College_Player.Where(f => f.TBCId == 55597).Single();
            tylerCox.DraftOvrPitcher = 0;
            tylerCox.DraftOvrHitter = 0;
            db.College_Player.Where(f => f.TBCId == 147199).Single().DraftOvrPitcher = 1044;

            // Some teams have 0 for strikeouts
            List<(int, int, int)> InvalidStrikeoutList = new(){
                // 2010 Rutgers
                (2010, 2144, 35),
                (2010, 22205, 25),
                (2010, 22996, 34),
                (2010, 129152, 37),
                (2010, 143001, 3),
                (2010, 144235, 33),
                (2010, 144236, 14),
                (2010, 144240, 42),
                (2010, 144241, 30),
                (2010, 151910, 5),
                (2010, 151927, 1),
                (2010, 153082, 35),
                (2010, 153091, 4),
                (2010, 158206, 5),
                (2010, 158225, 23),
                (2010, 158762, 10),
                (2010, 158922, 22),
                // 2002-2003 Belmont
                (2002, 26327, 32),
                (2002, 26351, 31),
                (2002, 27812, 17),
                (2002, 28127, 0),
                (2002, 31207, 1),
                (2002, 31804, 21),
                (2002, 32173, 8),
                (2002, 32920, 3),
                (2002, 33399, 20),
                (2002, 33419, 0),
                (2002, 33784, 27),
                (2002, 35266, 19),
                (2002, 35636, 21),
                (2002, 36388, 39),
                (2002, 36430, 21),
                (2002, 36680, 11),
                (2002, 161640, 25),

                (2003, 26351, 7),
                (2003, 27812, 26),
                (2003, 29021, 7),
                (2003, 32173, 8),
                (2003, 32920, 22),
                (2003, 33399, 39),
                (2003, 33419, 11),
                (2003, 33784, 35),
                (2003, 35266, 18),
                (2003, 35636, 35),
                (2003, 36388, 15),
                (2003, 36430, 17),
                (2003, 49464, 2),
                (2003, 161640, 26),
                // 2002 Binghamton
                (2002, 39527, 43),
                (2002, 177712, 35),
                // 2009 Utah Valley
                (2009, 150254, 24),
                (2009, 148805, 13),
                (2009, 22311, 25),
                (2009, 149594, 13),
                (2009, 149988, 6),
                (2009, 146631, 9),
                // Anthony Forgione
                (2005, 128894, 9),
                (2007, 128894, 10),
                (2008, 128894, 26),
                // 2002 Lafayette
                (2002, 39644, 16),
                (2002, 38525, 17),
                // Santiago Ruis, couldn't find so guess
                (2009, 164514, 14),
                (2010, 164514, 15),
            };
            foreach ((int year, int id, int k) in InvalidStrikeoutList)
            {
                //Console.WriteLine($"{year}-{id}");
                db.College_HitterStats.Where(f => f.Year == year && f.TBCId == id).Single().K = k;
            }

            List<(int, int, int)> InvalidWalkList = new()
            {
                // 2002 Wofford
                (2002, 233819, 36),
                (2002, 38696, 8),
                (2002, 40846, 10),
                (2002, 49769, 3),
                (2002, 233820, 4),
                (2002, 233821, 2),

                // 2002 Binghamton
                (2002, 177712, 7),
            };
            foreach ((int year, int id, int bb) in InvalidWalkList)
            {
                //Console.WriteLine($"{year}-{id}");
                var HS = db.College_HitterStats.Where(f => f.Year == year && f.TBCId == id).Single();
                HS.BB = bb;
                HS.PA += bb;
            }

            db.SaveChanges();
        }

        private static int GetLevelScale(int level)
        {
            switch (level)
            {
                case 1:
                    return 4;
                case 11:
                case 12:
                    return 2;
                default:
                    return 1;
            }
        }

        public static void CreateCollegeHittersProData(int currentYear)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.Model_College_HitterProStats.ExecuteDelete();

            var hitters = db.College_Player.Where(f => f.IsHitter);
            int hitterCount = hitters.Count();
            List<Model_College_HitterProStats> proStats = new();
            proStats.Capacity = hitterCount;
            using (ProgressBar progressBar = new(hitterCount, $"Calculating College Hitter Pro Stats"))
            {
                foreach (var hitter in hitters)
                {
                    // Get Fielding stats at each position
                    var fieldingStats = db.Player_Fielder_YearStats.Where(f => f.MlbId == hitter.MlbId
                        && f.Year <= (hitter.LastYear + PRO_YEARS_FOR_STATS));

                    int outsC = 0;
                    int outs1B = 0;
                    int outs2B = 0;
                    int outs3B = 0;
                    int outsSS = 0;
                    int outsLF = 0;
                    int outsCF = 0;
                    int outsRF = 0;
                    int outsDH = 0;
                    int defOuts = 0;
                    float defRuns = 0;

                    float mlbRuns = 0;
                    int mlbOuts = 0;

                    foreach (var fs in fieldingStats)
                    {
                        int scale = GetLevelScale(fs.LevelId);

                        int outs = scale * fs.Outs;
                        
                        switch(fs.Position)
                        {
                            case Position.C:
                                outsC += outs;
                                break;
                            case Position.B1:
                                outs1B += outs;
                                break;
                            case Position.B2:
                                outs2B += outs;
                                break;
                            case Position.B3:
                                outs3B += outs;
                                break;
                            case Position.SS:
                                outsSS += outs;
                                break;
                            case Position.LF:
                                outsLF += outs;
                                break;
                            case Position.CF:
                                outsCF += outs;
                                break;
                            case Position.RF:
                                outsRF += outs;
                                break;
                            case Position.DH:
                                outsDH += outs;
                                break;
                            default:
                                break;
                        }

                        defOuts += fs.Outs;
                        defRuns += scale * fs.ScaledDRAA;

                        if (fs.LevelId == 1)
                        {
                            mlbRuns += fs.ScaledDRAA;
                            mlbOuts += fs.Outs;
                        }
                    }

                    int totalOuts = outsC + outs1B + outs2B + outs3B + outsSS + outsLF + outsCF + outsRF + outsDH;

                    Model_Players? mp = db.Model_Players.Where(f => f.MlbId == hitter.MlbId).SingleOrDefault();

                    int mlbPA = 0;
                    float mlbWar = 0;
                    int signingYear = currentYear + 1;
                    float mlbOff = 0;
                    if (mp != null)
                    {
                        mlbPA = mp.TotalPA;
                        mlbWar = mp.WarHitter;
                        signingYear = mp.SigningYear;
                        mlbOff = mp.RateOff * mp.TotalPA;
                    }

                    

                    const float DEFAULT_OUTS = 1.0f / 9.0f;
                    proStats.Add(new Model_College_HitterProStats
                    {
                        TBCId = hitter.TBCId,
                        PercC = Utilities.SafeDivide(outsC, totalOuts, DEFAULT_OUTS),
                        Perc1B = Utilities.SafeDivide(outs1B, totalOuts, DEFAULT_OUTS),
                        Perc2B = Utilities.SafeDivide(outs2B, totalOuts, DEFAULT_OUTS),
                        Perc3B = Utilities.SafeDivide(outs3B, totalOuts, DEFAULT_OUTS),
                        PercSS = Utilities.SafeDivide(outsSS, totalOuts, DEFAULT_OUTS),
                        PercLF = Utilities.SafeDivide(outsLF, totalOuts, DEFAULT_OUTS),
                        PercCF = Utilities.SafeDivide(outsCF, totalOuts, DEFAULT_OUTS),
                        PercRF = Utilities.SafeDivide(outsRF, totalOuts, DEFAULT_OUTS),
                        PercDH = Utilities.SafeDivide(outsDH, totalOuts, DEFAULT_OUTS),
                        DEF = defRuns,
                        MLB_WAR = mlbWar,
                        DefOuts = defOuts,
                        MLB_PA = mlbPA,
                        MLB_DefPer1000IN = Utilities.SafeDivide(mlbRuns * 3000, mlbOuts, 0),
                        MLB_OFFPer600PA = Utilities.SafeDivide(mlbOff * 600, mlbPA, 0),
                        MLB_DefOuts = mlbOuts,
                        YearsSinceDraft = currentYear - signingYear,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_HitterProStats.AddRange(proStats);
            db.SaveChanges();
        }

        public static void CreateCollegePitchersProData(int currentYear)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.Model_College_PitcherProStats.ExecuteDelete();

            var pitchers = db.College_Player.Where(f => f.IsPitcher);
            int pitcherCount = pitchers.Count();
            List<Model_College_PitcherProStats> proStats = new();
            proStats.Capacity = pitcherCount;
            using (ProgressBar progressBar = new(pitcherCount, $"Calculating College Pitcher Pro Stats"))
            {
                foreach (var pitcher in pitchers)
                {
                    // Get Starting/relieving stats
                    var pitchingStats = db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == pitcher.MlbId
                        && f.MlbId != 0
                        && f.Year <= (pitcher.LastYear + PRO_YEARS_FOR_STATS));

                    float outsSP = 0;
                    float outsRP = 0;

                    foreach (var ps in pitchingStats)
                    {
                        int scale = GetLevelScale(ps.LevelId);

                        int outs = scale * ps.Outs;
                        outsSP += outs * ps.SPPerc;
                        outsRP += outs * (1 - ps.SPPerc);
                    }

                    float totalOuts = outsSP + outsRP;

                    Model_Players? mp = db.Model_Players.Where(f => f.MlbId == pitcher.MlbId).SingleOrDefault();

                    int mlbOuts = 0;
                    float mlbWar = 0;
                    int signingYear = currentYear + 1;
                    if (mp != null)
                    {
                        mlbOuts = mp.TotalOuts;
                        mlbWar = mp.WarPitcher;
                        signingYear = mp.SigningYear;
                    }

                    proStats.Add(new Model_College_PitcherProStats
                    {
                        TBCId = pitcher.TBCId,
                        PercSP = Utilities.SafeDivide(outsSP, totalOuts, 0.5f),
                        PercRP = Utilities.SafeDivide(outsRP, totalOuts, 0.5f),
                        MLB_WAR = mlbWar,
                        Outs = (int)totalOuts,
                        MLB_Outs = mlbOuts,
                        YearsSinceDraft = currentYear - signingYear,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_PitcherProStats.AddRange(proStats);
            db.SaveChanges();
        }
    }
}
