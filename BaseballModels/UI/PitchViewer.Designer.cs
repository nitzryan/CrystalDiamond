namespace UI
{
    partial class PitchViewer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pitchPanel1 = new PitchPanel();
            SuspendLayout();
            // 
            // pitchPanel1
            // 
            pitchPanel1.Location = new Point(312, 74);
            pitchPanel1.Name = "pitchPanel1";
            pitchPanel1.Size = new Size(599, 655);
            pitchPanel1.TabIndex = 0;
            // 
            // PitchViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(986, 783);
            Controls.Add(pitchPanel1);
            Name = "PitchViewer";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Panel pitchDrawingPanel;
        private PitchPanel pitchPanel1;
    }
}
