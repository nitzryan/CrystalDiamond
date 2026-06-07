using Python.Runtime;

namespace UI
{
    internal class PySetup
    {
        public static dynamic Py_DbTypes, Py_Constants, Py_PitchDbTypes;
        private static bool INITIALIZED = false;

        public static event Action<string> PythonMessage;
        private static readonly PySetup _instance = new PySetup();

        public static void Initialize()
        {
            if (INITIALIZED)
                return;

            PythonEngine.Initialize();
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                // Get Base Pitch Model Location
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string stuffDir = Path.GetFullPath(Path.Combine(baseDir, "../../../../", "PitchModel"));
                if (!((object[])sys.path).Contains(stuffDir))  // avoid duplicates
                {
                    sys.path.append(stuffDir);
                }

                Py_DbTypes = Py.Import("DBTypes");
                Py_Constants = Py.Import("Constants");
                Py_PitchDbTypes = Py.Import("PitchDBTypes");

                // Redirect output
                sys.stdout = _instance;
                sys.stderr = _instance;
            }

            INITIALIZED = true;
        }

        // Python will call when written to stdout
        public void write(string message)
        {
            PythonMessage?.Invoke(message);
        }

        public static void WriteException(PythonException pyEx)
        {
            _instance.write(pyEx.Message);
            _instance.write(pyEx.StackTrace);
        }

        // Required by Python
        public void flush()
        {

        }
    }
}
