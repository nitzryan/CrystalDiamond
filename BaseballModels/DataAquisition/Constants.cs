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
        public const int MEXICAN_LEAGUE_ID = 125;
        public const int DSL_LEAGUE_ID = 130;
    }
}
