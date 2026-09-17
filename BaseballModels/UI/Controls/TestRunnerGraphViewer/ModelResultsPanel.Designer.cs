namespace UI.Controls.TestRunnerGraphViewer
{
    partial class ModelResultsPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tlpMain = new TableLayoutPanel();
            tlpRightCol = new TableLayoutPanel();
            legend = new SeriesLegend();
            detail = new SelectionDetailTable();
            graph = new ModelGraphViewer();
            tlpMain.SuspendLayout();
            tlpRightCol.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tlpMain.Controls.Add(tlpRightCol, 1, 0);
            tlpMain.Controls.Add(graph, 0, 0);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 1;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(573, 312);
            tlpMain.TabIndex = 0;
            // 
            // tlpRightCol
            // 
            tlpRightCol.ColumnCount = 1;
            tlpRightCol.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRightCol.Controls.Add(legend, 0, 0);
            tlpRightCol.Controls.Add(detail, 0, 1);
            tlpRightCol.Dock = DockStyle.Fill;
            tlpRightCol.Location = new Point(376, 3);
            tlpRightCol.Name = "tlpRightCol";
            tlpRightCol.RowCount = 2;
            tlpRightCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tlpRightCol.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRightCol.Size = new Size(194, 306);
            tlpRightCol.TabIndex = 0;
            // 
            // legend
            // 
            legend.AutoSize = true;
            legend.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            legend.Dock = DockStyle.Fill;
            legend.Location = new Point(3, 3);
            legend.Name = "legend";
            legend.Size = new Size(188, 44);
            legend.TabIndex = 0;
            // 
            // detail
            // 
            detail.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            detail.Location = new Point(3, 153);
            detail.Name = "detail";
            detail.Size = new Size(150, 150);
            detail.TabIndex = 1;
            detail.Visible = false;
            // 
            // graph
            // 
            graph.Dock = DockStyle.Fill;
            graph.Location = new Point(3, 3);
            graph.Name = "graph";
            graph.Size = new Size(367, 306);
            graph.TabIndex = 1;
            // 
            // ModelResultsPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tlpMain);
            Name = "ModelResultsPanel";
            Size = new Size(573, 312);
            tlpMain.ResumeLayout(false);
            tlpRightCol.ResumeLayout(false);
            tlpRightCol.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpMain;
        private TableLayoutPanel tlpRightCol;
        private SelectionDetailTable detail;
        private ModelGraphViewer graph;
        private SeriesLegend legend;
    }
}
