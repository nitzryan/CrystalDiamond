namespace SitePrep
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception($"Wrong number of arguments: 2 expected (year, month), {args.Length} provided");

            int year = Convert.ToInt32(args[0]);
            int month = Convert.ToInt32(args[1]);

            ModelAggregation.Update();
            GeneratePlayerPositions.Update();
            GeneratePredictions.Update();
            GenerateRankings.Update(year, month);
            GenerateTeamRank.Update();
            DraftRankings.Update();
            HitterPage.Update();
            PitcherPage.Update();
            OrgMap.Update();
            SearchIndex.Update();
            Homepage.Update();
            MoveDbToServer.Update();
        }
    }
}
