namespace SitePrep
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!ModelAggregation.Main())
                return;
        
            if (!HitterPage.Main())
                return;

            if (OrgMap.Main())
                return;
        }
    }
}
