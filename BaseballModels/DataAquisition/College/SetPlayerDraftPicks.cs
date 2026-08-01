using Db;

namespace DataAquisition.College
{
    internal class SetPlayerDraftPicks
    {
        public static void Update(int year)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var playersDrafted = db.Player.Where(f => f.SigningYear == year && f.DraftPick != null);

            foreach (var player in playersDrafted)
            {
                College_Player? cp = db.College_Player
                    .Where(f => f.MlbId == player.MlbId)
                    .SingleOrDefault();

                if (cp == null) // Don't have college data
                    continue;

                #pragma warning disable CS8629 // Was filtered out in playersDrafted query.
                if (player.Position == "H" || player.Position == "TWP")
                    cp.DraftOvrHitter = player.DraftPick.Value;

                if (player.Position == "P" || player.Position == "TWP")
                    cp.DraftOvrPitcher = player.DraftPick.Value;
                #pragma warning restore CS8629
            }

            db.SaveChanges();
        }
    }
}
