namespace SwingDecisions
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for (int year = 2017; year <= 2026; year++)
            {
                //CreateSwingDecisions.Update(year, true);
                CreateSwingResultAggregation.Update(year, null, true);
            }
        }
    }
}
