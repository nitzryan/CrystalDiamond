using System.Reflection;
using System.Text;

namespace DataAquisition.Temp
{
    using System.Globalization;

    internal static class ModelStatsDiff
    {
        public const double FloatTolerance = 1e-3;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // Only int / float, non-nullable. Anything else is a bug -> Format() throws.
        private static PropertyInfo[] GetColumns<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead &&
                                        (p.PropertyType == typeof(int) || p.PropertyType == typeof(float)))
                            .ToArray();
        }

        private static string Format(object? value)
        {
            switch (value)
            {
                case int i: return i.ToString(Inv);
                case float f: return f.ToString("R", Inv); // round-trip => reparses exactly
                default:
                    throw new NotSupportedException(
                        $"ModelStatsDiff only supports int/float columns, got '{value?.GetType().Name ?? "null"}'.");
            }
        }

        // ---------------------------------------------------------------- write

        public static void WriteCsv<T>(IEnumerable<T> rows, string path)
        {
            PropertyInfo[] columns = GetColumns<T>();
            using StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(string.Join(",", columns.Select(c => c.Name)));

            foreach (T row in rows)
            {
                string[] cells = new string[columns.Length];
                for (int i = 0; i < columns.Length; i++)
                    cells[i] = Format(columns[i].GetValue(row));
                writer.WriteLine(string.Join(",", cells));
            }
        }

        // ---------------------------------------------------------------- read

        private sealed class CsvTable
        {
            public required string[] Headers { get; init; }
            public required Dictionary<string, int> HeaderIndex { get; init; }
            public required List<string[]> Rows { get; init; }
        }

        private static CsvTable ReadCsv(string path)
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0)
                throw new InvalidOperationException($"CSV '{path}' is empty.");

            string[] headers = lines[0].Split(',');
            Dictionary<string, int> headerIndex = new Dictionary<string, int>(headers.Length);
            for (int i = 0; i < headers.Length; i++)
                headerIndex[headers[i]] = i;

            List<string[]> rows = new List<string[]>(lines.Length - 1);
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                    continue;
                rows.Add(lines[i].Split(','));
            }

            return new CsvTable { Headers = headers, HeaderIndex = headerIndex, Rows = rows };
        }

        private static Dictionary<(int MlbId, int Year, int Month), List<string[]>> IndexRows(CsvTable table)
        {
            int mlbIdx = table.HeaderIndex["MlbId"];
            int yearIdx = table.HeaderIndex["Year"];
            int monthIdx = table.HeaderIndex["Month"];

            Dictionary<(int MlbId, int Year, int Month), List<string[]>> map = new();
            foreach (string[] row in table.Rows)
            {
                (int MlbId, int Year, int Month) key = (
                    int.Parse(row[mlbIdx], Inv),
                    int.Parse(row[yearIdx], Inv),
                    int.Parse(row[monthIdx], Inv));

                if (!map.TryGetValue(key, out List<string[]>? bucket))
                {
                    bucket = new List<string[]>(1);
                    map[key] = bucket;
                }
                bucket.Add(row);
            }
            return map;
        }

        // ---------------------------------------------------------------- compare

        private static bool Differs(bool isFloat, string currentText, string newText)
        {
            if (isFloat)
            {
                float a = float.Parse(currentText, Inv);
                float b = float.Parse(newText, Inv);
                if (float.IsNaN(a) && float.IsNaN(b)) return false;
                if (float.IsNaN(a) || float.IsNaN(b)) return true;
                return Math.Abs(a - b) > FloatTolerance;
            }

            int ia = int.Parse(currentText, Inv);
            int ib = int.Parse(newText, Inv);
            return ia != ib;
        }

        // Formats a cell value for the markdown table: floats get trimmed to 3 decimals.
        private static string FormatCell(bool isFloat, string text)
        {
            if (!isFloat)
                return text;
            float f = float.Parse(text, Inv);
            return f.ToString("0.###", Inv);
        }

        public static void Compare<T>(string currentCsvPath, string newCsvPath, string markdownPath, string title)
        {
            PropertyInfo[] columns = GetColumns<T>();
            Dictionary<string, bool> isFloatByColumn = new Dictionary<string, bool>(columns.Length);
            foreach (PropertyInfo column in columns)
                isFloatByColumn[column.Name] = column.PropertyType == typeof(float);

            CsvTable current = ReadCsv(currentCsvPath);
            CsvTable updated = ReadCsv(newCsvPath);

            Dictionary<(int MlbId, int Year, int Month), List<string[]>> currentRows = IndexRows(current);
            Dictionary<(int MlbId, int Year, int Month), List<string[]>> updatedRows = IndexRows(updated);

            // Group all keys by MlbId in a single pass
            Dictionary<int, List<(int MlbId, int Year, int Month)>> keysByMlbId = new();
            foreach ((int MlbId, int Year, int Month) key in currentRows.Keys.Concat(updatedRows.Keys))
            {
                if (!keysByMlbId.TryGetValue(key.MlbId, out List<(int MlbId, int Year, int Month)>? list))
                {
                    list = new List<(int MlbId, int Year, int Month)>();
                    keysByMlbId[key.MlbId] = list;
                }
                list.Add(key);
            }

            List<int> allMlbIds = keysByMlbId.Keys.OrderBy(x => x).ToList();

            StringBuilder body = new StringBuilder();
            Dictionary<string, int> diffCountByColumn = new Dictionary<string, int>();
            Dictionary<string, int> diffCountByCombination = new Dictionary<string, int>();
            int mlbIdsMissingOnly = 0;
            int mlbIdsValueOnly = 0;
            int mlbIdsBoth = 0;

            foreach (int mlbId in allMlbIds)
            {
                List<(int MlbId, int Year, int Month)> keys = keysByMlbId[mlbId]
                    .Distinct()
                    .OrderBy(k => k.Year).ThenBy(k => k.Month)
                    .ToList();

                List<string> missingLines = new List<string>();
                List<string> cellSections = new List<string>();

                foreach ((int MlbId, int Year, int Month) key in keys)
                {
                    currentRows.TryGetValue(key, out List<string[]>? curList);
                    updatedRows.TryGetValue(key, out List<string[]>? newList);
                    int curCount = curList?.Count ?? 0;
                    int newCount = newList?.Count ?? 0;

                    if (curCount == 0 || newCount == 0)
                    {
                        string where = curCount > 0 ? "only in CURRENT" : "only in NEW";
                        missingLines.Add($"- Row (Year {key.Year}, Month {key.Month}) {where}");
                        continue;
                    }
                    if (curCount != newCount)
                    {
                        missingLines.Add(
                            $"- Row (Year {key.Year}, Month {key.Month}) duplicate-count mismatch " +
                            $"(current={curCount}, new={newCount})");
                        continue;
                    }

                    for (int rowIdx = 0; rowIdx < curCount; rowIdx++)
                    {
                        string[] curRow = curList![rowIdx];
                        string[] newRow = newList![rowIdx];
                        List<(string Column, string Current, string New)> rowDiffs = new();

                        foreach (PropertyInfo column in columns)
                        {
                            if (!current.HeaderIndex.TryGetValue(column.Name, out int ci)) continue;
                            if (!updated.HeaderIndex.TryGetValue(column.Name, out int ni)) continue;

                            string cv = curRow[ci];
                            string nv = newRow[ni];
                            bool isFloat = isFloatByColumn[column.Name];
                            if (Differs(isFloat, cv, nv))
                            {

                                rowDiffs.Add((column.Name, FormatCell(isFloat, cv), FormatCell(isFloat, nv)));

                                diffCountByColumn.TryGetValue(column.Name, out int count);
                                diffCountByColumn[column.Name] = count + 1;
                            }
                        }

                        if (rowDiffs.Count == 0)
                            continue;

                        string combo = string.Join(", ", rowDiffs.Select(d => d.Column).OrderBy(c => c));
                        diffCountByCombination.TryGetValue(combo, out int comboCount);
                        diffCountByCombination[combo] = comboCount + 1;

                        StringBuilder section = new StringBuilder();
                        string suffix = curCount > 1 ? $" #[{rowIdx}]" : "";
                        section.AppendLine($"### Year {key.Year}, Month {key.Month}{suffix} <{combo}>");
                        section.AppendLine();

                        section.AppendLine($"| {"Column",-20} | {"Current",-10} | {"New",-10} |");
                        section.AppendLine($"|{new string('-', 22)}|{new string('-', 12)}|{new string('-', 12)}|");
                        foreach ((string Column, string Current, string New) diff in rowDiffs)
                            section.AppendLine($"| {diff.Column,-20} | {diff.Current,-10} | {diff.New,-10} |");
                        section.AppendLine();

                        cellSections.Add(section.ToString());
                    }
                }

                if (missingLines.Count == 0 && cellSections.Count == 0)
                    continue;

                string category;
                if (missingLines.Count > 0 && cellSections.Count > 0) 
                { 
                    mlbIdsBoth++; 
                    category = "Both"; 
                }
                else if (missingLines.Count > 0) 
                { 
                    mlbIdsMissingOnly++; 
                    category = "Row"; 
                }
                else 
                { 
                    mlbIdsValueOnly++; 
                    category = "Value"; 
                }

                body.AppendLine($"## MlbId {mlbId} {category}");
                body.AppendLine();
                if (missingLines.Count > 0)
                {
                    body.AppendLine("### Missing / extra rows");
                    body.AppendLine();
                    foreach (string line in missingLines)
                        body.AppendLine(line);
                    body.AppendLine();
                }
                foreach (string section in cellSections)
                    body.Append(section);
            }

            int mlbIdsWithDiffs = mlbIdsMissingOnly + mlbIdsValueOnly + mlbIdsBoth;

            StringBuilder header = new StringBuilder();
            header.AppendLine($"# {title}");
            header.AppendLine();
            header.AppendLine($"- Float tolerance: {FloatTolerance}");
            header.AppendLine($"- Current rows: {current.Rows.Count}");
            header.AppendLine($"- New rows: {updated.Rows.Count}");
            header.AppendLine($"- MlbIds with differences: {mlbIdsWithDiffs}");
            header.AppendLine($"  - Missing/extra rows only: {mlbIdsMissingOnly}");
            header.AppendLine($"  - Value differences only: {mlbIdsValueOnly}");
            header.AppendLine($"  - Both: {mlbIdsBoth}");
            header.AppendLine();
            if (mlbIdsWithDiffs == 0)
                header.AppendLine("**No differences found.**");
            header.AppendLine();

            if (diffCountByColumn.Count > 0)
            {
                header.AppendLine("## Column difference counts");
                header.AppendLine();
                header.AppendLine($"| {"Column",-20} | {"Count",-6} |");
                header.AppendLine($"|{new string('-', 22)}|{new string('-', 8)}|");
                foreach (KeyValuePair<string, int> kvp in diffCountByColumn.OrderByDescending(k => k.Value))
                    header.AppendLine($"| {kvp.Key,-20} | {kvp.Value,-6} |");
                header.AppendLine();
            }

            if (diffCountByCombination.Count > 0)
            {
                header.AppendLine("## Column combination counts");
                header.AppendLine();
                header.AppendLine($"| {"Columns",-45} | {"Count",-6} |");
                header.AppendLine($"|{new string('-', 47)}|{new string('-', 8)}|");
                foreach (KeyValuePair<string, int> kvp in diffCountByCombination.OrderByDescending(k => k.Value))
                    header.AppendLine($"| {kvp.Key,-45} | {kvp.Value,-6} |");
                header.AppendLine();
            }

            File.WriteAllText(markdownPath, header.ToString() + body.ToString());
        }
    }
}
