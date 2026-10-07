using Db;
using SitePrep.Draft;

namespace SitePrep
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
            SetTimestepQuality.Create();
            WriteWarBucketAverages.Update();
            TeamDraftPlayerGen.Calculate();

            Draft.CalculateDraftPickValue.Update(Draft.DraftPickTarget.Actual);
            Draft.CalculateDraftPickValue.Update(Draft.DraftPickTarget.Model);

            Draft.PickExpectedValueCalibration.CreateTimeGraph(Draft.DraftPickTarget.Actual);
            Draft.PickExpectedValueCalibration.CreateTimeGraph(Draft.DraftPickTarget.Model);

            Draft.TeamDraftOverviews.RegenerateTeamDraftOverviews();
            Draft.TeamDraftOverviews.CheckValueCalibration();
            Draft.TeamDraftOverviews.CheckPickBinCalibration();
            Draft.PickExpectedValueCalibration.CreateGraph();

            MoveDbToServer.Update();
        }
    }
}
