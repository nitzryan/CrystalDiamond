using SiteDb;

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

            if (!ModelAggregation.Main())
                return;

            if (!GenerateRankings.Main(year, month))
                return;

            if (!GenerateTeamRank.Main())
                return;

            if (!HitterPage.Main())
                return;

            if (!PitcherPage.Main())
                return;

            if (!OrgMap.Main())
                return;

            if (!SearchIndex.Main())
                return;

            if (!Homepage.Main())
                return;

            if (!MoveDbToServer.Main())
                return;
        }
    }
}
