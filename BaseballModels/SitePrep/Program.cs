namespace SitePrep
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //if (!ModelAggregation.Main())
            //    return;

            //if (!HitterPage.Main())
            //    return;

            //if (!PitcherPage.Main())
            //    return;

            //if (OrgMap.Main())
            //    return;

            //if (!SearchIndex.Main())
            //    return;

            if (!GenerateRankings.Main(2024, 9))
                return;
        }
    }
}
