using PitchDb;

namespace UI
{
    public class Global
    {
        public record YearLeagueDevKey(int modelId, int year, int balls, int strikes);
        #pragma warning disable CA2211 // Set once at initialization
        public static Dictionary<YearLeagueDevKey, YearLeagueDeviations> YldDict = new();
        #pragma warning restore CA2211
    }
}
