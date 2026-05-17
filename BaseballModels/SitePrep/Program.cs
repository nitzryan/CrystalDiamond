using Db;

namespace SitePrep
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception($"Wrong number of arguments: 2 expected (year, month), {args.Length} provided");

            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            int year = db.Model_HitterStats.Select(f => f.Year).Max();
            int month = db.Model_HitterStats.Where(f => f.Year == year).Select(f => f.Month).Max();

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
