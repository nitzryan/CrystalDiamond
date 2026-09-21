namespace UI.Controls
{
    partial class ModelGraphViewer
    {
        private void InitializeComponent()
        {
            tlpMain = new TableLayoutPanel();
            outputSelectionComboBox = new ComboBox();
            formsPlot = new ScottPlot.WinForms.FormsPlot();
            tlpMain.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tlpMain.Controls.Add(outputSelectionComboBox, 0, 0);
            tlpMain.Controls.Add(formsPlot, 0, 1);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 2;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(435, 321);
            tlpMain.TabIndex = 0;
            // 
            // outputSelectionComboBox
            // 
            outputSelectionComboBox.Anchor = AnchorStyles.Bottom;
            outputSelectionComboBox.FormattingEnabled = true;
            outputSelectionComboBox.Location = new Point(157, 24);
            outputSelectionComboBox.Name = "outputSelectionComboBox";
            outputSelectionComboBox.Size = new Size(121, 23);
            outputSelectionComboBox.TabIndex = 0;
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
            // ModelGraphViewer
            // 
            Controls.Add(tlpMain);
            Name = "ModelGraphViewer";
            Size = new Size(435, 321);
            tlpMain.ResumeLayout(false);
            ResumeLayout(false);
        }
        private TableLayoutPanel tlpMain;
        private ComboBox outputSelectionComboBox;
        private ScottPlot.WinForms.FormsPlot formsPlot;
    }
}
