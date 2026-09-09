using Python.Runtime;

namespace UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Global.SetupDatabases();
            PyThread.Start();
            ApplicationConfiguration.Initialize();
            Application.Run(new HomePage());
            PyThread.Stop();
        }
    }
}