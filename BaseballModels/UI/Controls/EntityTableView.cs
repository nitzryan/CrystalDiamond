using System.Data;
using System.Reflection;

namespace UI.Controls
{
    public partial class EntityTableView : UserControl
    {
        // Stand-out highlight for edited cells
        private static readonly Color EditedBackColor = Color.FromArgb(231, 76, 60);  // red
        private static readonly Color EditedForeColor = Color.White;
        private Font _editedFont = null!;

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
        }

        /// <summary>
        /// One grid row per DB row, one column per public property.
        /// Auto-hides the control when there are no rows.
        /// </summary>
        public void SetData<T>(string tableName, IEnumerable<T> rows)
        {
            lblTitle.Text = tableName;

            var list = rows?.ToList() ?? new List<T>();
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
                    col = new DataGridViewTextBoxColumn();
                }

                // Fill in cell
                col.Name = p.Name;
                col.HeaderText = p.Name;
                col.ReadOnly = !p.CanWrite;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                grid.Columns.Add(col);
            }

            _editedCells.Clear();
            foreach (var item in list)
            {
                var values = new object?[_props.Length];
                for (int i = 0; i < _props.Length; i++)
                    values[i] = _props[i].GetValue(item);

                grid.Rows.Add(values);
                _originalValues.Add(values);
            }

            RefreshAllCellStyles();
            ApplySizing();
        }

        private void Grid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            // Commit immediately so the "edited" highlight updates as soon as the cell changes
            if (grid.IsCurrentCellDirty)
                grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) 
                return;

            RefreshCellStyle(e.RowIndex, e.ColumnIndex);
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            for (int r = 0; r < grid.Rows.Count && r < _originalValues.Count; r++)
                for (int c = 0; c < grid.Columns.Count; c++)
                    grid.Rows[r].Cells[c].Value = _originalValues[r][c];

            RefreshAllCellStyles();
        }

        private void RefreshAllCellStyles()
        {
            for (int r = 0; r < grid.Rows.Count; r++)
                for (int c = 0; c < grid.Columns.Count; c++)
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

            cell.Style.BackColor = edited ? EditedBackColor : Color.Empty;
            cell.Style.ForeColor = edited ? EditedForeColor : Color.Empty;
            cell.Style.Font = edited ? _editedFont : null;

            btnReset.Enabled = _editedCells.Count > 0;
        }

        private static bool ValuesEqual(object? a, object? b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;

            return string.Equals(a.ToString(), b.ToString(), StringComparison.Ordinal);
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
    }
}
