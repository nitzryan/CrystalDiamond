using Db;
using Microsoft.EntityFrameworkCore;
using PitchDb;

namespace PitchAnalysis
{
    internal class Program
    {
        const bool FORCE_REFRESH = true;

        static void Main()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);

            if (FORCE_REFRESH)
            {
                pitchDb.Output_PitchValueAggregation.ExecuteDelete();
                pitchDb.YearLeagueDeviations.ExecuteDelete();
                pitchDb.PitchValue.ExecuteDelete();
                pitchDb.PitcherStuff.ExecuteDelete();
                pitchDb.PitcherStatcastMonth.ExecuteDelete();
                pitchDb.PitchModelResultBasis.ExecuteDelete();
            }

            PitchAggregation.Update();

            int startYear = pitchDb.Output_PitchValueAggregation.Min(f => f.Year);
            int endYear = pitchDb.Output_PitchValueAggregation.Max(f => f.Year);
            int endMonth = Math.Min(db.PitchStatcast
                .Where(f => f.Year == endYear)
                .Max(f => f.Month), 9);

            YearDeviations.Update(endYear);
            PitchStatcastOutput.Update();
            PitcherAggregator.CreateStats(endYear, FORCE_REFRESH);
            for (int year = startYear; year <= endYear; year++)
            {
                for (int month = 4; month <= 9; month++)
                {
                    if (year == endYear && month > endMonth)
                        break;

                    MonthStats.Update(month, year);
                }
                ModelViewerMinMax.Update(year, year == endYear);
            }

            //NullMonthStats.Update();
        }
    }
}
