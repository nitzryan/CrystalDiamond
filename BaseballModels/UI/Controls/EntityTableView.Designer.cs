namespace UI.Controls
{
    partial class EntityTableView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            btnReset = new Button();
            lblTitle = new Label();
            grid = new DataGridView();
            btnToggle = new Button();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(btnToggle);
            pnlHeader.Controls.Add(btnReset);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1000, 30);
            pnlHeader.TabIndex = 1;
            // 
            // btnReset
            // 
            btnReset.Dock = DockStyle.Right;
            btnReset.Location = new Point(880, 0);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(120, 30);
            btnReset.TabIndex = 0;
            btnReset.Text = "Reset Values";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += BtnReset_Click;
            btnReset.Enabled = false;
            // 
            // btnToggle
            // 
            btnToggle.Dock = DockStyle.Right;
            btnToggle.Name = "btnToggle";
            btnToggle.Size = new Size(80, 30);
            btnToggle.Text = "Hide ▴";
            btnToggle.UseVisualStyleBackColor = true;
            btnToggle.Click += BtnToggle_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitle.Location = new Point(3, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(46, 20);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Table";
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = DockStyle.Fill;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;
            grid.Location = new Point(0, 30);
            grid.Name = "grid";
            grid.RowHeadersVisible = false;
            grid.Size = new Size(1000, 230);
            grid.TabIndex = 0;
            grid.CellValueChanged += Grid_CellValueChanged;
            grid.CurrentCellDirtyStateChanged += Grid_CurrentCellDirtyStateChanged;
            // 
            // EntityTableView
            // 
            Controls.Add(grid);
            Controls.Add(pnlHeader);
            Name = "EntityTableView";
            Size = new Size(1000, 260);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        private Panel pnlHeader;
        private Button btnReset;
        private Label lblTitle;
        private DataGridView grid;
        private Button btnToggle;
    }
}
