using ModelDb;

namespace UI.Types
{
    public class ModelResults
    {
        public List<Output_College_HitterAggregation>? ColHitOutput { get; init; }
        public List<Output_College_PitcherAggregation>? ColPitOutput { get; init; }
        public required List<Output_PlayerWarAggregation> ProWar { get; init; }
        public List<List<Output_HitterStatsAggregation>>? ProHitStats { get; init; }
        public List<List<Output_PitcherStatsAggregation>>? ProPitStats { get; init; }
    }
}
