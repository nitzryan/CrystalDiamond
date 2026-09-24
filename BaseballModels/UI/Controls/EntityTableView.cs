using Db;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace UI.Controls
{
    public partial class EntityTableView : UserControl
    {
        // Stand-out highlight for edited cells
        private static readonly Color EditedBackColor = Color.FromArgb(231, 76, 60);  // red
        private static readonly Color EditedForeColor = Color.White;
        private Font _editedFont = null!;

        private Type? _elementType = null;

        // Tracks last valid values to be used to revert if an invalid value is set
        private readonly List<object?[]> _lastValidValues = new();

        // Handling soft-deleted rows
        private static readonly Color DeletedBackColor = Color.FromArgb(189, 195, 199);
        private static readonly Color DeletedForeColor = Color.FromArgb(80, 80, 80);
        private Font _deletedFont = null!;
        private readonly HashSet<int> _deletedRows = new();
        private const string ToggleColumnName = "__DeleteToggle";
        private int ToggleColumnIndex => _props.Length;
        private const string DeleteText = "-";
        private const string RestoreText = "+";

        // Takes an id column and only lets the user select valid entries
        private static readonly Dictionary<string, IReadOnlyDictionary<int, string>> ColumnMaps = new(StringComparer.Ordinal);

        // A registered rule: if a grid has all Columns, each row's values (joined in
        // column order) must appear in ValidKeys
        private sealed record CombinationRule(string Name, string[] Columns, HashSet<string> ValidKeys);
        private static readonly List<CombinationRule> GlobalCombinationRules = [];

        private readonly HashSet<(int row, int col)> _editedCells = new();
        private bool _collapsed;
        private const int MaxGridHeight = 200;  // cap before the grid starts scrolling

        private PropertyInfo[] _props = Array.Empty<PropertyInfo>();
        private readonly List<object?[]> _originalValues = new();

        // Used to map between MLB and AL-NL for leagueId
        private const string LeagueIdColumn = "LeagueId";
        private const int HybridMlbLeagueId = 1;
        private const int AmericanLeagueId = 103;
        private const int NationalLeagueId = 104;
        public bool CombineMlbLeagues { get; set; } = false;

        private static string[] LockedColumns = [
            nameof(Player_Hitter_MonthStats.MlbId),
            nameof(Player_Hitter_MonthStats.Year),
            nameof(Player_Hitter_MonthStats.Month)
        ];

        public EntityTableView()
        {
            InitializeComponent();
            _editedFont = new Font(grid.Font, FontStyle.Bold);

            // Paint the grid in one shot instead of row-by-row when shown
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(grid, true);

            grid.DataError += Grid_DataError;
            grid.MultiSelect = false;

            _deletedFont = new Font(grid.Font, FontStyle.Strikeout);
            grid.CellContentClick += Grid_CellContentClick;

            grid.Leave += Grid_Leave;

            // Get columns that should be made readonly in table
            string[] lockedColumns = [

            ];
            var locked = new HashSet<string>(lockedColumns, StringComparer.Ordinal);
        }

        /// <summary>
        /// One grid row per DB row, one column per public property.
        /// Auto-hides the control when there are no rows.
        /// </summary>
        public void SetData<T>(string tableName, IEnumerable<T> rows)
        {
            _elementType = typeof(T);
            lblTitle.Text = tableName;

            var list = rows.ToList();
            if (list.Count == 0)
            {
                Visible = false;
                return;
            }
            Visible = true;
            _collapsed = true;

            _props = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .OrderBy(p => p.MetadataToken)   // source declaration order
                .ToArray();

            grid.Rows.Clear();
            grid.Columns.Clear();
            _originalValues.Clear();
            foreach (var p in _props)
            {
                Type t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                // Set cell contents based on type
                DataGridViewColumn col;
                if (t == typeof(bool))
                {
                    col = new DataGridViewCheckBoxColumn
                    {
                        ThreeState = Nullable.GetUnderlyingType(p.PropertyType) != null
                    };
                }
                else if (TryGetColumnMap(p.Name, out IReadOnlyDictionary<int, string>? map))
                {
                    Debug.Assert(t == typeof(int), $"Dropdown map on non-int column '{p.Name}'");
                    col = new DataGridViewComboBoxColumn
                    {
                        DataSource = map.OrderBy(kv => kv.Key).ToList(),
                        DisplayMember = "Value",   // KeyValuePair.Value = display text
                        ValueMember = "Key",       // KeyValuePair.Key = stored int
                        ValueType = t,
                        FlatStyle = FlatStyle.Flat
                    };
                }
                else if (t.IsEnum)
                {
                    col = new DataGridViewComboBoxColumn
                    {
                        DataSource = Enum.GetValues(t),
                        ValueType = t,
                        FlatStyle = FlatStyle.Flat
                    };
                }
                else
                {
                    col = new DataGridViewTextBoxColumn { ValueType = p.PropertyType };
                }

                // Set readonly values
                bool isLocked = LockedColumns.Contains(p.Name);
                col.ReadOnly = !p.CanWrite || isLocked;
                if (col.ReadOnly)
                    col.DefaultCellStyle.ForeColor = SystemColors.GrayText;

                // Fill in cell
                col.Name = p.Name;
                col.HeaderText = p.Name;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                grid.Columns.Add(col);
            }

            // Delete/Restore button column
            var toggleCol = new DataGridViewButtonColumn
            {
                Name = ToggleColumnName,
                HeaderText = "",
                UseColumnTextForButtonValue = false,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true
            };
            grid.Columns.Add(toggleCol);
            toggleCol.DisplayIndex = 0;   // index stays last, but it's shown first


            _editedCells.Clear();
            _deletedRows.Clear();
            _lastValidValues.Clear();
            foreach (var item in list)
            {
                var values = new object?[_props.Length];
                for (int i = 0; i < _props.Length; i++)
                {
                    values[i] = _props[i].GetValue(item);
                    if (TryGetColumnMap(_props[i].Name, out var map)
                        && (values[i] is not int v || !map.ContainsKey(v)))
                    {
                        throw new InvalidOperationException(
                            $"{tableName}.{_props[i].Name}: value '{values[i] ?? "null"}' has no dropdown mapping.");
                    }
                }

                int rowIdx = grid.Rows.Add(values);
                grid.Rows[rowIdx].Cells[ToggleColumnIndex].Value = DeleteText;

                _originalValues.Add(values);
                _lastValidValues.Add((object?[])values.Clone());
            }

            // Size columns
            grid.DefaultCellStyle.Font = _editedFont;
            grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            grid.DefaultCellStyle.Font = null;
            grid.CurrentCell = null;

            RefreshAllCellStyles();
            ApplySizing();
        }

        // Dropdown map for a column, adjusted for the MLB combine toggle
        private bool TryGetColumnMap(string column, [NotNullWhen(true)] out IReadOnlyDictionary<int, string>? map)
        {
            // Check for non-dropdown
            if (!ColumnMaps.TryGetValue(column, out IReadOnlyDictionary<int, string>? baseMap))
            {
                map = null;
                return false;
            }
            // League Dropdown
            if (CombineMlbLeagues && column == LeagueIdColumn)
            {
                var combined = baseMap
                    .Where(kv => kv.Key != AmericanLeagueId && kv.Key != NationalLeagueId)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                combined[HybridMlbLeagueId] = "MLB";
                map = combined;
                return true;
            }

            // Other dropdown
            map = baseMap;
            return true;
        }

        // Exact key match, or (when combining) league 1 standing in for AL or NL
        private bool IsCombinationValid(CombinationRule rule, int[] values)
        {
            if (rule.ValidKeys.Contains(string.Join("|", values)))
                return true;

            if (!CombineMlbLeagues)
                return false;

            int leagueIdx = Array.IndexOf(rule.Columns, LeagueIdColumn);
            if (leagueIdx < 0 || values[leagueIdx] != HybridMlbLeagueId)
                return false;

            foreach (int realLeague in new[] { AmericanLeagueId, NationalLeagueId })
            {
                int[] substituted = (int[])values.Clone();
                substituted[leagueIdx] = realLeague;
                if (rule.ValidKeys.Contains(string.Join("|", substituted)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// GetData, but first checks all registered combination rules.
        /// Shows a popup and returns false (with an empty list) on the first invalid row.
        /// </summary>
        public bool TryGetData<T>(out List<T> data)
        {
            string? error = FindInvalidCombination();
            if (error != null)
            {
                data = [];
                MessageBox.Show(error, $"Invalid data: {lblTitle.Text}");
                return false;
            }
            data = GetData<T>();
            return true;
        }

        /// <summary>
        /// Rebuilds a list of T from the current (possibly edited) grid contents.
        /// T must match the type passed to SetData and have a parameterless constructor.
        /// </summary>
        private List<T> GetData<T>()
        {
            Debug.Assert(_elementType != null, "GetData called before SetData.");
            Debug.Assert(typeof(T) == _elementType,
                $"GetData<{typeof(T).Name}> called but grid holds {_elementType?.Name}.");
            if (_elementType != typeof(T))
                throw new InvalidOperationException(
                    $"Grid holds '{_elementType?.Name ?? "nothing"}', not '{typeof(T).Name}'.");

            var result = new List<T>(grid.Rows.Count);
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow || _deletedRows.Contains(row.Index))
                    continue;

                // Throws if T has no parameterless ctor
                T item = Activator.CreateInstance<T>();

                for (int i = 0; i < _props.Length; i++)
                {
                    PropertyInfo p = _props[i];
                    if (!p.CanWrite)
                        continue;   // read-only column; nothing to write back

                    object? value = ConvertCellValue(row.Cells[i].Value, p.PropertyType, p.Name);
                    p.SetValue(item, value);
                }
                result.Add(item);
            }
            return result;
        }

        private static object? ConvertCellValue(object? cellValue, Type targetType, string columnName)
        {
            Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Empty cell / indeterminate checkbox
            if (cellValue == null || cellValue == DBNull.Value ||
                (cellValue is string s && s.Length == 0 && underlying != typeof(string)))
            {
                bool nullAllowed = !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
                Debug.Assert(nullAllowed, $"Column '{columnName}': empty value for non-nullable {targetType.Name}.");
                return nullAllowed ? null : Activator.CreateInstance(targetType); // default(T) fallback
            }

            // Already the right type (checkbox bools, combo enum values, unedited cells)
            if (underlying.IsInstanceOfType(cellValue))
                return cellValue;

            // Text edits come back as strings
            if (underlying.IsEnum)
                return cellValue is string es
                    ? Enum.Parse(underlying, es, ignoreCase: true)
                    : Enum.ToObject(underlying, cellValue);

            return Convert.ChangeType(cellValue, underlying, CultureInfo.InvariantCulture);
        }

        // Uses the dropdown display name when the column has one; raw value kept for debugging
        private string FormatColumnForExternalUse(string column, int value)
        {
            if (TryGetColumnMap(column, out var map) && map.TryGetValue(value, out string? display))
                return $"\t{column}={display} ({value})\n";
            return $"\t{column}={value}\n";
        }

        private void Grid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            grid.CancelEdit();
            e.Cancel = false;
        }

        private void Grid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            // Commit immediately so the "edited" highlight updates as soon as the cell changes
            if (grid.IsCurrentCellDirty)
                grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private bool IsValidForColumn(int colIndex, object? value)
        {
            if (value != null && value != DBNull.Value)
                return true;
            Type t = _props[colIndex].PropertyType;
            // null/DBNull is fine only for reference types and Nullable<T>
            return !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
        }

        private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.ColumnIndex >= _props.Length) 
                return;

            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (!IsValidForColumn(e.ColumnIndex, cell.Value))
            {
                // Blanked a non-nullable cell: put back the last value that was valid.
                try { cell.Value = _lastValidValues[e.RowIndex][e.ColumnIndex]; }
                catch {}
            }
            else
            {
                _lastValidValues[e.RowIndex][e.ColumnIndex] = cell.Value;
            }

            RefreshCellStyle(e.RowIndex, e.ColumnIndex);
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            // Deleted Rows
            foreach (int r in _deletedRows.ToList())
                SetRowDeleted(r, false);

            // Changed values
            for (int r = 0; r < grid.Rows.Count && r < _originalValues.Count; r++)
                for (int c = 0; c < _props.Length; c++)
                {
                    grid.Rows[r].Cells[c].Value = _originalValues[r][c];
                    _lastValidValues[r][c] = _originalValues[r][c];
                }

            RefreshAllCellStyles();
        }

        private void RefreshAllCellStyles()
        {
            for (int r = 0; r < grid.Rows.Count; r++)
                for (int c = 0; c < _props.Length; c++)
                    RefreshCellStyle(r, c);
        }

        private void RefreshCellStyle(int rowIndex, int colIndex)
        {
            if (rowIndex >= _originalValues.Count)
                return;

            object? original = _originalValues[rowIndex][colIndex];
            var cell = grid.Rows[rowIndex].Cells[colIndex];
            bool edited = !ValuesEqual(original, cell.Value);

            if (edited)
                _editedCells.Add((rowIndex, colIndex));
            else
                _editedCells.Remove((rowIndex, colIndex));

            bool deleted = _deletedRows.Contains(rowIndex);

            if (deleted)
            {
                cell.Style.BackColor = DeletedBackColor;
                cell.Style.ForeColor = DeletedForeColor;
                cell.Style.Font = _deletedFont;
            }
            else if (edited)
            {
                cell.Style.BackColor = EditedBackColor;
                cell.Style.ForeColor = EditedForeColor;
                cell.Style.Font = _editedFont;
            }
            else
            {
                cell.Style.BackColor = Color.Empty;
                cell.Style.ForeColor = Color.Empty;
                cell.Style.Font = null;
            }

            btnReset.Enabled = _editedCells.Count > 0 || _deletedRows.Count > 0;
        }

        private static bool ValuesEqual(object? a, object? b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;

            return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
        }

        private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != ToggleColumnIndex)
                return;

            SetRowDeleted(e.RowIndex, !_deletedRows.Contains(e.RowIndex));
        }

        private void SetRowDeleted(int rowIndex, bool deleted)
        {
            if (deleted) 
                _deletedRows.Add(rowIndex);
            else 
                _deletedRows.Remove(rowIndex);

            DataGridViewRow row = grid.Rows[rowIndex];
            row.ReadOnly = deleted;
            row.Cells[ToggleColumnIndex].Value = deleted ? RestoreText : DeleteText;
            
            for (int c = 0; c < _props.Length; c++)
                RefreshCellStyle(rowIndex, c);
        }

        private void BtnToggle_Click(object? sender, EventArgs e)
        {
            _collapsed = !_collapsed;
            ApplySizing();
        }

        public void ApplySizing()
        {
            grid.Visible = !_collapsed;
            btnToggle.Text = _collapsed ? "Show ▾" : "Hide ▴";

            // Set Height
            int height = pnlHeader.Height;
            if (grid.Visible)
            {
                int content = grid.ColumnHeadersHeight + 2;  // borders
                foreach (DataGridViewRow row in grid.Rows)
                    content += row.Height;

                // Account for a horizontal scrollbar if the columns overflow
                int totalColWidth = grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible);
                if (totalColWidth > grid.ClientSize.Width)
                    content += SystemInformation.HorizontalScrollBarHeight;

                height += Math.Min(content, MaxGridHeight);
            }

            Height = height;

            // Shrink the width if the table doesn't need the entirety of what it was set to
            int preferredWidth = 3; // Borders
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Visible)
                    preferredWidth += col.Width;
            }
            if (grid.Controls.OfType<VScrollBar>().Any(s => s.Visible))
                preferredWidth += SystemInformation.VerticalScrollBarWidth;
            Width = preferredWidth;
        }

        private void Grid_Leave(object? sender, EventArgs e)
        {
            // Changing CurrentCell directly inside Enter/Leave can throw a reentrancy
            // exception ("SetCurrentCellAddressCore"), so defer it one message.
            BeginInvoke(() =>
            {
                if (grid.IsDisposed)
                    return;

                grid.EndEdit();
                grid.CurrentCell = null;
                grid.ClearSelection();
            });
        }

        /// <summary>
        /// Renders an int column as a dropdown of display names instead of raw values.
        /// Rows containing a value not in the map cause SetData to throw.
        /// </summary>
        public static void RegisterGlobalDropdown(string columnName, IReadOnlyDictionary<int, string> valueToDisplay)
        {
            ColumnMaps[columnName] = valueToDisplay;
        }

        /// <summary>
        /// Registers a validity rule applied to every EntityTableView whose type has all
        /// of the given columns: each row's values for those columns (in this order) must
        /// match one of the provided combinations. Register at startup.
        /// </summary>
        public static void RegisterGlobalCombinationCheck(string name, string[] columns, IEnumerable<int[]> validCombinations)
        {
            var keys = validCombinations.Select(c => string.Join("|", c)).ToHashSet();
            GlobalCombinationRules.Add(new CombinationRule(name, columns, keys));
        }

        // Returns a description of the first invalid row/rule pairing, or null if all rows pass
        private string? FindInvalidCombination()
        {
            foreach (CombinationRule rule in GlobalCombinationRules)
            {
                // Rule only applies if this type has every column it covers
                int[] colIndexes = rule.Columns
                    .Select(name => Array.FindIndex(_props, p => p.Name == name))
                    .ToArray();

                // Check to see if any column is not found
                if (colIndexes.Any(i => i < 0))
                    continue; // Missing column, rule doesn't apply

                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow || _deletedRows.Contains(row.Index))
                        continue;
                    int[] values = colIndexes
                        .Select(c => (int)ConvertCellValue(row.Cells[c].Value, typeof(int), grid.Columns[c].Name)!)
                        .ToArray();
                    //if (!rule.ValidKeys.Contains(string.Join("|", values)))
                    //    return $"{rule.Name} (row {row.Index + 1}): " +
                    //        string.Join("", rule.Columns.Zip(values, FormatColumnForExternalUse));
                    if (!IsCombinationValid(rule, values))
                        return $"{rule.Name} (row {row.Index + 1}): " +
                            string.Join("", rule.Columns.Zip(values, FormatColumnForExternalUse));
                }
            }

            return null;
        }
    }
}
