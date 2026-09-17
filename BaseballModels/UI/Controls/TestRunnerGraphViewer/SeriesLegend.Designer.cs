namespace UI.Controls.TestRunnerGraphViewer
{
    partial class SeriesLegend
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
            flpEntries = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flpEntries
            // 
            flpEntries.AutoSize = true;
            flpEntries.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpEntries.Dock = DockStyle.Fill;
            flpEntries.FlowDirection = FlowDirection.TopDown;
            flpEntries.Location = new Point(0, 0);
            flpEntries.Name = "flpEntries";
            flpEntries.Size = new Size(150, 150);
            flpEntries.TabIndex = 0;
            flpEntries.WrapContents = false;
            // 
            // SeriesLegend
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flpEntries);
            Name = "SeriesLegend";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flpEntries;
    }
}
