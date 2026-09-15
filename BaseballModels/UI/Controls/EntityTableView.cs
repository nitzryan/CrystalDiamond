using System.Data;
using System.Diagnostics;
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
        private bool _reverting;   // guards against re-entrancy when we set cell.Value

        // Handling soft-deleted rows
        private static readonly Color DeletedBackColor = Color.FromArgb(189, 195, 199);
        private static readonly Color DeletedForeColor = Color.FromArgb(80, 80, 80);
        private Font _deletedFont = null!;
        private readonly HashSet<int> _deletedRows = new();
        private const string ToggleColumnName = "__DeleteToggle";
        private int ToggleColumnIndex => _props.Length;
        private const string DeleteText = "-";
        private const string RestoreText = "+";

        private readonly HashSet<(int row, int col)> _editedCells = new();
        private bool _collapsed;
        private const int MaxGridHeight = 400;  // cap before the grid starts scrolling

        private PropertyInfo[] _props = Array.Empty<PropertyInfo>();
        private readonly List<object?[]> _originalValues = new();

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
        }

        /// <summary>
        /// One grid row per DB row, one column per public property.
        /// Auto-hides the control when there are no rows.
        /// </summary>
        public void SetData<T>(string tableName, IEnumerable<T> rows, params string[] lockedColumns)
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
            _collapsed = false;

            _props = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .OrderBy(p => p.MetadataToken)   // source declaration order
                .ToArray();

            // Get columns that should be made readonly in table
            var locked = new HashSet<string>(lockedColumns, StringComparer.Ordinal);
            Debug.Assert(locked.All(n => _props.Any(p => p.Name == n)),
                "lockedColumns contains a name that is not a property of " + typeof(T).Name);

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
                bool isLocked = locked.Contains(p.Name);
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
                    values[i] = _props[i].GetValue(item);

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

        /// <summary>
        /// Rebuilds a list of T from the current (possibly edited) grid contents.
        /// T must match the type passed to SetData and have a parameterless constructor.
        /// </summary>
        public List<T> GetData<T>()
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
                _reverting = true;
                try { cell.Value = _lastValidValues[e.RowIndex][e.ColumnIndex]; }
                finally { _reverting = false; }
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

        private void ApplySizing()
        {
            grid.Visible = !_collapsed;
            btnToggle.Text = _collapsed ? "Show ▾" : "Hide ▴";

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
    }
}
