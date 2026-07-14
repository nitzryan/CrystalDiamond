

using Python.Runtime;

namespace UI.Python
{
    internal static class ModelPy
    {
        // TODO: redesign - these belong on the python side (data prep / dataset), not baked in here.
        private const int PRO_INPUT_SIZE = 0;       // <-- set to the real pro input size
        private const int COLLEGE_INPUT_SIZE = 0;   // <-- was train_dataset.GetColInputSize()

        private static dynamic? _constants, _dbTypes;
        private static PyObject? _dataPrep;
        private static PyObject? _proHitterModel, _proPitcherModel;
        private static PyObject? _collegeHitterModel, _collegePitcherModel;

        public static dynamic Constants => _constants ?? throw NotLoaded("Constants");
        public static dynamic DbTypes => _dbTypes ?? throw NotLoaded("DBTypes");
        public static PyObject DataPrep => _dataPrep ?? throw NotLoaded("Combined_Data_Prep");
        public static PyObject ProHitterModel => _proHitterModel ?? throw NotLoaded("Pro_Model (hitter)");
        public static PyObject ProPitcherModel => _proPitcherModel ?? throw NotLoaded("Pro_Model (pitcher)");
        public static PyObject CollegeHitterModel => _collegeHitterModel ?? throw NotLoaded("College_Model (hitter)");
        public static PyObject CollegePitcherModel => _collegePitcherModel ?? throw NotLoaded("College_Model (pitcher)");

        public static bool IsReady { get; private set; } = false;
        public static event EventHandler? Ready;
        private static Task? _load;   // fire-once guard

        private static InvalidOperationException NotLoaded(string obj) => new InvalidOperationException(
            "ModelPy.LoadPythonResources() has not finished. " +
            $"Error with {obj}. " +
            "Gate on ModelPy.IsReady / ModelPy.Ready before touching this.");

        public static PyObject ProModel(bool isHitter) => isHitter ? ProHitterModel : ProPitcherModel;
        public static PyObject CollegeModel(bool isHitter) => isHitter ? CollegeHitterModel : CollegePitcherModel;

        // Call from the UI thread so Ready is raised there.
        public static async void LoadPythonResources()
        {
            if (_load != null)
                return;   // fire-once

            _load = PyThread.InvokeAsync(() =>
            {
                // Runs on the Python thread with the GIL held.
                _constants = Py.Import("Model.Constants");
                _dbTypes = Py.Import("Model.DBTypes");

                dynamic dataPrepModule = Py.Import("Model.Combined.DataPrep.Data_Prep");
                _dataPrep = dataPrepModule.Combined_Data_Prep.Load_From_File(
                    Path.Combine(PyCore.ModelDir, (string)Constants.DATA_PREP_BINARY_ALL_FILE));

                PyObject proDataPrep = DataPrep.GetAttr("pro_data_prep");
                PyObject collegeDataPrep = DataPrep.GetAttr("college_data_prep");

                PyObject proType = Py.Import("Model.Pro.Model.Player_Model").GetAttr("RNN_Model");
                PyObject collegeType = Py.Import("Model.College.Model.College_Model").GetAttr("RNN_Model");

                _proHitterModel = BuildProModel(proType, proDataPrep, isHitter: true);
                _proPitcherModel = BuildProModel(proType, proDataPrep, isHitter: false);

                // College models take their output geometry from the matching pro model.
                _collegeHitterModel = BuildCollegeModel(collegeType, collegeDataPrep, _proHitterModel, isHitter: true);
                _collegePitcherModel = BuildCollegeModel(collegeType, collegeDataPrep, _proPitcherModel, isHitter: false);
            });

            await _load;   // no catch: a failure here propagates
            IsReady = true;
            Ready?.Invoke(null, EventArgs.Empty);
        }

        // Runs on the Python thread with the GIL held.
        private static PyObject BuildProModel(PyObject proType, PyObject proDataPrep, bool isHitter)
        {
            dynamic torch = Py.Import("torch");   // cached in sys.modules after the first import
            PyObject mutators = torch.empty(0);

            return proType.Invoke(
                Array.Empty<PyObject>(),
                Py.kw(
                    "input_size", PRO_INPUT_SIZE,
                    "mutators", mutators,
                    "data_prep", proDataPrep,
                    "is_hitter", isHitter
                ));
        }

        // Runs on the Python thread with the GIL held.
        private static PyObject BuildCollegeModel(
            PyObject collegeType, PyObject collegeDataPrep, PyObject proModel, bool isHitter)
        {
            return collegeType.Invoke(
                Array.Empty<PyObject>(),
                Py.kw(
                    "input_size", COLLEGE_INPUT_SIZE,
                    "data_prep", collegeDataPrep,
                    "is_hitter", isHitter,
                    "output_hidden_size", proModel.InvokeMethod("GetHiddenSize"),
                    "output_num_layers", proModel.InvokeMethod("GetNumLayers")
                ));
        }
    }
}
