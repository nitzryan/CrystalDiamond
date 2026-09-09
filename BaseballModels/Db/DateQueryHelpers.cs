namespace Db
{
    /// <summary>Any generated table row that is keyed by a (Year, Month) pair.</summary>
    public interface IYearMonth
    {
        int Year { get; }
        int Month { get; }
    }

    // Attach the interface to the generated entities without editing the generated code.
    public partial class Player_Hitter_GameLog : IYearMonth { }
    public partial class Player_Pitcher_GameLog : IYearMonth { }
    public partial class Player_Hitter_MonthStats : IYearMonth { }
    public partial class Player_Pitcher_MonthStats : IYearMonth { }
    public partial class Player_Fielder_MonthStats : IYearMonth { }
    public partial class Player_Hitter_MonthBaserunning : IYearMonth { }
    public partial class Player_MonthlyWar : IYearMonth { }
    public partial class Model_LevelYearGames : IYearMonth { }
    public partial class Model_LeagueHittingBaselines : IYearMonth { }
    public partial class Model_LeaguePitchingBaselines : IYearMonth { }
    public partial class Model_HitterLevelStats : IYearMonth { }
    public partial class Model_PitcherLevelStats : IYearMonth { }
    public partial class Model_HitterStats : IYearMonth { }
    public partial class Model_PitcherStats : IYearMonth { }

    public static class DateQueryHelpers
    {
        /// Rolling 12-month block immediately AFTER (year, month): (year, month+1) .. (year+1, month).
        public static IQueryable<T> InTwelveMonthsAfter<T>(this IQueryable<T> source, int year, int month)
            where T : IYearMonth
            => source.Where(f => (f.Year == year && f.Month > month)
                              || (f.Year == year + 1 && f.Month <= month));

        /// Exact (year, month) bucket.
        public static IQueryable<T> AtYearMonth<T>(this IQueryable<T> source, int year, int month)
            where T : IYearMonth
            => source.Where(f => f.Year == year && f.Month == month);
    }
}
