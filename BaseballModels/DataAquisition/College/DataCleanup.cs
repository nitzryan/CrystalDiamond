using Db;
using ShellProgressBar;

namespace DataAquisition.College
{
    internal class DataCleanup
    {
        public static void Cleanup()
        {
            FixBadData();
            FixDraftedMissingMLBIds();
            HandleTwoWayDraftedPlayers();
        }

        private static void FixBadData()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            // Josh Morgan has wrong draft data
            db.College_Player.Where(f => f.TBCId == 38027).Single().DraftOvrHitter = 605;

            // The wrong Tyler Cox was listed as drafted 1044
            var tylerCox = db.College_Player.Where(f => f.TBCId == 55597).Single();
            tylerCox.DraftOvrPitcher = 0;
            tylerCox.DraftOvrHitter = 0;
            db.College_Player.Where(f => f.TBCId == 147199).Single().DraftOvrPitcher = 1044;

            // Some teams have 0 for strikeouts
            List<(int, int, int)> InvalidStrikeoutList = new(){
                // 2010 Rutgers
                (2010, 2144, 35),
                (2010, 22205, 25),
                (2010, 22996, 34),
                (2010, 129152, 37),
                (2010, 143001, 3),
                (2010, 144235, 33),
                (2010, 144236, 14),
                (2010, 144240, 42),
                (2010, 144241, 30),
                (2010, 151910, 5),
                (2010, 151927, 1),
                (2010, 153082, 35),
                (2010, 153091, 4),
                (2010, 158206, 5),
                (2010, 158225, 23),
                (2010, 158762, 10),
                (2010, 158922, 22),
                // 2002-2003 Belmont
                (2002, 26327, 32),
                (2002, 26351, 31),
                (2002, 27812, 17),
                (2002, 28127, 0),
                (2002, 31207, 1),
                (2002, 31804, 21),
                (2002, 32173, 8),
                (2002, 32920, 3),
                (2002, 33399, 20),
                (2002, 33419, 0),
                (2002, 33784, 27),
                (2002, 35266, 19),
                (2002, 35636, 21),
                (2002, 36388, 39),
                (2002, 36430, 21),
                (2002, 36680, 11),
                (2002, 161640, 25),

                (2003, 26351, 7),
                (2003, 27812, 26),
                (2003, 29021, 7),
                (2003, 32173, 8),
                (2003, 32920, 22),
                (2003, 33399, 39),
                (2003, 33419, 11),
                (2003, 33784, 35),
                (2003, 35266, 18),
                (2003, 35636, 35),
                (2003, 36388, 15),
                (2003, 36430, 17),
                (2003, 49464, 2),
                (2003, 161640, 26),
                // 2002 Binghamton
                (2002, 39527, 43),
                (2002, 177712, 35),
                // 2009 Utah Valley
                (2009, 150254, 24),
                (2009, 148805, 13),
                (2009, 22311, 25),
                (2009, 149594, 13),
                (2009, 149988, 6),
                (2009, 146631, 9),
                // Anthony Forgione
                (2005, 128894, 9),
                (2007, 128894, 10),
                (2008, 128894, 26),
                // 2002 Lafayette
                (2002, 39644, 16),
                (2002, 38525, 17),
                // Santiago Ruis, couldn't find so guess
                (2009, 164514, 14),
                (2010, 164514, 15),
            };
            foreach ((int year, int id, int k) in InvalidStrikeoutList)
            {
                //Console.WriteLine($"{year}-{id}");
                db.College_HitterStats.Where(f => f.Year == year && f.TBCId == id).Single().K = k;
            }

            List<(int, int, int)> InvalidWalkList = new()
            {
                // 2002 Wofford
                (2002, 233819, 36),
                (2002, 38696, 8),
                (2002, 40846, 10),
                (2002, 49769, 3),
                (2002, 233820, 4),
                (2002, 233821, 2),

                // 2002 Binghamton
                (2002, 177712, 7),
            };
            foreach ((int year, int id, int bb) in InvalidWalkList)
            {
                //Console.WriteLine($"{year}-{id}");
                var HS = db.College_HitterStats.Where(f => f.Year == year && f.TBCId == id).Single();
                HS.BB = bb;
                HS.PA += bb;
            }

            db.SaveChanges();
        }

        // Look up drafted player  to get the MLBId for some players
        private static bool FixDraftedMissingMLBIds()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var missingPlayers = db.College_Player.Where(f => f.MlbId == 0 && (f.DraftOvrHitter > 0 || f.DraftOvrPitcher > 0));

            foreach (var p in missingPlayers)
            {
                var dbPlayer = db.Player.Where(f => (f.DraftPick == p.DraftOvrHitter || f.DraftPick == p.DraftOvrPitcher) && f.SigningYear == p.LastYear);

                if (dbPlayer.Any())
                {
                    Player player = dbPlayer.Single();

                    // Make sure that last names match (a player might have been drafted but not signed
                    if (p.LastName.Replace(" ", "").ToLower() != player.UseLastName.Replace(" ", "").ToLower() &&
                        (p.LastName + " Jr.").ToLower() != player.UseLastName.ToLower())
                    {
                        throw new Exception($"Last Names did not match: TBC {p.LastName} vs MLB {player.UseLastName}");
                    }

                    p.MlbId = player.MlbId;
                }
            }

            db.SaveChanges();
            return true;
        }

        // Assign drafted players to either only be drafted as a hitter or pitcher, except in case of TWP
        // This way, the model can handle that a bad hitter could be drafted high as a pitcher and visa versa
        private static bool HandleTwoWayDraftedPlayers()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var twoWayDraftedPlayers = db.College_Player.Where(f => f.IsHitter && f.IsPitcher && f.DraftOvrHitter > 0)
                .Join(db.Player, cp => cp.MlbId, p => p.MlbId, (cp, p) => new { cp, p });

            using (ProgressBar progressBar = new(twoWayDraftedPlayers.Count(), $"Assigning Two-Way drafted players to hitter or pitcher"))
            {
                foreach (var p in twoWayDraftedPlayers)
                {
                    Player player = p.p;
                    College_Player collegePlayer = p.cp;

                    if (player.Position == "H")
                        collegePlayer.DraftOvrPitcher = 0;
                    else if (player.Position == "P")
                        collegePlayer.DraftOvrHitter = 0;

                    progressBar.Tick();
                }
            }

            db.SaveChanges();
            return true;
        }


    }
}
