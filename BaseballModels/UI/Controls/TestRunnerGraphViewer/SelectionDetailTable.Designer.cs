namespace UI.Controls.TestRunnerGraphViewer
{
    partial class SelectionDetailTable
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
            tlpMain.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.AutoScroll = true;
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 6;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6666679F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6666679F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6666679F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6666679F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6666679F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 16.6666679F));
            tlpMain.Size = new Size(150, 150);
            tlpMain.TabIndex = 0;
            // 
            // SelectionDetailTable
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tlpMain);
            Name = "SelectionDetailTable";
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpMain;
    }
}
