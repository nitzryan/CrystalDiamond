using Db;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;
using ShellProgressBar;

namespace DataAquisition
{
    internal class College
    {
        // Maps team name to same as BaseballCube so lookup can occur
        private static string BoydToBaseballCubeTeamName(string boydName)
        {
            switch (boydName)
            {
            default:
                return boydName;
            }
        }

        public static bool GetConfStrength(int year)
        {
            return false;
        }

        private static int LevelStringToInt(string lvl)
        {
            switch(lvl)
            {
                default:
                    throw new Exception($"Unexpected Level String: {lvl}");
            }
        }

        private static int ExpStringToYears(string exp)
        {
            switch (exp)
            {
                default:
                    throw new Exception($"Unexpected Exp String: {exp}");
            }
        }

        private static float GetSeasonAge(int year, int birthDay, int birthMonth, int birthYear)
        {
            return Utilities.GetAge1MinusAge0(year, 5, 1, birthYear, birthMonth, birthDay);
        }

        private static int GetHeightInInches(string height)
        {
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

            string[] positions = pos.Split('-');
            foreach (string p in positions)
            {
                switch (p)
                {
                    case "C":
                        position |= DbEnums.CollegePosition.C;
                        break;
                    case "1B":
                        position |= DbEnums.CollegePosition.B1;
                        break;
                    case "2B":
                        position |= DbEnums.CollegePosition.B2;
                        break;
                    case "3B":
                        position |= DbEnums.CollegePosition.B3;
                        break;
                    case "SS":
                        position |= DbEnums.CollegePosition.SS;
                        break;
                    case "LF":
                        position |= DbEnums.CollegePosition.LF;
                        break;
                    case "CF":
                        position |= DbEnums.CollegePosition.CF;
                        break;
                    case "RF":
                        position |= DbEnums.CollegePosition.RF;
                        break;
                    case "DH":
                        position |= DbEnums.CollegePosition.DH;
                        break;
                    case "IF":
                        position |= DbEnums.CollegePosition.IF;
                        break;
                    case "OF":
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

            // Read Stats File
            Dictionary<(int tbcId, int year), (int tbcId, 
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
                string borndate)> statsFileEntries = new();

            string filepath = Constants.DATA_AQ_DIRECTORY + "/TBC/hitterStats.csv";
            Console.WriteLine("Reading " + filepath);
            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] columns = line.Split(',');

                    int tbcId = Int32.Parse(columns[0]);
                    int year = Int32.Parse(columns[1]);

                    statsFileEntries.Add(new() {
                        tbcId = tbcId,
                        year = year,
                    }, new()
                    {
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
                        H = Int32.Parse(columns[12]),
                        H2B = Int32.Parse(columns[13]),
                        H3B = Int32.Parse(columns[14]),
                        HR = Int32.Parse(columns[15]),
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
                        borndate = columns[36]
                    });
                }
            }

            // Read player file
            List<(int tbcId,
                int year,
                int mlbId,
                string teamName,
                int draftYear,
                int draftOvr,
                int teamId)> playerFileEntries = new();

            filepath = Constants.DATA_AQ_DIRECTORY + "/TBC/hitterBios.csv";
            Console.WriteLine("Reading " + filepath);
            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] columns = line.Split(',');

                    playerFileEntries.Add(new()
                    {
                        tbcId = Int32.Parse(columns[0]),
                        year = Int32.Parse(columns[1]),
                        teamName = columns[2],
                        mlbId = Int32.Parse(columns[9]),
                        draftYear = Int32.Parse(columns[10]),
                        draftOvr = Int32.Parse(columns[12]),
                        teamId = Int32.Parse(columns[14])
                    });
                }
            }

            // Conf and Team Maps
            string[] conferences = statsFileEntries.Values.Select(f => f.confAbvr).Distinct().ToArray();
            List<College_ConfMap> confMap = new();
            for (var i = 0; i < conferences.Length; i++)
            {
                confMap.Add(new College_ConfMap
                {
                    Id = i,
                    Name = conferences[i]
                });
            }
            db.College_ConfMap.AddRange(confMap);

            var teams = playerFileEntries.DistinctBy(f => f.teamId);
            foreach (var t in teams)
            {
                db.College_TeamMap.Add(new College_TeamMap
                {
                    Id = t.teamId,
                    Name = t.teamName
                });
            }

            // Iterate through players
            var playerFileGroupings = playerFileEntries.GroupBy(f => f.tbcId);
            List<College_Player> players = new();
            List<College_HitterStats> hitterStats = new();
            players.Capacity = playerFileGroupings.Count();
            hitterStats.Capacity = players.Capacity * 4;
            using (ProgressBar progressBar = new(playerFileGroupings.Count(), $"Parsing College Hitter Stats"))
            {
                foreach (var player in playerFileGroupings)
                {
                    progressBar.Tick();
                    // Check to make sure that player is not a pure-pitcher that had some ABs
                    if (!player.Select(f => statsFileEntries[(f.tbcId, f.year)]).Any(f => f.position != "P"))
                        continue;

                    var firstStats = statsFileEntries[(player.Key, player.First().year)];
                    int[] birthDateValues = firstStats.borndate.Split('/').Select(f => Int32.Parse(f)).ToArray();

                    int BirthYear = birthDateValues[2];
                    int BirthMonth = birthDateValues[0];
                    int BirthDay = birthDateValues[1];
                    // Playe
                    players.Add(new College_Player
                    {
                        TBCId = player.Key,
                        MlbId = player.First().mlbId,
                        FirstName = firstStats.firstname,
                        LastName = firstStats.lastname,
                        BirthYear = BirthYear,
                        BirthMonth = BirthMonth,
                        BirthDay = BirthDay,
                        DraftOvr = player.First().draftOvr,
                        FirstYear = player.Min(f => f.year),
                        LastYear = player.Max(f => f.year),
                        Bats = firstStats.bats,
                        Throws = firstStats.throws,
                        IsPitcher = false,
                        IsHitter = true,
                    });

                    // Stats
                    foreach (var p in player)
                    {
                        var stat = statsFileEntries[(p.tbcId, p.year)];

                        hitterStats.Add(new College_HitterStats
                        {
                            TBCId = stat.tbcId,
                            Year = stat.year,
                            Level = LevelStringToInt(stat.level),
                            TeamId = p.teamId,
                            ConfId = confMap.Where(f => f.Name.Equals(p.teamName)).Single().Id,
                            ExpYears = ExpStringToYears(stat.exp),
                            AB = stat.AB,
                            PA = stat.PA,
                            H = stat.H,
                            H2B = stat.H2B,
                            H3B = stat.H3B,
                            HR = stat.HR,
                            SB = stat.SB,
                            CS = stat.CS,
                            BB = stat.BB,
                            IBB = stat.IBB,
                            K = stat.K,
                            HBP = stat.HBP,
                            AVG = stat.AVG,
                            OBP = stat.OBP,
                            SLG = stat.SLG,
                            OPS = stat.OPS,
                            Age = GetSeasonAge(stat.year, BirthDay, BirthMonth, BirthYear),
                            Pos = ParsePosString(stat.position),
                            Height = GetHeightInInches(stat.height),
                            Weight = stat.weight,
                        });
                    }
                }
            }

            db.College_Player.AddRange(players);
            db.College_HitterStats.AddRange(hitterStats);

            return true;
        }

        public static bool InsertCollegePitcherStats()
        {
            // Remove current data
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.College_PitcherStats.ExecuteDelete();

            // Read Stats File
            Dictionary<(int tbcId, int year), (int tbcId,
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
                string borndate)> statsFileEntries = new();

            string filepath = Constants.DATA_AQ_DIRECTORY + "/TBC/pitcherStats.csv";
            Console.WriteLine("Reading " + filepath);
            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] columns = line.Split(',');

                    int tbcId = Int32.Parse(columns[0]);
                    int year = Int32.Parse(columns[1]);

                    statsFileEntries.Add(new()
                    {
                        tbcId = tbcId,
                        year = year,
                    }, new()
                    {
                        tbcId = tbcId,
                        year = year,
                        school = columns[2],
                        confAbvr = columns[3],
                        level = columns[5],
                        lastname = columns[6],
                        firstname = columns[7],
                        exp = columns[8],
                        G = Int32.Parse(columns[9]),
                        GS = Int32.Parse(columns[10]),
                        IP = columns[15],
                        H = Int32.Parse(columns[16]),
                        HR = Int32.Parse(columns[17]),
                        R = Int32.Parse(columns[18]),
                        ER = Int32.Parse(columns[19]),
                        BB = Int32.Parse(columns[20]),
                        IBB = Int32.Parse(columns[21]),
                        K = Int32.Parse(columns[22]),
                        HBP = Int32.Parse(columns[25]),
                        ERA = float.Parse(columns[26]),
                        H9 = float.Parse(columns[27]),
                        HR9 = float.Parse(columns[28]),
                        BB9 = float.Parse(columns[29]),
                        K9 = float.Parse(columns[30]),
                        WHIP = float.Parse(columns[31]),
                        height = columns[33],
                        weight = int.Parse(columns[34]),
                        bats = columns[35],
                        throws = columns[36],
                        position = columns[37],
                        borndate = columns[38]
                    });
                }
            }

            // Read player file
            List<(int tbcId,
                int year,
                int mlbId,
                string teamName,
                int draftYear,
                int draftOvr,
                int teamId)> playerFileEntries = new();

            filepath = Constants.DATA_AQ_DIRECTORY + "/TBC/pitcherBios.csv";
            Console.WriteLine("Reading " + filepath);
            using (StreamReader reader = new StreamReader(filepath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] columns = line.Split(',');

                    playerFileEntries.Add(new()
                    {
                        tbcId = Int32.Parse(columns[0]),
                        year = Int32.Parse(columns[1]),
                        teamName = columns[2],
                        mlbId = Int32.Parse(columns[9]),
                        draftYear = Int32.Parse(columns[10]),
                        draftOvr = Int32.Parse(columns[12]),
                        teamId = Int32.Parse(columns[14])
                    });
                }
            }

            // Conf and Team Maps
            string[] conferences = statsFileEntries.Values.Select(f => f.confAbvr).Distinct().ToArray();
            List<College_ConfMap> confMap = new();
            for (var i = 0; i < conferences.Length; i++)
            {
                confMap.Add(new College_ConfMap
                {
                    Id = i,
                    Name = conferences[i]
                });
            }
            db.College_ConfMap.AddRange(confMap);

            var teams = playerFileEntries.DistinctBy(f => f.teamId);
            foreach (var t in teams)
            {
                db.College_TeamMap.Add(new College_TeamMap
                {
                    Id = t.teamId,
                    Name = t.teamName
                });
            }

            // Iterate through players
            var playerFileGroupings = playerFileEntries.GroupBy(f => f.tbcId);
            List<College_Player> players = new();
            List<College_PitcherStats> pitcherStats = new();
            players.Capacity = playerFileGroupings.Count();
            pitcherStats.Capacity = players.Capacity * 4;
            using (ProgressBar progressBar = new(playerFileGroupings.Count(), $"Parsing College Hitter Stats"))
            {
                foreach (var player in playerFileGroupings)
                {
                    progressBar.Tick();
                    // Check to make sure that player is not a pure-hitter that had some pitching
                    if (!player.Select(f => statsFileEntries[(f.tbcId, f.year)]).Any(f => f.position.Contains('P')))
                        continue;

                    var firstStats = statsFileEntries[(player.Key, player.First().year)];
                    int[] birthDateValues = firstStats.borndate.Split('/').Select(f => Int32.Parse(f)).ToArray();

                    int BirthYear = birthDateValues[2];
                    int BirthMonth = birthDateValues[0];
                    int BirthDay = birthDateValues[1];
                    // Player
                    // Check if they already exist
                    if (db.College_Player.Any(f => f.TBCId == player.Key))
                    {
                        db.College_Player.Where(f => f.TBCId == player.Key).Single().IsPitcher = true;
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
                            DraftOvr = player.First().draftOvr,
                            FirstYear = player.Min(f => f.year),
                            LastYear = player.Max(f => f.year),
                            Bats = firstStats.bats,
                            Throws = firstStats.throws,
                            IsPitcher = true,
                            IsHitter = false,
                        });
                    }


                    // Stats
                    foreach (var p in player)
                    {
                        var stat = statsFileEntries[(p.tbcId, p.year)];

                        pitcherStats.Add(new College_PitcherStats
                        {
                            TBCId = stat.tbcId,
                            Year = stat.year,
                            Level = LevelStringToInt(stat.level),
                            TeamId = p.teamId,
                            ConfId = confMap.Where(f => f.Name.Equals(p.teamName)).Single().Id,
                            ExpYears = ExpStringToYears(stat.exp),
                            G = stat.G,
                            GS = stat.GS,
                            R = stat.R,
                            ER = stat.ER,
                            Outs = IPStringToOuts(stat.IP),
                            H = stat.H,
                            HR = stat.HR,
                            BB = stat.BB,
                            K = stat.K,
                            HBP = stat.HBP,
                            ERA = stat.ERA,
                            H9 = stat.H9,
                            HR9 = stat.HR9,
                            BB9 = stat.BB9,
                            K9 = stat.K9,
                            WHIP = stat.WHIP,
                            Age = GetSeasonAge(stat.year, BirthDay, BirthMonth, BirthYear),
                            Height = GetHeightInInches(stat.height),
                            Weight = stat.weight,
                        });
                    }
                }
            }

            db.College_Player.AddRange(players);
            db.College_PitcherStats.AddRange(pitcherStats);

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

            return true;
        }

        private static float NormalizeStat(int stat, int opp, float confAvg)
        {
            return (float)((stat) / opp) / confAvg;
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
                confRankLookup[(c.ConfId, c.Year)] = c.ISR;
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
                    College_ConfHitterAvg confAvg = confLookup[(stat.ConfId, stat.Year)];

                    modelStats.Add(new Model_College_HitterYear
                    {
                        TBCId = stat.TBCId,
                        Level = stat.Level,
                        ExpYears = stat.ExpYears,
                        ParkRunFactor = parkLookup[(stat.TeamId, stat.Year)],
                        ConfScore = confRankLookup[(stat.ConfId, stat.Year)],
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
                        Height = stat.Height,
                        Weight = stat.Weight,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_HitterYear.AddRange(modelStats);

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
                confRankLookup[(c.ConfId, c.Year)] = c.ISR;
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
                    College_ConfPitcherAvg confAvg = confLookup[(stat.ConfId, stat.Year)];

                    modelStats.Add(new Model_College_PitcherYear
                    {
                        TBCId = stat.TBCId,
                        Level = stat.Level,
                        ExpYears = stat.ExpYears,
                        ParkRunFactor = parkLookup[(stat.TeamId, stat.Year)],
                        ConfScore = confRankLookup[(stat.ConfId, stat.Year)],
                        G = stat.G,
                        GS = stat.GS,
                        Outs = stat.Outs,
                        ERA = stat.ERA / confAvg.ERA,
                        H9 = stat.H9 / confAvg.H9,
                        HR9 = stat.HR9 / confAvg.HR9,
                        BB9 = stat.BB9 / confAvg.BB9,
                        K9 = stat.K9 / confAvg.K9,
                        WHIP = stat.WHIP / confAvg.WHIP,
                        Age = stat.Age,
                        Height = stat.Height,
                        Weight = stat.Weight,
                    });

                    progressBar.Tick();
                }
            }

            db.Model_College_PitcherYear.AddRange(modelStats);

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

            // Make GET request
            HttpClient httpClient = new();
            int reqYear = year == 2002 ? 2003 : year; // Different req for 2002
            HttpResponseMessage response = await httpClient.GetAsync($"http://www.boydsworld.com/data/pf{reqYear}.html");
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
            for (int i = 2; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int parkFactor))
                    {
                        // Everything after the first two numbers is the team name
                        string boydTeamName = string.Join(" ", parts, 2, parts.Length - 2);
                        string teamName = BoydToBaseballCubeTeamName(boydTeamName);
                        int teamId = db.College_TeamMap.Where(f => f.Name.Equals(teamName)).Single().Id;

                        teamParkFactors.Add((teamId, parkFactor));
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
            return true;
        }
    }
}
