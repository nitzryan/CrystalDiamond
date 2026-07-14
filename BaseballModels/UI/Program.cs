using Python.Runtime;

namespace UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            PyThread.Start();
            ApplicationConfiguration.Initialize();
            Application.Run(new HomePage());
            PyThread.Stop();
        }
    }
}