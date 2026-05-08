using Db;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class YearDeviations
    {
        const float PITCH_CAP = 0.1f;

        public static void Update(int endYear, bool forceRefresh)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            if (forceRefresh)
                pitchDb.YearLeagueDeviations.ExecuteDelete();
            else
                pitchDb.YearLeagueDeviations.Where(f => f.Year >= endYear).ExecuteDelete();

                var years = pitchDb.Output_PitchValue.Select(f => f.Year).Distinct();
            years = years.Where(f => !pitchDb.YearLeagueDeviations.Select(f => f.Year).Contains(f));
            var models = pitchDb.Output_PitchValue.Select(f => f.Model).Distinct();

            using (ProgressBar progressBar = new ProgressBar(years.Count(), "Calculating Year Deviations"))
            {
                foreach (int year in years)
                {
                    double actualDev = Math.Sqrt(db.PitchStatcast.Where(f => f.Year == year && f.LevelId == 1).Select(f => f.RunValueHitter * f.RunValueHitter).Average());

                    foreach (int modelId in models)
                    {
                        double locationDev = Math.Sqrt(pitchDb.Output_PitchValue
                            .Where(f => f.Year == year && f.Model == modelId)
                            .Select(f => f.LocationOnly)
                            .AsEnumerable()
                            .Select(v => Math.Clamp(v, -PITCH_CAP, PITCH_CAP))
                            .Select(v => v * v)
                            .Average());
                        double stuffDev = Math.Sqrt(pitchDb.Output_PitchValue
                            .Where(f => f.Year == year && f.Model == modelId)
                            .Select(f => f.StuffOnly)
                            .AsEnumerable()
                            .Select(v => Math.Clamp(v, -PITCH_CAP, PITCH_CAP))
                            .Select(v => v * v)
                            .Average());
                        double pitchDev = Math.Sqrt(pitchDb.Output_PitchValue
                            .Where(f => f.Year == year && f.Model == modelId)
                            .Select(f => f.Combined)
                            .AsEnumerable()
                            .Select(v => Math.Clamp(v, -PITCH_CAP, PITCH_CAP))
                            .Select(v => v * v)
                            .Average());

                        pitchDb.YearLeagueDeviations.Add(new YearLeagueDeviations
                        {
                            Year = year,
                            ModelId = modelId,
                            ActDev = (float)actualDev,
                            LocDev = (float)locationDev,
                            StuffDev = (float)stuffDev,
                            PitchDev = (float)pitchDev
                        });
                    }

                    progressBar.Tick();
                }
            }

            pitchDb.SaveChanges();
        }
    }
}
