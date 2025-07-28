using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAquisition
{
    internal class Constants
    {
        public const int START_YEAR = 2005;
        public static ReadOnlyCollection<int> SPORT_IDS = new([1, 11, 12, 13, 14, 15, 16, 17]);
        public static ReadOnlyCollection<string> SPORT_ID_NAMES = new(["MLB", "AAA", "AA", "A+", "A", "A-", "Rk", "DSL"]);
        public const int MEXICAN_LEAGUE_ID = 125;
        public const int DSL_LEAGUE_ID = 130;
    }
}
