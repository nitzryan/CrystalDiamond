using Db;
using ModelDb;

namespace ModelEvaluation.DraftPickBonus
{
    public class AggregatedDraftData
    {
        public int MlbId { get; set; }
        public int SigningYear { get; set; }
        public int DraftPick { get; set; }
        public bool IsHitter { get; set; }
        public int SlotValue { get; set; }
        public int SigningBonus { get; set; }
        public float ActualWar { get; set; }
        public float PredictedWar { get; set; }
    }

    public static class AggregateData
    {
        private const int YEARS_FROM_SIGNING = 3;
        private const int MODEL_ID = 1;

        private static readonly string SlotsDirectory =
            Path.Combine("..", "..", "..", "..", "DataAquisition", "OutputFiles", "DraftSlots");

        public static List<AggregatedDraftData> Aggregate(int cutoffYear)
        {
            // 1. Read every slots*.csv into a (year, pick) → slot lookup
            var slotLookup = ReadAllSlotFiles();
            var slotYears = slotLookup.Keys.Select(k => k.Year).ToHashSet();

            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            // 2. Players
            var players = db.Player
                .Where(p => p.DraftPick != null
                         && p.SigningYear <= cutoffYear
                         && p.SigningYear != 2020 // Short COVID draft
                         && p.Position != "TWP"
                         && slotYears.Contains(p.SigningYear!.Value))
                .Select(p => new {
                    p.MlbId,
                    p.Position,
                    DraftPick = p.DraftPick!.Value,
                    SigningYear = p.SigningYear!.Value,
                })
            .ToList();

            // Draft Bonuses
            var bonuses = db.Draft_Results
                .Where(f => slotYears.Contains(f.Year))
                .ToDictionary(
                    f => (f.Year,f.Pick), 
                    f => f.Bonus);


            // Actual WAR
            var modelPlayerDict = db.Model_Players
                .Select(mp => new { mp.MlbId, mp.WarHitter, mp.WarPitcher })
                .ToDictionary(mp => mp.MlbId);

            // Model Predicted Stats
            var warAggregations = modelDb.Output_PlayerWarAggregation
                .Where(o => o.ModelId == MODEL_ID)
                .ToList()
                .GroupBy(o => (o.MlbId, o.IsHitter))
                .ToDictionary(g => g.Key, g => g.ToList());

            var results = new List<AggregatedDraftData>();

            foreach (var player in players)
            {
                bool isHitter = player.Position == "H";
                int signingYear = player.SigningYear;
                int draftPick = player.DraftPick;

                // Slot value from CSV
                if (!slotLookup.TryGetValue((signingYear, draftPick), out int slotValue))
                    continue;

                // Actual WAR from Model_Players
                if (!modelPlayerDict.TryGetValue(player.MlbId, out var mp))
                    continue;

                float actualWar = isHitter ? mp.WarHitter : mp.WarPitcher;

                // Predicted WAR: latest entry where Year <= signingYear + N
                if (!warAggregations.TryGetValue((player.MlbId, isHitter), out var aggregations))
                    continue;

                var prediction = aggregations
                    .Where(a => a.Year <= signingYear + YEARS_FROM_SIGNING)
                    .OrderByDescending(a => a.Year)
                    .ThenByDescending(a => a.Month)
                    .FirstOrDefault();

                if (prediction is null)
                    continue;

                if (!bonuses.TryGetValue((player.SigningYear, player.DraftPick), out var signingBonus))
                    continue;

                results.Add(new AggregatedDraftData
                {
                    MlbId = player.MlbId,
                    SigningYear = player.SigningYear,
                    DraftPick = player.DraftPick,
                    IsHitter = isHitter,
                    SlotValue = slotValue,
                    SigningBonus = signingBonus,
                    ActualWar = actualWar,
                    PredictedWar = prediction.War,
                });
            }

            return results;
        }

        private static Dictionary<(int Year, int Pick), int> ReadAllSlotFiles()
        {
            var lookup = new Dictionary<(int Year, int Pick), int>();

            foreach (var file in Directory.GetFiles(SlotsDirectory, "slots*.csv"))
            {
                foreach (var line in File.ReadLines(file))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(',');
                    int year = int.Parse(parts[0].Trim());
                    int pick = int.Parse(parts[1].Trim());
                    int slot = int.Parse(parts[2].Trim());

                    lookup[(year, pick)] = slot;
                }
            }

            return lookup;
        }
    }
}
