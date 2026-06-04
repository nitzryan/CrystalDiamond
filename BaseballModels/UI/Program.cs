using Python.Runtime;

namespace UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            
            // Must set PYTHONNET_PYDLL environmental variable to Python DLL
            PythonEngine.Initialize();
            PySetup.Initialize();

            ApplicationConfiguration.Initialize();
            Application.Run(new PitchViewer());

            PythonEngine.Shutdown();
        }
    }
}