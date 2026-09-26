namespace UI.Controls
{
    partial class ModelGraphViewer
    {
        private void InitializeComponent()
        {
            tlpMain = new TableLayoutPanel();
            formsPlot = new ScottPlot.WinForms.FormsPlot();
            tlpDropdowns = new TableLayoutPanel();
            outputSelectionComboBox = new ComboBox();
            metricSelectionComboBox = new ComboBox();
            tlpMain.SuspendLayout();
            tlpDropdowns.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tlpMain.Controls.Add(formsPlot, 0, 1);
            tlpMain.Controls.Add(tlpDropdowns, 0, 0);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpMain.Size = new Size(435, 321);
            tlpMain.TabIndex = 0;
            // 
            // formsPlot
            // 
            formsPlot.Dock = DockStyle.Fill;
            formsPlot.Location = new Point(3, 53);
            formsPlot.Name = "formsPlot";
            formsPlot.Size = new Size(429, 265);
            formsPlot.TabIndex = 1;
            formsPlot.MouseDown += FormsPlot_MouseDown;
            // 
            // tlpDropdowns
            // 
            tlpDropdowns.ColumnCount = 2;
            tlpDropdowns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDropdowns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDropdowns.Controls.Add(metricSelectionComboBox, 1, 0);
            tlpDropdowns.Controls.Add(outputSelectionComboBox, 0, 0);
            tlpDropdowns.Dock = DockStyle.Fill;
            tlpDropdowns.Location = new Point(3, 3);
            tlpDropdowns.Name = "tlpDropdowns";
            tlpDropdowns.RowCount = 1;
            tlpDropdowns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpDropdowns.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpDropdowns.Size = new Size(429, 44);
            tlpDropdowns.TabIndex = 2;
            // 
            // outputSelectionComboBox
            // 
            outputSelectionComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            outputSelectionComboBox.FormattingEnabled = true;
            outputSelectionComboBox.Location = new Point(98, 18);
            outputSelectionComboBox.Name = "outputSelectionComboBox";
            outputSelectionComboBox.Size = new Size(113, 23);
            outputSelectionComboBox.TabIndex = 0;
            // 
            // metricSelectionComboBox
            // 
            metricSelectionComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            metricSelectionComboBox.FormattingEnabled = true;
            metricSelectionComboBox.Location = new Point(217, 18);
            metricSelectionComboBox.Name = "metricSelectionComboBox";
            metricSelectionComboBox.Size = new Size(113, 23);
            metricSelectionComboBox.TabIndex = 1;
            // 
            // ModelGraphViewer
            // 
            Controls.Add(tlpMain);
            Name = "ModelGraphViewer";
            Size = new Size(435, 321);
            tlpMain.ResumeLayout(false);
            tlpDropdowns.ResumeLayout(false);
            ResumeLayout(false);
        }
        private TableLayoutPanel tlpMain;
        private ComboBox outputSelectionComboBox;
        private ScottPlot.WinForms.FormsPlot formsPlot;
        private TableLayoutPanel tlpDropdowns;
        private ComboBox metricSelectionComboBox;
    }
}
