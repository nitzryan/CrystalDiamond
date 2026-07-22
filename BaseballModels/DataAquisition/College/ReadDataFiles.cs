using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition.College
{
    internal class ReadDataFiles
    {
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

                    int tbcId = int.Parse(columns[0]);
                    int year = int.Parse(columns[1]);

                    // Need to filter out a couple of players with bad data
                    int h = int.Parse(columns[12]);
                    int hit2B = int.Parse(columns[13]);
                    int hit3B = int.Parse(columns[14]);
                    int hr = int.Parse(columns[15]);
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
                        G = int.Parse(columns[9]),
                        AB = int.Parse(columns[10]),
                        R = int.Parse(columns[11]),
                        H = h,
                        H2B = hit2B,
                        H3B = hit3B,
                        HR = hr,
                        SB = int.Parse(columns[17]),
                        CS = int.Parse(columns[18]),
                        BB = int.Parse(columns[19]),
                        IBB = int.Parse(columns[20]),
                        K = int.Parse(columns[21]),
                        PA = int.Parse(columns[10]) +
                            int.Parse(columns[22]) +
                            int.Parse(columns[23]) +
                            int.Parse(columns[24]),
                        HBP = int.Parse(columns[24]),
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
                        mlbId = int.Parse(columns[40]),
                        draftYear = int.Parse(columns[41]),
                        draftOvr = int.Parse(columns[43]),
                        teamId = int.Parse(columns[45])
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
                        [0, 0, 0] : firstStats.borndate.Split('/').Select(f => int.Parse(f)).ToArray();

                    int BirthYear = birthDateValues[2];
                    int BirthMonth = birthDateValues[0];
                    int BirthDay = birthDateValues[1];

                    // Zero-out draft data if they played after drafted (means they did not sign)
                    int draftPick = player.First().draftOvr;
                    int lastYear = player.Max(f => f.year);
                    if (lastYear != player.First().draftYear)
                        draftPick = 0;

                    // Toggle if player should be included in training data
                    

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
                        IsEligible = Utilities.IsCollegePlayerElibible(db, player.First().mlbId, lastYear),
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

                        int expYears = ColUtilities.ExpStringToYears(p.exp);
                        hitterStats.Add(new College_HitterStats
                        {
                            TBCId = p.tbcId,
                            Year = p.year,
                            Level = ColUtilities.LevelStringToInt(p.level),
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
                            Age = ColUtilities.GetSeasonAge(p.year, BirthDay, BirthMonth, BirthYear, expYears),
                            Pos = ColUtilities.ParsePosString(p.position),
                            Height = ColUtilities.GetHeightInInches(p.height),
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

                    int tbcId = int.Parse(columns[0]);
                    int year = int.Parse(columns[1]);

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
                        G = int.Parse(columns[11]),
                        GS = int.Parse(columns[12]),
                        IP = columns[17],
                        H = int.Parse(columns[18]),
                        HR = int.Parse(columns[19]),
                        R = int.Parse(columns[20]),
                        ER = int.Parse(columns[21]),
                        BB = int.Parse(columns[22]),
                        IBB = int.Parse(columns[23]),
                        K = int.Parse(columns[24]),
                        HBP = int.Parse(columns[27]),
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
                        mlbId = int.Parse(columns[44]),
                        draftYear = int.Parse(columns[45]),
                        draftOvr = int.Parse(columns[47]),
                        teamId = int.Parse(columns[49])
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
                    if (player.All(f => !f.position.Contains('P') && f.G < 5))
                        continue;

                    // Craig Johnson has a year of pitching after hitting, likely different player.  
                    // Messes up logic, so just skip
                    if (player.Key == 46230)
                        continue;

                    var firstStats = player.First();
                    int[] birthDateValues = firstStats.borndate == "" ?
                        [0, 0, 0] : firstStats.borndate.Split('/').Select(f => int.Parse(f)).ToArray();

                    int BirthYear = birthDateValues[2];
                    int BirthMonth = birthDateValues[0];
                    int BirthDay = birthDateValues[1];

                    // Zero-out draft data if they played after drafted (means they did not sign)
                    int draftPick = player.First().draftOvr;
                    int lastYear = player.Max(f => f.year);
                    if (lastYear != player.First().draftYear)
                        draftPick = 0;

                    // Check if they already exist
                    if (db.College_Player.Any(f => f.TBCId == player.Key))
                    {
                        var dbPlayer = db.College_Player.Where(f => f.TBCId == player.Key).Single();
                        dbPlayer.IsPitcher = true;
                        dbPlayer.DraftOvrPitcher = dbPlayer.DraftOvrHitter;

                        // Need to check if a player has more pitching data than hitting data
                        if (lastYear > dbPlayer.LastYear)
                        {
                            dbPlayer.LastYear = lastYear;
                            dbPlayer.DraftOvrPitcher = draftPick;
                        }
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
                            IsEligible = Utilities.IsCollegePlayerElibible(db, player.First().mlbId, lastYear),
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

                        int expYears = ColUtilities.ExpStringToYears(p.exp);
                        pitcherStats.Add(new College_PitcherStats
                        {
                            TBCId = p.tbcId,
                            Year = p.year,
                            Level = ColUtilities.LevelStringToInt(p.level),
                            TeamId = p.teamId,
                            ConfId = confMap.Where(f => f.Name.Equals(p.confAbvr)).Single().ConfId,
                            ExpYears = expYears,
                            G = p.G,
                            GS = p.GS,
                            R = p.R,
                            ER = p.ER,
                            Outs = ColUtilities.IPStringToOuts(p.IP),
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
                            Age = ColUtilities.GetSeasonAge(p.year, BirthDay, BirthMonth, BirthYear, expYears),
                            Height = ColUtilities.GetHeightInInches(p.height),
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
    }
}
