using Db;
using ModelDb;
using Python.Runtime;
using System.Reflection;
using UI.Types;

namespace UI.Python
{
    internal class TestRunnerPy
    {
        private static dynamic? _testRunnerModule, _testModelRunner;

        public static dynamic TestRunnerModule => _testRunnerModule ?? throw NotLoaded("Test_Model_Runner module");
        public static dynamic TestModelRunner => _testModelRunner ?? throw NotLoaded("Test_Model_Runner instance");

        public static bool IsReady { get; private set; } = false;
        public static event EventHandler? Ready;

        private static Task? _load;   // fire-once guard

        private const int MODEL_IDX = 1;

        // Call from the UI thread so Ready is raised there.
        public static async void LoadPythonResources()
        {
            if (_load != null)
                return;   // fire-once

            _load = PyThread.InvokeAsync(() =>
            {
                // Runs on the Python thread with the GIL held.
                dynamic constants = Py.Import("Model.Constants");
                dynamic dataPrepModule = Py.Import("Model.Combined.DataPrep.Data_Prep");
                _testRunnerModule = Py.Import("Model.Combined.TrainEval.Test_Model_Runner");

                string dataPrepFile = Path.GetFullPath(
                    Path.Combine(PyCore.ModelDir, (string)constants.GetDataPrepBinaryFile(MODEL_IDX)));
                dynamic dataPrep = dataPrepModule.Combined_Data_Prep.Load_From_File(dataPrepFile);

                // device left at its Python default
                _testModelRunner = _testRunnerModule.Test_Model_Runner(dataPrep, PyCore.ProspectModelsDir);
            });

            await _load;   // no catch: failures crash the app (matches PitchPy)
            IsReady = true;
            Ready?.Invoke(null, EventArgs.Empty);
        }

        private static InvalidOperationException NotLoaded(string obj) => new InvalidOperationException(
            "TestRunnerPy.LoadPythonResources() has not finished. " +
            $"Error with {obj}. " +
            "Gate on TestRunnerPy.IsReady / TestRunnerPy.Ready before touching this.");

        public static Task<List<ModelResults>> RunHitterVariants(
            int mlbId, int? tbcId, int modelId,
            Model_Players dbPlayer, Model_Players tablePlayer,
            List<Model_HitterStats> dbStats, List<Model_HitterStats> tableStats)
        {
            // Snapshot on the calling (UI) thread
            object?[][][] statRows =
            [
                dbStats.Select(s => ScalarValues(s)).ToArray(),
                tableStats.Select(s => ScalarValues(s)).ToArray()
            ];
            return RunVariants(mlbId, tbcId, modelId, isHitter: true, dbPlayer, tablePlayer, statRows);
        }
        public static Task<List<ModelResults>> RunPitcherVariants(
            int mlbId, int? tbcId, int modelId,
            Model_Players dbPlayer, Model_Players tablePlayer,
            List<Model_PitcherStats> dbStats, List<Model_PitcherStats> tableStats)
        {
            // Snapshot on the calling (UI) thread
            object?[][][] statRows =
            [
                dbStats.Select(s => ScalarValues(s)).ToArray(),
                tableStats.Select(s => ScalarValues(s)).ToArray()
            ];
            return RunVariants(mlbId, tbcId, modelId, isHitter: false, dbPlayer, tablePlayer, statRows);
        }

        public static Task<List<ModelResults>> RunVariants(
            int mlbId, int? tbcId, int modelId, bool isHitter,
            Model_Players dbPlayer, Model_Players tablePlayer,
            object?[][][] statRows)
        {
            // Snapshot on the calling (UI) thread
            object?[][] playerRows = [ScalarValues(dbPlayer), ScalarValues(tablePlayer)];

            return PyThread.InvokeAsync(() =>
            {
                // Runs on the Python thread with the GIL held.
                PyObject playerType = TestRunnerModule.DB_Model_Players;
                PyObject statsType = isHitter ?
                    TestRunnerModule.DB_Model_HitterStats :
                    TestRunnerModule.DB_Model_PitcherStats;

                // pro_player : list[DB_Model_Players]
                var proPlayer = new PyList();
                foreach (object?[] row in playerRows)
                    proPlayer.Append(ToPyRow(playerType, row));

                // pro_stats : list[list[DB_Model_HitterStats | DB_Model_PitcherStats]] (all one or the other)
                var proStats = new PyList();
                foreach (object?[][] variant in statRows)
                {
                    var variantList = new PyList();
                    foreach (object?[] row in variant)
                        variantList.Append(ToPyRow(statsType, row));
                    proStats.Append(variantList);
                }

                var kwargs = new PyDict();
                kwargs["pro_player"] = proPlayer;
                kwargs["pro_stats"] = proStats;

                PyObject overridesType = isHitter
                   ? TestRunnerModule.HitterOverrides
                   : TestRunnerModule.PitcherOverrides;
                PyObject overrides = overridesType.Invoke(new PyTuple(), kwargs);

                PyObject results = TestModelRunner.Run_Variants(
                    mlbId, ToPyValue(tbcId), modelId, isHitter, overrides);
                return ToModelResults(results, isHitter);
            });
        }

        // Column values in declaration order; navigation properties are skipped
        private static class ScalarProperties<T>
        {
            public static readonly PropertyInfo[] Properties = typeof(T).GetProperties()
                .Where(p =>
                {
                    Type t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    return p.CanRead && p.CanWrite
                        && (t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal));
                })
                .ToArray();
        }

        private static object?[] ScalarValues<T>(T row)
        {
            return ScalarProperties<T>.Properties
                .Select(p => p.GetValue(row))
                .ToArray();
        }

        private static PyObject ToPyRow(PyObject pyType, object?[] values)
        {
            var tuple = new PyTuple(values.Select(ToPyValue).ToArray());
            return pyType.Invoke(new PyObject[] { tuple });
        }

        private static PyObject ToPyValue(object? value) => value switch
        {
            null => PyObject.None,
            bool b => new PyInt(b ? 1 : 0),
            int i => new PyInt(i),
            long l => new PyInt(l),
            short s => new PyInt(s),
            byte by => new PyInt(by),
            float f => new PyFloat(f),
            double d => new PyFloat(d),
            decimal m => new PyFloat((double)m),
            string str => new PyString(str),
            Enum en => new PyInt(Convert.ToInt64(en)),
            _ => throw new NotSupportedException($"No Python conversion for {value.GetType().Name}")
        };

        private static List<ModelResults> ToModelResults(PyObject pyResults, bool isHitter)
        {
            var results = new List<ModelResults>();
            long count = pyResults.Length();
            for (int i = 0; i < count; i++)
            {
                PyObject r = pyResults[i];

                // Prospect mask
                PyObject proIo = r.GetAttr("combined_io").GetAttr("pro_io");
                if (proIo.IsNone())
                    return [];
                double[] prospectMask = ToDoubleArray(proIo.GetAttr("prospect_mask").InvokeMethod("tolist"));

                PyObject colOutput = r.GetAttr("col_output");
                PyObject proStats = r.GetAttr("pro_stats");

                List<Output_PlayerWarAggregation> proWar = FilterByMask(
                FromPyRows<Output_PlayerWarAggregation>(r.GetAttr("pro_war")) ?? [], prospectMask);

                results.Add(isHitter
                    ? new ModelResults
                    {
                        ColHitOutput = FromPyRows<Output_College_HitterAggregation>(colOutput),
                        ProWar = proWar,
                        ProHitStats = FromPyRows2D<Output_HitterStatsAggregation>(proStats)
                    }
                    : new ModelResults
                    {
                        ColPitOutput = FromPyRows<Output_College_PitcherAggregation>(colOutput),
                        ProWar = proWar,
                        ProPitStats = FromPyRows2D<Output_PitcherStatsAggregation>(proStats)
                    });
            }
            return results;
        }

        // list[DB_T] | None -> List<T>?
        private static List<T>? FromPyRows<T>(PyObject pyList)
        {
            if (pyList.IsNone())
                return null;

            long count = pyList.Length();
            var rows = new List<T>((int)count);
            for (int i = 0; i < count; i++)
                rows.Add(FromPyRow<T>(pyList[i]));
            return rows;
        }

        // list[list[DB_T]] | None -> List<List<T>>?
        private static List<List<T>>? FromPyRows2D<T>(PyObject pyList)
        {
            if (pyList.IsNone())
                return null;

            long count = pyList.Length();
            var rows = new List<List<T>>((int)count);
            for (int i = 0; i < count; i++)
                rows.Add(FromPyRows<T>(pyList[i]) ?? []);
            return rows;
        }

        private static T FromPyRow<T>(PyObject pyRow)
        {
            PropertyInfo[] props = ScalarProperties<T>.Properties;
            PyObject tuple = pyRow.InvokeMethod("To_Tuple");

            long length = tuple.Length();
            if (length != props.Length)
                throw new InvalidOperationException(
                    $"{typeof(T).Name}: Python To_Tuple() returned {length} values, C# has {props.Length} properties");

            T row = (T)Activator.CreateInstance(typeof(T))!;
            for (int i = 0; i < props.Length; i++)
                props[i].SetValue(row, FromPyValue(tuple[i], props[i].PropertyType));
            return row;
        }

        private static object? FromPyValue(PyObject value, Type type)
        {
            Type? underlying = Nullable.GetUnderlyingType(type);
            if (value.IsNone())
            {
                if (underlying != null || !type.IsValueType)
                    return null;
                throw new InvalidCastException($"Python None cannot be assigned to non-nullable {type.Name}");
            }

            Type t = underlying ?? type;
            if (t.IsEnum)
                return Enum.ToObject(t, value.As<long>());
            if (t == typeof(bool))
                return value.IsTrue();
            return value.AsManagedObject(t);
        }

        // Python list of numbers (e.g. tensor.tolist()) -> double[]
        private static double[] ToDoubleArray(PyObject pyList)
        {
            long count = pyList.Length();
            var values = new double[count];
            for (int i = 0; i < count; i++)
                values[i] = pyList[i].As<double>();
            return values;
        }

        // Keep rows[i] where mask[i] > 0; rows and mask must be parallel
        private static List<T> FilterByMask<T>(List<T> rows, double[] mask)
        {
            if (rows.Count != mask.Length)
                throw new InvalidOperationException(
                    $"{typeof(T).Name}: {rows.Count} rows but prospect_mask has {mask.Length} entries");

            var kept = new List<T>();
            for (int i = 0; i < rows.Count; i++)
                if (mask[i] > 0)
                    kept.Add(rows[i]);
            return kept;
        }
    }
}
