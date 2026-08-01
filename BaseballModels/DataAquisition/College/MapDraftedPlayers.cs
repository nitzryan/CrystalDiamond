using Db;

namespace DataAquisition.College
{
    internal class MapDraftedPlayers
    {
        public static void Update(int year)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var playersDrafted = db.Player.Where(f => f.SigningYear == year && f.DraftPick != null)
                .OrderBy(f => f.DraftPick);

            foreach (var player in playersDrafted)
            {
                // Check for college player with same birthdate and last name
                College_Player? cp = db.College_Player
                    .Where(f => f.MlbId <= 0 &&
                        f.BirthYear == player.BirthYear &&
                        f.BirthMonth == player.BirthMonth &&
                        f.BirthDay == player.BirthDate &&
                        f.LastName.ToUpper().Equals(player.UseLastName.ToUpper()) &&
                        f.FirstName.Substring(0, 1).ToUpper() == player.UseFirstName.Substring(0, 1).ToUpper())
                    .SingleOrDefault();

                if (cp == null) // Either HS player or already assigned
                    continue;

                cp.MlbId = player.MlbId;
            }

            db.SaveChanges();
        }
    }
}
