using Db;
using PitchDb;
using Python.Runtime;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UI
{
    public class Global
    {
        public record YearLeagueDevKey(int modelId, int year, int balls, int strikes);
        #pragma warning disable CA2211 // Set once at initialization
        public static Dictionary<YearLeagueDevKey, YearLeagueDeviations> YldDict = new();
        #pragma warning restore CA2211

        public static SqliteDbContext db = new(Db.Connection.DB_READONLY_OPTIONS);
        public static PitchDbContext pitchDb = new(PitchDb.Connection.PITCHDB_READONLY_OPTIONS);

        public static Color GetValueColor(float value, float min, float max, float neutral)
        {
            // Adjust so that it is 0 centered
            value -= neutral;
            min -= neutral;
            max -= neutral;

            float clampedValue = Math.Clamp(value, min, max);

            float h, s, v;

            if (clampedValue <= 0)
            {
                // Blue -> Light Gray
                float t = clampedValue / min;
                h = 240f;
                s = t;
                v = 0.85f + (t * 0.15f);
            }
            else
            {
                // Light Gray -> Red
                float t = clampedValue / max;
                h = 0f;
                s = t;
                v = 0.85f + (t * 0.15f);
            }

            return Global.HsvToColor(h, s, v);
        }

        public static Color HsvToColor(float h, float s, float v, byte alpha = 255)
        {
            h = h % 360;
            if (h < 0) h += 360;

            float c = v * s;
            float x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
            float m = v - c;

            float r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromArgb(alpha,
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255));
        }

        public static dynamic CreateFromCSharp<T>(T csharpObj, dynamic pythonClass)
        {
            if (csharpObj == null) throw new ArgumentNullException(nameof(csharpObj));

            using (Py.GIL())
            {
                var type = typeof(T);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .OrderBy(p => p.MetadataToken)   // preserves source declaration order
                                     .ToList();

                var pyValues = new List<PyObject>(properties.Count);
                foreach (var prop in properties)
                {
                    object? value = prop.GetValue(csharpObj);
                    pyValues.Add(value.ToPython());
                }

                var tuple = new PyTuple(pyValues.ToArray());

                return pythonClass(tuple);
            }
        }

        public static T CreateFromPython<T>(PyObject pyInstance)
        {
            if (pyInstance == null || pyInstance.IsNone())
                throw new ArgumentNullException(nameof(pyInstance));

            using (Py.GIL())
            {
                var type = typeof(T);

                // Create object without calling constructor
                T csharpObj = (T)RuntimeHelpers.GetUninitializedObject(type);

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .OrderBy(p => p.MetadataToken)   // preserves source declaration order
                                     .ToList();

                foreach (var prop in properties)
                {
                    if (!prop.CanWrite) continue;

                    PyObject? pyValue = null;

                    // Python object may or may not start with capital letter
                    if (pyInstance.HasAttr(prop.Name))
                        pyValue = pyInstance.GetAttr(prop.Name);
                    else 
                    {
                        string lowerName = char.ToLower(prop.Name[0]) + prop.Name[1..];
                        if (pyInstance.HasAttr(lowerName))
                        {
                            pyValue = pyInstance.GetAttr(lowerName);
                        }
                        else
                        {
                            throw new Exception("Unable to find property name " + prop.Name);
                        }
                    }
                        
                    object? value = pyValue.AsManagedObject(prop.PropertyType);

                    prop.SetValue(csharpObj, value);
                }

                return csharpObj;
            }
        }
    }
}
