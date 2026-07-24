using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition.ModelStats
{
    internal class LeagueAveragePlayerAge
    {
        public static void Update()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            db.LeagueAverageAge.ExecuteDelete();

            // Load all players to not hit DB each time
            var playerBirthdates = db.Player.ToDictionary(
                f => f.MlbId,
                f => new { f.BirthYear, f.BirthMonth, f.BirthDate }
            );

            // Get Playing time for each month
            var hitterData = db.Player_Hitter_MonthStats
                .Select(f => new { f.LeagueId, f.Year, f.Month, f.PA, f.MlbId })
                .GroupBy(f => new { f.LeagueId, f.Year, f.Month })
                .ToList();

            var pitcherData = db.Player_Pitcher_MonthStats
                .Select(f => new {f.LeagueId, f.Year, f.Month, f.BattersFaced, f.MlbId})
                .GroupBy(f => new { f.LeagueId, f.Year, f.Month })
                .ToDictionary(f => f.Key, f => f);

            using (ProgressBar progressBar = new ProgressBar(hitterData.Count(), "Calculating League average ages"))
            {
                foreach (var hd in hitterData)
                {
                    var pd = pitcherData[hd.Key];

                    // Get weighted sum of playing time and age

                    // Hitter
                    double sumHitterAge = 0;
                    int sumHitterPA = 0;
                    foreach (var h in hd)
                    {
                        if (!playerBirthdates.TryGetValue(h.MlbId, out var birthdate))
                            continue;
                        
                        double age = Utilities.GetAge1MinusAge0(h.Year, h.Month, 15, birthdate.BirthYear, birthdate.BirthMonth, birthdate.BirthDate);

                        // Add PA and weighted age
                        sumHitterPA += h.PA;
                        sumHitterAge += (h.PA * age);
                    }
                    float averageHitterAge = (float)sumHitterAge / sumHitterPA;

                    // Pitcher
                    double sumPitcherAge = 0;
                    int sumPitcherBF = 0;
                    foreach (var p in pd)
                    {
                        if (!playerBirthdates.TryGetValue(p.MlbId, out var birthdate))
                            continue;

                        double age = Utilities.GetAge1MinusAge0(p.Year, p.Month, 15, birthdate.BirthYear, birthdate.BirthMonth, birthdate.BirthDate);

                        // Add PA and weighted age
                        sumPitcherBF += p.BattersFaced;
                        sumPitcherAge += (p.BattersFaced * age);
                    }
                    float averagePitcherAge = (float)sumPitcherAge / sumPitcherBF;

                    // Insert data
                    db.LeagueAverageAge.Add(new LeagueAverageAge
                    {
                        LeagueId = hd.Key.LeagueId,
                        Year = hd.Key.Year,
                        Month = hd.Key.Month,
                        HitterAge = averageHitterAge,
                        PitcherAge = averagePitcherAge
                    });

                    progressBar.Tick();
                }
            }

            db.SaveChanges();
        }
    }
}
