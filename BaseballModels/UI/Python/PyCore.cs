using Python.Runtime;

namespace UI.Python
{
    internal class PyCore
    {
        private static bool INITIALIZED = false;
        private static IntPtr _threadState;
        private static readonly PyCore _instance = new PyCore();
        public static event Action<string>? PythonMessage;

        // The parent that contains both python projects; the ONLY sys.path entry.
        public static readonly string PythonRoot =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../"));
        
        public static readonly string PitchModelDir = Path.Combine(PythonRoot, "PitchModel");
        public static readonly string ModelDir = Path.Combine(PythonRoot, "Model");
        public static readonly string PitchModelingModelDir = Path.Combine(PitchModelDir, "Models");

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

        public static void Initialize()
        {
            if (INITIALIZED)
                return;

            PythonEngine.Initialize();
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                if (!((object[])sys.path).Contains(PythonRoot))
                    sys.path.append(PythonRoot);

                sys.stdout = _instance;
                sys.stderr = _instance;
            }
            _threadState = PythonEngine.BeginAllowThreads();
            INITIALIZED = true;
        }

        // Must run on the PyThread (the thread that called Initialize).
        public static void Shutdown()
        {
            if (!INITIALIZED)
                return;

            PythonEngine.EndAllowThreads(_threadState);
            PythonEngine.Shutdown();
            INITIALIZED = false;
        }
    }
}
