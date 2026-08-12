using Db;
using PitchDb;

namespace PitchAnalysis
{
    internal class Program
    {
        const bool FORCE_REFRESH = false;

        static void Main()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);

            PitchAggregation.Update(FORCE_REFRESH);

            int startYear = pitchDb.Output_PitchValueAggregation.Min(f => f.Year);
            int endYear = pitchDb.Output_PitchValueAggregation.Max(f => f.Year);
            int endMonth = Math.Min(db.PitchStatcast
                .Where(f => f.Year == endYear)
                .Max(f => f.Month), 9);

            YearDeviations.Update(endYear, FORCE_REFRESH);
            PitchStatcastOutput.Update(FORCE_REFRESH);
            PitcherAggregator.CreateStats(FORCE_REFRESH, endYear);
            for (int year = startYear; year <= endYear; year++)
            {
                for (int month = 4; month <= 9; month++)
                {
                    if (year == endYear && month > endMonth)
                        break;

                    MonthStats.Update(month, year, FORCE_REFRESH);
                }
                ModelViewerMinMax.Update(year, year == endYear || FORCE_REFRESH);
            }

            //NullMonthStats.Update();
        }
    }
}
