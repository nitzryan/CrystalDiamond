using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using static DataAquisition.ModelStats.CalculateHitterStats;
using static DataAquisition.ModelStats.CalculatePitcherStats;
using static DataAquisition.ModelStats.ModelHitterCache;
using static DataAquisition.ModelStats.ModelPitcherCache;

namespace DataAquisition.ModelStats
{
    public record LeagueMonthKey(int Year, int Month, int LeagueId);

    public record LeagueHitterRates(
        float Hit1B, float Hit2B, float Hit3B, float HitHR,
        float BB, float HBP, float K, float SB, float CS);

    public record PlayerMonthKey(int MlbId, int Year, int Month);

    public record PlayerMonthLeagueKey(int MlbId, int Year, int Month, int LeagueId);

    public record PlayerBio(int MlbId, int BirthYear, int BirthMonth, int BirthDate, int SigningYear);

    public record ModelLeagueCache(
        Dictionary<LeagueMonthKey, LeagueHitterRates> LeagueHitterRates,
        Dictionary<LeagueMonthKey, float> HitterAverageAges,
        Dictionary<LeagueMonthKey, float> PitcherAverageAges,
        Dictionary<LeagueMonthKey, float> GameFractions)
    {
        

        public float GameFraction(LeagueMonthKey key) =>
            GameFractions.TryGetValue(key, out float f) ? f : 0f;

        public bool HasGames(LeagueMonthKey key) => GameFraction(key) >= 0.2f;

        public static ModelLeagueCache Generate()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            return new ModelLeagueCache(
                LoadLeagueHitterRates(db),
                LoadHitterAverageAges(db),
                LoadPitcherAverageAges(db),
                LoadGameFractions(db));
        }

        public static Dictionary<LeagueMonthKey, LeagueHitterRates> LoadLeagueHitterRates(SqliteDbContext db)
        {
            return db.League_HitterStats
                .Select(f => new
                {
                    f.LeagueId, f.Year, f.Month,
                    f.Hit1B, f.Hit2B, f.Hit3B, f.HitHR, f.BB, f.HBP, f.K, f.SB, f.CS,
                })
                .ToDictionary(
                    f => new LeagueMonthKey(f.Year, f.Month, f.LeagueId),
                    f => new LeagueHitterRates(
                        f.Hit1B, f.Hit2B, f.Hit3B, f.HitHR, f.BB, f.HBP, f.K, f.SB, f.CS));
        }

        public static Dictionary<LeagueMonthKey, float> LoadHitterAverageAges(SqliteDbContext db)
        {
            return db.LeagueAverageAge
                .Select(f => new { f.LeagueId, f.Year, f.Month, f.HitterAge })
                .ToDictionary(
                    f => new LeagueMonthKey(f.Year, f.Month, f.LeagueId),
                    f => f.HitterAge);
        }

        public static Dictionary<LeagueMonthKey, float> LoadPitcherAverageAges(SqliteDbContext db)
        {
            return db.LeagueAverageAge
                .Select(f => new { f.LeagueId, f.Year, f.Month, f.PitcherAge })
                .ToDictionary(
                    f => new LeagueMonthKey(f.Year, f.Month, f.LeagueId),
                    f => f.PitcherAge);
        }

        public static Dictionary<LeagueMonthKey, float> LoadGameFractions(SqliteDbContext db)
        {
            var counts = db.League_GameCounts
                .Select(f => new { f.LeagueId, f.Year, f.Month, f.MaxPA })
                .ToList();

            Dictionary<(int, int), int> yearMax = counts
                .GroupBy(f => (f.LeagueId, f.Year))
                .ToDictionary(g => g.Key, g => g.Max(f => f.MaxPA));

            var result = new Dictionary<LeagueMonthKey, float>(counts.Count);
            foreach (var f in counts)
            {
                int max = yearMax[(f.LeagueId, f.Year)];
                result[new LeagueMonthKey(f.Year, f.Month, f.LeagueId)] = max > 0 ? (float)f.MaxPA / max : 0f;
            }
            return result;
        }
    }

    public record HitterSourceData(
        IQueryable<Model_Players> Model_Players,
        IQueryable<Player> Player,
        IQueryable<Player_Hitter_MonthlyRatios> Player_Hitter_MonthlyRatios,
        IQueryable<Player_Hitter_MonthStats> Player_Hitter_MonthStats,
        IQueryable<Player_Hitter_MonthAdvanced> Player_Hitter_MonthAdvanced,
        IQueryable<Player_Fielder_MonthStats> Player_Fielder_MonthStats,
        IQueryable<Player_MonthlyWar> Player_MonthlyWar,
        IQueryable<Player_Hitter_MonthBaserunning> Player_Hitter_MonthBaserunning,
        IQueryable<Transaction_Log> Transaction_Log)
    {
        public static HitterSourceData FromDb(SqliteDbContext db) => new(
            db.Model_Players.AsNoTracking(),
            db.Player,
            db.Player_Hitter_MonthlyRatios.AsNoTracking(),
            db.Player_Hitter_MonthStats,
            db.Player_Hitter_MonthAdvanced,
            db.Player_Fielder_MonthStats,
            db.Player_MonthlyWar,
            db.Player_Hitter_MonthBaserunning,
            db.Transaction_Log);

        public static HitterSourceData ForPlayer(
            Model_Players modelPlayer,
            Player player,
            IEnumerable<Player_Hitter_MonthlyRatios> monthlyRatios,
            IEnumerable<Player_Hitter_MonthStats> monthStats,
            IEnumerable<Player_Hitter_MonthAdvanced> monthAdvanced,
            IEnumerable<Player_Fielder_MonthStats> fielderMonthStats,
            IEnumerable<Player_MonthlyWar> monthlyWar,
            IEnumerable<Player_Hitter_MonthBaserunning> monthBaserunning,
            IEnumerable<Transaction_Log> transactionLog)
        {
            // Ensure player has valid signing year
            if (player.SigningYear == null)
                throw new ArgumentNullException("Player had invalid signing year");

            return new(
                new[] { modelPlayer }.AsQueryable(),
                new[] { player }.AsQueryable(),
                monthlyRatios.AsQueryable(),
                monthStats.AsQueryable(),
                monthAdvanced.AsQueryable(),
                fielderMonthStats.AsQueryable(),
                monthlyWar.AsQueryable(),
                monthBaserunning.AsQueryable(),
                transactionLog.AsQueryable());
        }
    }

    public record PitcherSourceData(
        IQueryable<Model_Players> Model_Players,
        IQueryable<Player> Player,
        IQueryable<Player_Pitcher_MonthlyRatios> Player_Pitcher_MonthlyRatios,
        IQueryable<Player_Pitcher_MonthStats> Player_Pitcher_MonthStats,
        IQueryable<Player_Pitcher_MonthAdvanced> Player_Pitcher_MonthAdvanced,
        IQueryable<Transaction_Log> Transaction_Log)
    {
        public static PitcherSourceData FromDb(SqliteDbContext db) => new(
            db.Model_Players.AsNoTracking(),
            db.Player,
            db.Player_Pitcher_MonthlyRatios.AsNoTracking(),
            db.Player_Pitcher_MonthStats,
            db.Player_Pitcher_MonthAdvanced,
            db.Transaction_Log);

        public static PitcherSourceData ForPlayer(
            Model_Players modelPlayer,
            Player player,
            IEnumerable<Player_Pitcher_MonthlyRatios> monthlyRatios,
            IEnumerable<Player_Pitcher_MonthStats> monthStats,
            IEnumerable<Player_Pitcher_MonthAdvanced> monthAdvanced,
            IEnumerable<Transaction_Log> transactionLog)
        {
            // Ensure player has valid signing year
            if (player.SigningYear == null)
                throw new ArgumentNullException("Player had invalid signing year");

            return new(
                new[] { modelPlayer }.AsQueryable(),
                new[] { player }.AsQueryable(),
                monthlyRatios.AsQueryable(),
                monthStats.AsQueryable(),
                monthAdvanced.AsQueryable(),
                transactionLog.AsQueryable());
        }
    }

    public record ModelHitterCache(
        Dictionary<int, Model_Players> ModelPlayers,
        Dictionary<int, PlayerBio> PlayerBios,
        Dictionary<int, List<Player_Hitter_MonthlyRatios>> MonthlyRatios,
        Dictionary<PlayerMonthLeagueKey, HitterMonthStats> MonthStats,
        Dictionary<PlayerMonthKey, HitterMonthValues> MonthValues,
        Dictionary<PlayerMonthKey, int> InjuryStatuses)
    {
        public record HitterMonthStats(
            int H, int Hit2B, int Hit3B, int HR, int PA,
            int BB, int HBP, int K, int SB, int CS, int AB,
            float ParkHRFactor, float ParkRunFactor);

        public record HitterMonthValues(float CrWAR, float CrOFF, float CrDPOS, float CrDRAA, float CrBSR)
        {
            public static readonly HitterMonthValues Zero = new(0, 0, 0, 0, 0);
        }

        public int InjuryStatus(PlayerMonthKey key) =>
            InjuryStatuses.TryGetValue(key, out int s) ? s : 0;

        public HitterMonthValues Values(PlayerMonthKey key) =>
            MonthValues.TryGetValue(key, out var v) ? v : HitterMonthValues.Zero;

        public static ModelHitterCache Generate()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            return Generate(HitterSourceData.FromDb(db));
        }

        public static ModelHitterCache Generate(HitterSourceData src)
        {
            return new ModelHitterCache(
                SharedCacheLoaders.LoadModelPlayers(src.Model_Players),
                SharedCacheLoaders.LoadPlayerBios(src.Player),
                LoadMonthlyRatios(src.Player_Hitter_MonthlyRatios),
                LoadMonthStats(src.Player_Hitter_MonthStats),
                LoadMonthValues(
                    src.Player_Hitter_MonthAdvanced,
                    src.Player_Fielder_MonthStats,
                    src.Player_MonthlyWar,
                    src.Player_Hitter_MonthBaserunning),
                SharedCacheLoaders.LoadInjuryStatuses(src.Transaction_Log));
        }

        public static Dictionary<int, List<Player_Hitter_MonthlyRatios>> LoadMonthlyRatios(IQueryable<Player_Hitter_MonthlyRatios> ratios)
        {
            return ratios
                .ToList()
                .GroupBy(f => f.MlbId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(f => f.Year).ThenBy(f => f.Month).ToList());
        }

        public static Dictionary<PlayerMonthLeagueKey, HitterMonthStats> LoadMonthStats(IQueryable<Player_Hitter_MonthStats> stats)
        {
            return stats
                .Select(f => new
                {
                    f.MlbId, f.Year, f.Month, f.LeagueId,
                    f.H, f.Hit2B, f.Hit3B, f.HR, f.PA, f.BB, f.HBP, f.K, f.SB, f.CS, f.AB,
                    f.ParkHRFactor, f.ParkRunFactor,
                })
                .ToDictionary(
                    f => new PlayerMonthLeagueKey(f.MlbId, f.Year, f.Month, f.LeagueId),
                    f => new HitterMonthStats(
                        f.H, f.Hit2B, f.Hit3B, f.HR, f.PA, f.BB, f.HBP, f.K, f.SB, f.CS, f.AB,
                        f.ParkHRFactor, f.ParkRunFactor));
        }

        public static Dictionary<PlayerMonthKey, HitterMonthValues> LoadMonthValues(
            IQueryable<Player_Hitter_MonthAdvanced> monthAdvanced,
            IQueryable<Player_Fielder_MonthStats> fielding,
            IQueryable<Player_MonthlyWar> monthlyWar,
            IQueryable<Player_Hitter_MonthBaserunning> baserunning)
        {
            var values = new Dictionary<PlayerMonthKey, HitterMonthValues>();

            HitterMonthValues At(int id, int y, int m) =>
                values.TryGetValue(new PlayerMonthKey(id, y, m), out var v) ? v : HitterMonthValues.Zero;
            void Set(int id, int y, int m, HitterMonthValues v) => values[new PlayerMonthKey(id, y, m)] = v;

            foreach (var r in monthAdvanced
                                 .GroupBy(f => new { f.MlbId, f.Year, f.Month })
                                 .Select(g => new { g.Key.MlbId, g.Key.Year, g.Key.Month, W = g.Sum(x => x.CrWAR), O = g.Sum(x => x.CrOFF) }))
            {
                Set(r.MlbId, r.Year, r.Month, At(r.MlbId, r.Year, r.Month) with { CrWAR = (float)r.W, CrOFF = (float)r.O });
            }

            foreach (var r in fielding
                                 .GroupBy(f => new { f.MlbId, f.Year, f.Month })
                                 .Select(g => new { g.Key.MlbId, g.Key.Year, g.Key.Month, P = g.Sum(x => x.PosAdjust) }))
            {
                Set(r.MlbId, r.Year, r.Month, At(r.MlbId, r.Year, r.Month) with { CrDPOS = (float)r.P });
            }

            // CrDRAA = Sum(ScaledDRAA where LevelId != 1)[MiLB] + Sum(Player_MonthlyWar.DRAA)[only MLB].
            foreach (var r in fielding
                                 .Where(f => f.LevelId != 1)
                                 .GroupBy(f => new { f.MlbId, f.Year, f.Month })
                                 .Select(g => new { g.Key.MlbId, g.Key.Year, g.Key.Month, D = g.Sum(x => x.ScaledDRAA) }))
            {
                var cur = At(r.MlbId, r.Year, r.Month);
                Set(r.MlbId, r.Year, r.Month, cur with { CrDRAA = cur.CrDRAA + (float)r.D });
            }

            foreach (var r in monthlyWar
                                 .GroupBy(f => new { f.MlbId, f.Year, f.Month })
                                 .Select(g => new { g.Key.MlbId, g.Key.Year, g.Key.Month, D = g.Sum(x => x.DRAA) }))
            {
                var cur = At(r.MlbId, r.Year, r.Month);
                Set(r.MlbId, r.Year, r.Month, cur with { CrDRAA = cur.CrDRAA + (float)r.D });
            }

            foreach (var r in baserunning
                                 .GroupBy(f => new { f.MlbId, f.Year, f.Month })
                                 .Select(g => new { g.Key.MlbId, g.Key.Year, g.Key.Month, B = g.Sum(x => x.RBSR) }))
            {
                Set(r.MlbId, r.Year, r.Month, At(r.MlbId, r.Year, r.Month) with { CrBSR = (float)r.B });
            }

            return values;
        }
    }

    public record ModelPitcherCache(
        Dictionary<int, Model_Players> ModelPlayers,
        Dictionary<int, PlayerBio> PlayerBios,
        Dictionary<int, List<Player_Pitcher_MonthlyRatios>> MonthlyRatios,
        Dictionary<PlayerMonthLeagueKey, PitcherMonthStats> MonthStats,
        Dictionary<PlayerMonthKey, PitcherMonthValues> MonthValues,
        Dictionary<PlayerMonthKey, int> InjuryStatuses)
    {
        public record PitcherMonthStats(int BattersFaced, float ParkHRFactor, float ParkRunFactor, float SPPerc);

        public record PitcherMonthValues(float CrWAR)
        {
            public static readonly PitcherMonthValues Zero = new(0);
        }

        public int InjuryStatus(PlayerMonthKey key) =>
            InjuryStatuses.TryGetValue(key, out int s) ? s : 0;

        public PitcherMonthValues Values(PlayerMonthKey key) =>
            MonthValues.TryGetValue(key, out var v) ? v : PitcherMonthValues.Zero;

        public static ModelPitcherCache Generate()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            return Generate(PitcherSourceData.FromDb(db));
        }

        public static ModelPitcherCache Generate(PitcherSourceData src)
        {
            return new ModelPitcherCache(
                SharedCacheLoaders.LoadModelPlayers(src.Model_Players),
                SharedCacheLoaders.LoadPlayerBios(src.Player),
                LoadMonthlyRatios(src.Player_Pitcher_MonthlyRatios),
                LoadMonthStats(src.Player_Pitcher_MonthStats),
                LoadMonthValues(src.Player_Pitcher_MonthAdvanced),
                SharedCacheLoaders.LoadInjuryStatuses(src.Transaction_Log));
        }

        public static Dictionary<int, List<Player_Pitcher_MonthlyRatios>> LoadMonthlyRatios(IQueryable<Player_Pitcher_MonthlyRatios> ratios)
        {
            return ratios
                .ToList()
                .GroupBy(f => f.MlbId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(f => f.Year).ThenBy(f => f.Month).ToList());
        }

        public static Dictionary<PlayerMonthLeagueKey, PitcherMonthStats> LoadMonthStats(IQueryable<Player_Pitcher_MonthStats> stats)
        {
            return stats
                .Select(f => new
                {
                    f.MlbId, f.Year, f.Month, f.LeagueId,
                    f.BattersFaced, f.ParkHRFactor, f.ParkRunFactor, f.SPPerc,
                })
                .ToDictionary(
                    f => new PlayerMonthLeagueKey(f.MlbId, f.Year, f.Month, f.LeagueId),
                    f => new PitcherMonthStats(f.BattersFaced, f.ParkHRFactor, f.ParkRunFactor, f.SPPerc));
        }

        public static Dictionary<PlayerMonthKey, PitcherMonthValues> LoadMonthValues(IQueryable<Player_Pitcher_MonthAdvanced> advanced)
        {
            return advanced
                .GroupBy(f => new { f.MlbId, f.Year, f.Month })
                .Select(g => new { g.Key.MlbId, g.Key.Year, g.Key.Month, W = g.Sum(x => x.CrWAR) })
                .ToDictionary(
                    r => new PlayerMonthKey(r.MlbId, r.Year, r.Month),
                    r => new PitcherMonthValues((float)r.W));
        }
    }

    public static class SharedCacheLoaders
    {
        public static Dictionary<int, Model_Players> LoadModelPlayers(IQueryable<Model_Players> modelPlayers)
        {
            return modelPlayers
                .ToDictionary(f => f.MlbId);
        }

        public static Dictionary<int, PlayerBio> LoadPlayerBios(IQueryable<Player> players)
        {
            return players
                .Where(f => f.SigningYear != null)
                .Select(f => new PlayerBio(f.MlbId, f.BirthYear, f.BirthMonth, f.BirthDate, f.SigningYear!.Value))
                .ToDictionary(f => f.MlbId);
        }

        public static Dictionary<PlayerMonthKey, int> LoadInjuryStatuses(IQueryable<Transaction_Log> transactionLog)
        {
            var rows = transactionLog
                .Where(f => f.ToIL > 0)
                .Select(f => new { f.MlbId, f.Year, f.Month, f.ToIL })
                .Distinct()
                .ToList();

            var result = new Dictionary<PlayerMonthKey, int>();
            foreach (var r in rows)
            {
                var key = new PlayerMonthKey(r.MlbId, r.Year, r.Month);
                result.TryGetValue(key, out int mask);
                result[key] = mask | (1 << (r.ToIL - 1));
            }
            return result;
        }
    }

    internal static class CalculateStatsShared
    {
        // Season months are 4..9; after September roll to April of the next year.
        public static (int Year, int Month) NextMonth(int year, int month) =>
            month >= 9 ? (year + 1, 4) : (year, month + 1);
        public static int ModelLevel(int levelId) => levelId == 1 ? 1 : levelId - 9;
    }

    public static class CalculateHitterStats
    {
        public record HitterModelContext(ModelLeagueCache League, ModelHitterCache Hitters, int EndYear, int EndMonth);
        private record ObservedMonth(Model_HitterStats Stats, int LeagueId, int LevelId);

        public static List<Model_HitterStats> BuildHitterStats(int mlbId, HitterModelContext ctx)
        {
            Model_Players player = ctx.Hitters.ModelPlayers[mlbId];
            PlayerBio bio = ctx.Hitters.PlayerBios[mlbId];

            if (!ctx.Hitters.MonthlyRatios.TryGetValue(mlbId, out var ratios) || ratios.Count == 0)
            {
                return BuildNewSigneeGaps(player, bio, ctx);
            }

            List<ObservedMonth> observed = BuildObservedMonths(player, bio, ratios, ctx);
            List<Model_HitterStats> internalGaps = FillInternalGaps(observed, player, bio, ctx);
            List<Model_HitterStats> trailingGaps = FillTrailingGaps(observed, player, bio, ctx);

            return observed.Select(o => o.Stats)
                .Concat(internalGaps)
                .Concat(trailingGaps)
                .OrderBy(s => s.Year).ThenBy(s => s.Month)
                .ToList();
        }

        private static List<ObservedMonth> BuildObservedMonths(
            Model_Players player, PlayerBio bio, List<Player_Hitter_MonthlyRatios> ratios, HitterModelContext ctx)
        {
            var output = new List<ObservedMonth>();

            // ratios are already ordered by (Year, Month) from the cache
            foreach (var month in ratios.GroupBy(r => (r.Year, r.Month)))
            {
                Model_HitterStats? acc = null;
                int bestPA = -1, bestLeague = -1, bestLevel = -1;

                foreach (Player_Hitter_MonthlyRatios r in month)
                {
                    Model_HitterStats row = BuildObservedRow(player, bio, r, ctx);

                    if (row.PA > bestPA)
                    {
                        bestPA = row.PA;
                        bestLeague = r.LeagueId;
                        bestLevel = CalculateStatsShared.ModelLevel(r.LevelId);
                    }

                    if (acc is null)
                    {
                        acc = row;
                    }
                    else
                    {
                        BlendInto(acc, row);
                    }
                }

                acc!.WRC = Utilities.ClampWRC(acc.WRC);
                output.Add(new ObservedMonth(acc, bestLeague, bestLevel));
            }

            return output;
        }

        private static Model_HitterStats BuildObservedRow(
            Model_Players player, PlayerBio bio, Player_Hitter_MonthlyRatios r, HitterModelContext ctx)
        {
            var leagueKey = new LeagueMonthKey(r.Year, r.Month, r.LeagueId);
            var monthKey = new PlayerMonthKey(r.MlbId, r.Year, r.Month);
            HitterMonthStats stats = ctx.Hitters.MonthStats[new PlayerMonthLeagueKey(r.MlbId, r.Year, r.Month, r.LeagueId)];
            LeagueHitterRates lhr = ctx.League.LeagueHitterRates[leagueKey];
            HitterMonthValues values = ctx.Hitters.Values(monthKey);

            return new Model_HitterStats
            {
                MlbId = r.MlbId,
                Year = r.Year,
                Month = r.Month,
                Age = Utilities.GetAge1MinusAge0(r.Year, r.Month, 15, bio.BirthYear, bio.BirthMonth, bio.BirthDate),
                LeagueAverageAge = ctx.League.HitterAverageAges[leagueKey],
                PA = stats.PA,
                TrainMask = Utilities.GetModelMask(player, r.Year, r.Month),
                InjStatus = ctx.Hitters.InjuryStatus(monthKey),
                MonthFrac = ctx.League.GameFraction(leagueKey),
                ParkHRFactor = stats.ParkHRFactor,
                ParkRunFactor = stats.ParkRunFactor,
                LevelId = CalculateStatsShared.ModelLevel(r.LevelId),
                AVGRatio = r.AVGRatio,
                OBPRatio = r.OBPRatio,
                ISORatio = r.ISORatio,
                WRC = r.WRC,
                CrWAR = values.CrWAR,
                CrOFF = values.CrOFF,
                CrDPOS = values.CrDPOS,
                CrDRAA = values.CrDRAA,
                CrBSR = values.CrBSR,
                SBPercRatio = r.SBPercRatio,
                SBRateRatio = r.SBRateRatio,
                HRPercRatio = r.HRPercRatio,
                BBPercRatio = r.BBPercRatio,
                KPercRatio = r.KPercRatio,
                PercC = r.PercC,
                Perc1B = r.Perc1B,
                Perc2B = r.Perc2B,
                Perc3B = r.Perc3B,
                PercSS = r.PercSS,
                PercLF = r.PercLF,
                PercCF = r.PercCF,
                PercRF = r.PercRF,
                PercDH = r.PercDH,
                Hit1B = Utilities.SafeDivide(stats.H - stats.Hit2B - stats.Hit3B - stats.HR, stats.PA * lhr.Hit1B),
                Hit2B = Utilities.SafeDivide(stats.Hit2B, stats.PA * lhr.Hit2B),
                Hit3B = Utilities.SafeDivide(stats.Hit3B, stats.PA * lhr.Hit3B),
                HitHR = Utilities.SafeDivide(stats.HR, stats.PA * lhr.HitHR),
                BB = Utilities.SafeDivide(stats.BB, stats.PA * lhr.BB),
                HBP = Utilities.SafeDivide(stats.HBP, stats.PA * lhr.HBP),
                K = Utilities.SafeDivide(stats.K, stats.PA * lhr.K),
                SB = Utilities.SafeDivide(stats.SB, stats.PA * lhr.SB),
                CS = Utilities.SafeDivide(stats.CS, stats.PA * lhr.CS),
            };
        }

        // PA-weighted merge of a second league's row into the month's accumulated record.
        private static void BlendInto(Model_HitterStats acc, Model_HitterStats row)
        {
            float prop = acc.PA + row.PA > 0 ? (float)row.PA / (acc.PA + row.PA) : 0.5f;
            float Mix(float current, float incoming) => (incoming * prop) + (current * (1 - prop));

            acc.PA += row.PA;
            acc.LeagueAverageAge = Mix(acc.LeagueAverageAge, row.LeagueAverageAge);
            acc.MonthFrac = Mix(acc.MonthFrac, row.MonthFrac);
            acc.ParkHRFactor = Mix(acc.ParkHRFactor, row.ParkHRFactor);
            acc.ParkRunFactor = Mix(acc.ParkRunFactor, row.ParkRunFactor);
            acc.LevelId = Mix(acc.LevelId, row.LevelId);
            acc.AVGRatio = Mix(acc.AVGRatio, row.AVGRatio);
            acc.OBPRatio = Mix(acc.OBPRatio, row.OBPRatio);
            acc.ISORatio = Mix(acc.ISORatio, row.ISORatio);
            acc.WRC = Mix(acc.WRC, row.WRC);
            acc.SBPercRatio = Mix(acc.SBPercRatio, row.SBPercRatio);
            acc.SBRateRatio = Mix(acc.SBRateRatio, row.SBRateRatio);
            acc.HRPercRatio = Mix(acc.HRPercRatio, row.HRPercRatio);
            acc.BBPercRatio = Mix(acc.BBPercRatio, row.BBPercRatio);
            acc.KPercRatio = Mix(acc.KPercRatio, row.KPercRatio);
            acc.PercC = Mix(acc.PercC, row.PercC);
            acc.Perc1B = Mix(acc.Perc1B, row.Perc1B);
            acc.Perc2B = Mix(acc.Perc2B, row.Perc2B);
            acc.Perc3B = Mix(acc.Perc3B, row.Perc3B);
            acc.PercSS = Mix(acc.PercSS, row.PercSS);
            acc.PercLF = Mix(acc.PercLF, row.PercLF);
            acc.PercCF = Mix(acc.PercCF, row.PercCF);
            acc.PercRF = Mix(acc.PercRF, row.PercRF);
            acc.PercDH = Mix(acc.PercDH, row.PercDH);
            acc.Hit1B = Mix(acc.Hit1B, row.Hit1B);
            acc.Hit2B = Mix(acc.Hit2B, row.Hit2B);
            acc.Hit3B = Mix(acc.Hit3B, row.Hit3B);
            acc.HitHR = Mix(acc.HitHR, row.HitHR);
            acc.BB = Mix(acc.BB, row.BB);
            acc.HBP = Mix(acc.HBP, row.HBP);
            acc.K = Mix(acc.K, row.K);
            acc.SB = Mix(acc.SB, row.SB);
            acc.CS = Mix(acc.CS, row.CS);
        }

        private static List<Model_HitterStats> FillInternalGaps(
            List<ObservedMonth> observed, Model_Players player, PlayerBio bio, HitterModelContext ctx)
        {
            var output = new List<Model_HitterStats>();

            for (int i = 0; i < observed.Count - 1; i++)
            {
                ObservedMonth last = observed[i];
                Model_HitterStats next = observed[i + 1].Stats;

                (int y, int m) = CalculateStatsShared.NextMonth(last.Stats.Year, last.Stats.Month);
                while (y < next.Year || (y == next.Year && m < next.Month))
                {
                    AddGapIfPlayed(output, last, y, m, player, bio, ctx);
                    (y, m) = CalculateStatsShared.NextMonth(y, m);
                }
            }

            return output;
        }

        private static List<Model_HitterStats> FillTrailingGaps(
            List<ObservedMonth> observed, Model_Players player, PlayerBio bio, HitterModelContext ctx)
        {
            var output = new List<Model_HitterStats>();
            ObservedMonth last = observed[^1];
            int lastYear = Math.Min(last.Stats.Year + 2, ctx.EndYear);

            (int endY, int endM) = last.Stats.Year + 2 < ctx.EndYear
                ? (last.Stats.Year + 2, 9)
                : (ctx.EndYear, ctx.EndMonth);

            (int y, int m) = CalculateStatsShared.NextMonth(last.Stats.Year, last.Stats.Month);
            while (y <= lastYear)
            {
                AddGapIfPlayed(output, last, y, m, player, bio, ctx);
                (y, m) = CalculateStatsShared.NextMonth(y, m);
            }

            return output;
        }

        // Adds an empty month in the league the player last played in, if that league had games that month.
        private static void AddGapIfPlayed(
            List<Model_HitterStats> output, ObservedMonth last, int year, int month,
            Model_Players player, PlayerBio bio, HitterModelContext ctx)
        {
            var leagueKey = new LeagueMonthKey(year, month, last.LeagueId);
            if (!ctx.League.HasGames(leagueKey))
            {
                return;
            }

            output.Add(BuildGapMonth(
                player, bio, year, month, last.LeagueId, last.LevelId, ctx));
        }

        // Player signed but has not played yet: empty months in the complex league after signing.
        private static List<Model_HitterStats> BuildNewSigneeGaps(Model_Players player, PlayerBio bio, HitterModelContext ctx)
        {
            const int MISSING_LEAGUE = 124; // Florida Complex League
            const int MISSING_LEVEL = 7;

            var output = new List<Model_HitterStats>();
            int y = bio.SigningYear + 1;
            int m = 4;

            while ((y < ctx.EndYear || (y == ctx.EndYear && m <= ctx.EndMonth)) && y < bio.SigningYear + 7)
            {
                if (ctx.League.HasGames(new LeagueMonthKey(y, m, MISSING_LEAGUE)))
                {
                    // leagueAverageAge = null -> uses the player's own age, matching the original
                    output.Add(BuildGapMonth(player, bio, y, m, MISSING_LEAGUE, MISSING_LEVEL, ctx));
                }
                (y, m) = CalculateStatsShared.NextMonth(y, m);
            }

            return output;
        }

        private static Model_HitterStats BuildGapMonth(
            Model_Players player, PlayerBio bio, int year, int month, int leagueId, int levelId,
            HitterModelContext ctx)
        {
            var leagueKey = new LeagueMonthKey(year, month, leagueId);

            return new Model_HitterStats
            {
                MlbId = player.MlbId,
                Year = year,
                Month = month,
                Age = Utilities.GetAge1MinusAge0(year, month, 15, bio.BirthYear, bio.BirthMonth, bio.BirthDate),
                LeagueAverageAge = ctx.League.HitterAverageAges[leagueKey],
                PA = 0,
                TrainMask = Utilities.GetModelMask(player, year, month),
                InjStatus = ctx.Hitters.InjuryStatus(new PlayerMonthKey(player.MlbId, year, month)),
                MonthFrac = ctx.League.GameFraction(leagueKey),
                LevelId = levelId,
                ParkHRFactor = 1,
                ParkRunFactor = 1,
                AVGRatio = 1,
                OBPRatio = 1,
                ISORatio = 1,
                WRC = 100,
                CrWAR = 0,
                CrBSR = 0,
                CrDPOS = 0,
                CrDRAA = 0,
                CrOFF = 0,
                SBPercRatio = 1,
                SBRateRatio = 1,
                HRPercRatio = 1,
                BBPercRatio = 1,
                KPercRatio = 1,
                PercC = 0,
                Perc1B = 0,
                Perc2B = 0,
                Perc3B = 0,
                PercSS = 0,
                PercLF = 0,
                PercCF = 0,
                PercRF = 0,
                PercDH = 0,
                Hit1B = 1,
                Hit2B = 1,
                Hit3B = 1,
                HitHR = 1,
                BB = 1,
                HBP = 1,
                K = 1,
                SB = 1,
                CS = 1,
            };
        }

        internal static List<Model_HitterStats> BuildAllHitterStats(IEnumerable<int> ids, HitterModelContext ctx, IProgress<float>? progress = null)
        {
            int[] idArray = ids.ToArray();
            var output = new List<Model_HitterStats>(idArray.Length * 70);

            for (int i = 0; i < idArray.Length; i++)
            {
                output.AddRange(BuildHitterStats(idArray[i], ctx));
                progress?.Report((float)(i + 1) / idArray.Length);
            }

            return output;
        }
    }
    
    public static class CalculatePitcherStats
    {
        public record PitcherModelContext(ModelLeagueCache League, ModelPitcherCache Pitchers, int EndYear, int EndMonth);
        private record ObservedMonth(Model_PitcherStats Stats, int LeagueId, int LevelId);

        public static List<Model_PitcherStats> BuildPitcherStats(int mlbId, PitcherModelContext ctx)
        {
            Model_Players player = ctx.Pitchers.ModelPlayers[mlbId];
            PlayerBio bio = ctx.Pitchers.PlayerBios[mlbId];

            if (!ctx.Pitchers.MonthlyRatios.TryGetValue(mlbId, out var ratios) || ratios.Count == 0)
            {
                return FillNeverPlayedMonths(player, bio, ctx);
            }

            List<ObservedMonth> observed = BuildObservedMonths(player, bio, ratios, ctx);
            List<Model_PitcherStats> internalGaps = FillInternalGaps(observed, player, bio, ctx);
            List<Model_PitcherStats> trailingGaps = FillTrailingGaps(observed, player, bio, ctx);

            return observed.Select(o => o.Stats)
                .Concat(internalGaps)
                .Concat(trailingGaps)
                .OrderBy(s => s.Year).ThenBy(s => s.Month)
                .ToList();
        }

        private static List<ObservedMonth> BuildObservedMonths(
            Model_Players player, PlayerBio bio, List<Player_Pitcher_MonthlyRatios> ratios, PitcherModelContext ctx)
        {
            var output = new List<ObservedMonth>();

            // ratios are already ordered by (Year, Month) from the cache
            foreach (var month in ratios.GroupBy(r => (r.Year, r.Month)))
            {
                Model_PitcherStats? acc = null;
                int bestBF = -1, bestLeague = -1, bestLevel = -1;

                foreach (Player_Pitcher_MonthlyRatios r in month)
                {
                    Model_PitcherStats row = BuildObservedRow(player, bio, r, ctx);

                    if (row.BF > bestBF)
                    {
                        bestBF = row.BF;
                        bestLeague = r.LeagueId;
                        bestLevel = CalculateStatsShared.ModelLevel(r.LevelId);
                    }

                    if (acc is null)
                    {
                        acc = row;
                    }
                    else
                    {
                        BlendInto(acc, row);
                    }
                }

                output.Add(new ObservedMonth(acc!, bestLeague, bestLevel));
            }

            return output;
        }

        private static Model_PitcherStats BuildObservedRow(
            Model_Players player, PlayerBio bio, Player_Pitcher_MonthlyRatios r, PitcherModelContext ctx)
        {
            var leagueKey = new LeagueMonthKey(r.Year, r.Month, r.LeagueId);
            var monthKey = new PlayerMonthKey(r.MlbId, r.Year, r.Month);
            PitcherMonthStats stats = ctx.Pitchers.MonthStats[new PlayerMonthLeagueKey(r.MlbId, r.Year, r.Month, r.LeagueId)];
            PitcherMonthValues values = ctx.Pitchers.Values(monthKey);

            return new Model_PitcherStats
            {
                MlbId = r.MlbId,
                Year = r.Year,
                Month = r.Month,
                Age = Utilities.GetAge1MinusAge0(r.Year, r.Month, 15, bio.BirthYear, bio.BirthMonth, bio.BirthDate),
                LeagueAverageAge = ctx.League.PitcherAverageAges[leagueKey],
                BF = stats.BattersFaced,
                TrainMask = Utilities.GetModelMask(player, r.Year, r.Month),
                InjStatus = ctx.Pitchers.InjuryStatus(monthKey),
                MonthFrac = ctx.League.GameFraction(leagueKey),
                ParkHRFactor = stats.ParkHRFactor,
                ParkRunFactor = stats.ParkRunFactor,
                SpPerc = stats.SPPerc,
                LevelId = CalculateStatsShared.ModelLevel(r.LevelId),
                WOBARatio = r.WOBARatio,
                HRPercRatio = r.HRPercRatio,
                BBPercRatio = r.BBPercRatio,
                KPercRatio = r.KPercRatio,
                GBPercRatio = r.GBPercRatio,
                ERARatio = r.ERARatio,
                FIPRatio = r.FIPRatio,
                CrWAR = values.CrWAR,
            };
        }

        private static void BlendInto(Model_PitcherStats acc, Model_PitcherStats row)
        {
            float prop = acc.BF + row.BF > 0 ? (float)row.BF / (acc.BF + row.BF) : 0.5f;
            float Mix(float current, float incoming) => (incoming * prop) + (current * (1 - prop));

            acc.BF += row.BF;
            acc.LeagueAverageAge = Mix(acc.LeagueAverageAge, row.LeagueAverageAge);
            acc.MonthFrac = Mix(acc.MonthFrac, row.MonthFrac);
            acc.ParkHRFactor = Mix(acc.ParkHRFactor, row.ParkHRFactor);
            acc.ParkRunFactor = Mix(acc.ParkRunFactor, row.ParkRunFactor);
            acc.SpPerc = Mix(acc.SpPerc, row.SpPerc);
            acc.LevelId = Mix(acc.LevelId, row.LevelId);
            acc.WOBARatio = Mix(acc.WOBARatio, row.WOBARatio);
            acc.HRPercRatio = Mix(acc.HRPercRatio, row.HRPercRatio);
            acc.BBPercRatio = Mix(acc.BBPercRatio, row.BBPercRatio);
            acc.KPercRatio = Mix(acc.KPercRatio, row.KPercRatio);
            acc.GBPercRatio = Mix(acc.GBPercRatio, row.GBPercRatio);
            acc.ERARatio = Mix(acc.ERARatio, row.ERARatio);
            acc.FIPRatio = Mix(acc.FIPRatio, row.FIPRatio);
        }

        private static List<Model_PitcherStats> FillInternalGaps(
            List<ObservedMonth> observed, Model_Players player, PlayerBio bio, PitcherModelContext ctx)
        {
            var output = new List<Model_PitcherStats>();

            for (int i = 0; i < observed.Count - 1; i++)
            {
                ObservedMonth last = observed[i];
                Model_PitcherStats next = observed[i + 1].Stats;

                (int y, int m) = CalculateStatsShared.NextMonth(last.Stats.Year, last.Stats.Month);
                while (y < next.Year || (y == next.Year && m < next.Month))
                {
                    AddGapIfPlayed(output, last, y, m, player, bio, ctx);
                    (y, m) = CalculateStatsShared.NextMonth(y, m);
                }
            }

            return output;
        }

        private static List<Model_PitcherStats> FillTrailingGaps(
            List<ObservedMonth> observed, Model_Players player, PlayerBio bio, PitcherModelContext ctx)
        {
            var output = new List<Model_PitcherStats>();
            ObservedMonth last = observed[^1];

            (int endY, int endM) = last.Stats.Year + 2 < ctx.EndYear
                ? (last.Stats.Year + 2, 9)
                : (ctx.EndYear, ctx.EndMonth);

            (int y, int m) = CalculateStatsShared.NextMonth(last.Stats.Year, last.Stats.Month);
            while (y < endY || (y == endY && m <= endM))
            {
                AddGapIfPlayed(output, last, y, m, player, bio, ctx);
                (y, m) = CalculateStatsShared.NextMonth(y, m);
            }

            return output;
        }

        private static void AddGapIfPlayed(
            List<Model_PitcherStats> output, ObservedMonth last, int year, int month,
            Model_Players player, PlayerBio bio, PitcherModelContext ctx)
        {
            var leagueKey = new LeagueMonthKey(year, month, last.LeagueId);
            if (!ctx.League.HasGames(leagueKey))
            {
                return;
            }

            output.Add(BuildGapMonth(player, bio, year, month, last.LeagueId, last.LevelId, ctx));
        }

        private static List<Model_PitcherStats> FillNeverPlayedMonths(Model_Players player, PlayerBio bio, PitcherModelContext ctx)
        {
            const int MISSING_LEAGUE = 124; // Florida Complex League
            const int MISSING_LEVEL = 7;

            var output = new List<Model_PitcherStats>();
            int y = bio.SigningYear + 1;
            int m = 4;

            while ((y < ctx.EndYear || (y == ctx.EndYear && m <= ctx.EndMonth)) && y < bio.SigningYear + 7)
            {
                if (ctx.League.HasGames(new LeagueMonthKey(y, m, MISSING_LEAGUE)))
                {
                    output.Add(BuildGapMonth(player, bio, y, m, MISSING_LEAGUE, MISSING_LEVEL, ctx));
                }
                (y, m) = CalculateStatsShared.NextMonth(y, m);
            }

            return output;
        }

        private static Model_PitcherStats BuildGapMonth(
            Model_Players player, PlayerBio bio, int year, int month, int leagueId, int levelId,
            PitcherModelContext ctx)
        {
            var leagueKey = new LeagueMonthKey(year, month, leagueId);

            return new Model_PitcherStats
            {
                MlbId = player.MlbId,
                Year = year,
                Month = month,
                Age = Utilities.GetAge1MinusAge0(year, month, 15, bio.BirthYear, bio.BirthMonth, bio.BirthDate),
                LeagueAverageAge = ctx.League.PitcherAverageAges[leagueKey],
                BF = 0,
                TrainMask = Utilities.GetModelMask(player, year, month),
                InjStatus = ctx.Pitchers.InjuryStatus(new PlayerMonthKey(player.MlbId, year, month)),
                MonthFrac = ctx.League.GameFraction(leagueKey),
                LevelId = levelId,
                ParkHRFactor = 1,
                ParkRunFactor = 1,
                SpPerc = 0.5f,
                WOBARatio = 1,
                HRPercRatio = 1,
                BBPercRatio = 1,
                KPercRatio = 1,
                GBPercRatio = 1,
                ERARatio = 1,
                FIPRatio = 1,
                CrWAR = 0,
            };
        }

        internal static List<Model_PitcherStats> BuildAllPitcherStats(IEnumerable<int> ids, PitcherModelContext ctx, IProgress<float>? progress = null)
        {
            int[] idArray = ids.ToArray();
            var output = new List<Model_PitcherStats>(idArray.Length * 70);

            for (int i = 0; i < idArray.Length; i++)
            {
                output.AddRange(BuildPitcherStats(idArray[i], ctx));
                progress?.Report((float)(i + 1) / idArray.Length);
            }

            return output;
        }
    }

    public static class CalculateModelStats
    {
        public static bool Update(int endYear, int endMonth)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Model_HitterStats.ExecuteDelete();
                db.Model_PitcherStats.ExecuteDelete();

                ModelLeagueCache league = ModelLeagueCache.Generate();
                UpdateHitters(db, league, endYear, endMonth);
                UpdatePitchers(db, league, endYear, endMonth);
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ModelMonthStats");
                Utilities.LogException(e);
                return false;
            }
        }

        private static void UpdateHitters(SqliteDbContext db, ModelLeagueCache league, int endYear, int endMonth)
        {
            var ctx = new CalculateHitterStats.HitterModelContext(league, ModelHitterCache.Generate(), endYear, endMonth);
            int[] ids = db.Model_Players.Where(f => f.IsHitter).Select(f => f.MlbId).ToArray();

            using ProgressBar progressBar = new(ids.Length, "Calculating Model Hitter Stats");
            List<Model_HitterStats> output = CalculateHitterStats.BuildAllHitterStats(ids, ctx, progressBar.AsProgress<float>());
            db.BulkInsert(output);
        }

        private static void UpdatePitchers(SqliteDbContext db, ModelLeagueCache league, int endYear, int endMonth)
        {
            var ctx = new CalculatePitcherStats.PitcherModelContext(league, ModelPitcherCache.Generate(), endYear, endMonth);
            int[] ids = db.Model_Players.Where(f => f.IsPitcher).Select(f => f.MlbId).ToArray();

            using ProgressBar progressBar = new(ids.Length, "Calculating Model Pitcher Stats");
            List<Model_PitcherStats> output = CalculatePitcherStats.BuildAllPitcherStats(ids, ctx, progressBar.AsProgress<float>());
            db.BulkInsert(output);
        }

        public static List<Model_HitterStats> BuildSinglePlayerStats(
            HitterSourceData data, ModelLeagueCache league, int endYear, int endMonth)
        {
            ModelHitterCache hitters = ModelHitterCache.Generate(data);
            var ctx = new HitterModelContext(league, hitters, endYear, endMonth);
            return BuildHitterStats(hitters.ModelPlayers.Keys.Single(), ctx);
        }

        public static List<Model_PitcherStats> BuildSinglePlayerStats(
            PitcherSourceData data, ModelLeagueCache league, int endYear, int endMonth)
        {
            ModelPitcherCache pitchers = ModelPitcherCache.Generate(data);
            var ctx = new PitcherModelContext(league, pitchers, endYear, endMonth);
            return BuildPitcherStats(pitchers.ModelPlayers.Keys.Single(), ctx);
        }
    }
}