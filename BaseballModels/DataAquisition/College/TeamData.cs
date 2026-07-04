using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition.College
{
    internal class TeamData
    {
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
                        rank = int.Parse(columns[1]),
                        conference = columns[3]
                    });
                }
            }

            var conferenceRankings = teamRanks.GroupBy(f => f.conference);
            foreach (var conf in conferenceRankings)
            {
                float avgRank = (float)conf.Sum(f => f.rank) / conf.Count();
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
                    float slg = (float)(hit1B + 2 * hit2B + 3 * hit3B + 4 * hr) / ab;

                    db.College_ConfHitterAvg.Add(new College_ConfHitterAvg
                    {
                        ConfId = stats.Key,
                        Year = year,
                        H = (float)h / ab,
                        H2B = (float)hit2B / ab,
                        H3B = (float)hit3B / ab,
                        HR = (float)hr / ab,
                        SB = (float)stats.Sum(f => f.SB) / pa,
                        CS = (float)stats.Sum(f => f.CS) / pa,
                        BB = (float)bb / pa,
                        IBB = (float)stats.Sum(f => f.IBB) / pa,
                        K = (float)stats.Sum(f => f.K) / pa,
                        HBP = (float)hbp / pa,
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


    }
}
