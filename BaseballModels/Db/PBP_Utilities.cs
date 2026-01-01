using static Db.DbEnums;

namespace Db
{
    public static class PBP_Utilities
    {
        // Use these 2 values so that at <=min, it doesn't apply any weight and linearly scales up to 1 at the full value
        private const int MIN_OCCURANCES = 10;
        private const int FULL_VALUE_OCCURANCE_POINT = 100;

        public static IEnumerable<GamePlayByPlay> GetDoublePlayOpportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f => f.StartBaseOccupancy.HasFlag(BaseOccupancy.B1) && f.StartOuts <= 1 && f.HitterId != -1);
        }

        private static float GetBaserunningRunsAdded(GamePlayByPlay pbp, BaserunningDict dict, int targetBase, int? finalBase)
        {
            if (finalBase == null)
                return 0;
        
            BaserunningScenario? scenario = PBP_TypeConversions.GetBaserunningScenario(pbp);
            if (scenario == null)
                return 0;

            BaserunningResult result = dict[scenario];
            float numOccurancesProportion = Math.Min((float)(result.NumOccurences - MIN_OCCURANCES) / (FULL_VALUE_OCCURANCE_POINT - MIN_OCCURANCES), 1);
            if (numOccurancesProportion <= 0)
                return 0;

            if (finalBase == 0)
                return numOccurancesProportion * result.RunsOut;
            else if (finalBase >= targetBase)
                return numOccurancesProportion * result.RunsAdvance;
            else
                return numOccurancesProportion * result.RunsStay;
        }

        public static float GetScenarioRunsScored(IEnumerable<GamePlayByPlay> pbp, 
                                                    BaserunningDict dict, Func<IEnumerable<GamePlayByPlay>, IEnumerable<GamePlayByPlay>> filterFunction,
                                                    int startBase, int targetBase)
        {
            return filterFunction(pbp)
                .Sum(f => GetBaserunningRunsAdded(f, dict, targetBase, startBase switch
                {
                    1 => f.Run1stOutcome,
                    2 => f.Run2ndOutcome,
                    3 => f.Run3rdOutcome,
                    _ => throw new Exception($"Unexpected startBase in GetScenarioRunsScored: {startBase}")
                }));
        }

        public static IEnumerable<GamePlayByPlay> GetAdvance_1stTo3rdOnSingle_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B1) &&
                f.Result.HasFlag(PBP_Events.SINGLE) &&
                (!f.StartBaseOccupancy.HasFlag(BaseOccupancy.B2) || f.Run2ndOutcome == 4) &&
                f.HitZone >= 7);
        }

        public static IEnumerable<GamePlayByPlay> GetAdvance_2ndToHomeOnSingle_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B2) &&
                f.Result.HasFlag(PBP_Events.SINGLE) &&
                f.HitZone >= 7);
        }

        public static IEnumerable<GamePlayByPlay> GetAdvance_1stToHomeOnDouble_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B1) &&
                f.Result.HasFlag(PBP_Events.DOUBLE));
        }

        public static IEnumerable<GamePlayByPlay> Avoid_1stToSecondForceout_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B1) &&
                ((f.Result & (PBP_Events.GNDOUT | PBP_Events.FIELDERS_CHOICE | PBP_Events.FIELDERS_CHOICE_OUT | PBP_Events.GIDP)) != 0) &&
                (f.Run2ndOutcome != 0 && f.Run3rdOutcome != 0) && // No other leading runner got out
                f.StartOuts != 2);
        }

        public static IEnumerable<GamePlayByPlay> GetAdvance_2ndTo3rdOnGroundout_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B2) &&
                !f.StartBaseOccupancy.HasFlag(BaseOccupancy.B1) && // Nobody on 1st, so runner isn't forced
                !(f.StartBaseOccupancy.HasFlag(BaseOccupancy.B3) && f.Run3rdOutcome == 4) && // Leading runner did not advance or got thrown out
                f.StartOuts < 2 &&
                f.Result.HasFlag(PBP_Events.GNDOUT)
                );
        }

        public static IEnumerable<GamePlayByPlay> GetAdvance_1stTo2ndOnFlyout_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B1) &&
                (!f.StartBaseOccupancy.HasFlag(BaseOccupancy.B2) || (f.Run2ndOutcome == 3 || f.Run2ndOutcome == 4)) && // Nobody on 2nd, or runner on 2nd advanced
                f.StartOuts < 2 &&
                (f.Result.HasFlag(PBP_Events.FLYOUT) || f.Result.HasFlag(PBP_Events.SAC_FLY) || f.Result.HasFlag(PBP_Events.FB_DOUBLE_PLAY) || f.Result.HasFlag(PBP_Events.LINEOUT) || f.Result.HasFlag(PBP_Events.POPOUT))&&
                f.HitZone >= 7
            );
        }

        public static IEnumerable<GamePlayByPlay> GetAdvance_2ndTo3rdOnFlyout_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B2) &&
                (!f.StartBaseOccupancy.HasFlag(BaseOccupancy.B3) || f.Run3rdOutcome == 4) && // Nobody on 3rd, or runner on 3rd scored
                f.StartOuts < 2 &&
                (f.Result.HasFlag(PBP_Events.FLYOUT) || f.Result.HasFlag(PBP_Events.SAC_FLY) || f.Result.HasFlag(PBP_Events.FB_DOUBLE_PLAY) || f.Result.HasFlag(PBP_Events.LINEOUT) || f.Result.HasFlag(PBP_Events.POPOUT)) &&
                f.HitZone >= 7
            );
        }

        public static IEnumerable<GamePlayByPlay> GetAdvance_3rdToHomeOnFlyout_Opportunities(IEnumerable<GamePlayByPlay> pbp)
        {
            return pbp.Where(f =>
                f.StartBaseOccupancy.HasFlag(BaseOccupancy.B3) &&
                f.StartOuts < 2 &&
                (f.Result.HasFlag(PBP_Events.FLYOUT) || f.Result.HasFlag(PBP_Events.SAC_FLY) || f.Result.HasFlag(PBP_Events.FB_DOUBLE_PLAY) || f.Result.HasFlag(PBP_Events.LINEOUT) || f.Result.HasFlag(PBP_Events.POPOUT)) &&
                f.HitZone >= 7
            );
        }
    }
}
