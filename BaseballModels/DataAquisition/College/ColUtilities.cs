using static Db.DbEnums;

namespace DataAquisition.College
{
    internal class ColUtilities
    {
        public static int DEFAULT_HEIGHT = 72;
        public static float UKNOWN_START_AGE = 18.5f;

        // Maps team name to same as BaseballCube so lookup can occur
        public static string BoydToBaseballCubeTeamName(string boydName)
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
        public static bool BoydYearSkip(string boydTeamName, int year)
        {
            if (boydTeamName == "Cal State Sacramento" && year >= 2016)
                return true;
            if (boydTeamName == "Texas-Pan American" && year >= 2016)
                return true;
            if (boydTeamName == "Houston Baptist" && year >= 2024)
                return true;

            return false;
        }

        public static int LevelStringToInt(string lvl)
        {
            return 1;
            //switch (lvl)
            //{
            //    case "NCAA-1":
            //    case "NCAA":
            //    case "Indy": // Data error
            //        return 1;
            //    default:
            //        throw new Exception($"Unexpected Level String: {lvl}");
            //}
        }

        public static int ExpStringToYears(string exp)
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

        public static float GetSeasonAge(int year, int birthDay, int birthMonth, int birthYear, int expYears)
        {
            float age = Utilities.GetAge1MinusAge0(year, 5, 1, birthYear, birthMonth, birthDay);

            // Check that age is valid, and not missing/incorrect data
            if (age < 100 && age >= 16)
                return age;

            // Estimate age for missing players
            return 18.5f + expYears;
        }

        public static int GetHeightInInches(string height)
        {
            if (height == "'" || height == "'-" || height == "")
                return 0;
            if (height == "'-59" || height == "5-91" || height == "5-96" || height == "5-99")
                return (5 * 12) + 9;
            if (height == "'6-+0" || height == "'60-" || height == "'6" || height == "'6-")
                return 6 * 12;
            if (height == "'6-21")
                return (6 * 12) + 2;

            int[] heightParts = height.Split("'").Last().Split('-').Select(f => int.Parse(f)).ToArray();
            if (heightParts.Length == 1)
                heightParts = [heightParts[0], 0];

            return heightParts[0] * 12 + heightParts[1];
        }

        public static int IPStringToOuts(string ip)
        {
            int[] parts = ip.Split('.').Select(f => int.Parse(f)).ToArray();

            int outs = parts[0] * 3;
            if (parts.Length == 2)
                outs += parts[1];

            return outs;
        }

        public static CollegePosition ParsePosString(string pos)
        {
            CollegePosition position = CollegePosition.None;

            string[] positions = pos.Split(['-', '/', '=', '0']).Select(f => f.Replace(" ", "")).ToArray();
            foreach (string p in positions)
            {
                switch (p.ToUpper())
                {
                    case "C":
                    case "2":
                        position |= CollegePosition.C;
                        break;
                    case "1B":
                    case "1B`":
                    case "3:":
                        position |= CollegePosition.B1;
                        break;
                    case "2B":
                    case "2b":
                    case "4":
                        position |= CollegePosition.B2;
                        break;
                    case "3B":
                    case "3b":
                    case "5":
                    case "CIF":
                        position |= CollegePosition.B3;
                        break;
                    case "SS":
                        position |= CollegePosition.SS;
                        break;
                    case "LF":
                    case "L":
                        position |= CollegePosition.LF;
                        break;
                    case "CF":
                        position |= CollegePosition.CF;
                        break;
                    case "RF":
                    case "R":
                        position |= CollegePosition.RF;
                        break;
                    case "DH":
                    case "H":
                    case "COACH":
                    case "B":
                    case "UMP":
                    case "KF":
                    case "":
                        position |= CollegePosition.DH;
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
                        position |= CollegePosition.IF;
                        break;
                    case "OF":
                    case "O":
                        position |= CollegePosition.OF;
                        break;
                    case "P":
                        position |= CollegePosition.P;
                        break;
                    default:
                        throw new Exception($"Unexpected Pos String: {p}");
                }
            }

            return position;
        }

        public static float NormalizeStat(int stat, int opp, float confAvg)
        {
            if (confAvg == 0 || opp == 0)
                return 1;
            return (float)stat / opp / confAvg;
        }

        public static int GetLevelScale(int level)
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
    }
}
