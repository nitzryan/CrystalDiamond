using Python.Runtime;

namespace UI.Python
{
    internal static class PitchPy
    {
        private static dynamic? _constants, _dbTypes, _pitchDbTypes, _pitchModel;
        private static PyObject? _dataPrep;

        public static dynamic Constants => _constants ?? throw NotLoaded("Constants");
        public static dynamic DbTypes => _dbTypes ?? throw NotLoaded("DBTypes");
        public static dynamic PitchDbTypes => _pitchDbTypes ?? throw NotLoaded("PitchDBTypes");
        public static dynamic PitchModel => _pitchModel ?? throw NotLoaded("PitchModel");
        public static PyObject DataPrep => _dataPrep ?? throw NotLoaded("DataPrep");

        public static bool IsReady { get; private set; } = false;
        public static event EventHandler? Ready;
        private static Task? _load;   // fire-once guard

        // Call from the UI thread so Ready is raised there.
        public static async void LoadPythonResources()
        {
            if (_load != null)
                return;   // fire-once

            _load = PyThread.InvokeAsync(() =>
            {
                // Runs on the Python thread with the GIL held.
                _constants = Py.Import("PitchModel.Constants");
                _dbTypes = Py.Import("PitchModel.DBTypes");
                _pitchDbTypes = Py.Import("PitchModel.PitchDBTypes");
                _pitchModel = Py.Import("PitchModel.Stuff.Model.PitchModel");

                dynamic dataPrepModule = Py.Import("PitchModel.Stuff.DataPrep.DataPrep");
                _dataPrep = dataPrepModule.DataPrep.Load_From_File(
                    Path.Combine(PyCore.PitchModelDir, (string)Constants.DATA_PREP_BINARY_ALL_FILE));
            });

            await _load;   // no catch: failures crash the app
                            // TODO: Handle this more elegantly
            IsReady = true;
            Ready?.Invoke(null, EventArgs.Empty);
        }

        private static InvalidOperationException NotLoaded(string obj) => new InvalidOperationException(
            "PitchPy.LoadPythonResources() has not finished. " +
            $"Error with {obj}. " + 
            "Gate on PitchPy.IsReady / PitchPy.Ready before touching this."); 
    }
}
