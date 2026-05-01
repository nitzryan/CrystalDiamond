namespace UI
{
    partial class RangeSelector
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
            name = new Label();
            minRange = new NumericUpDown();
            maxRange = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)minRange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)maxRange).BeginInit();
            SuspendLayout();
            // 
            // name
            // 
            name.AutoSize = true;
            name.Location = new Point(129, 5);
            name.Name = "name";
            name.Size = new Size(38, 15);
            name.TabIndex = 1;
            name.Text = "label1";
            // 
            // minRange
            // 
            minRange.Location = new Point(3, 3);
            minRange.Name = "minRange";
            minRange.Size = new Size(120, 23);
            minRange.TabIndex = 0;
            minRange.ValueChanged += this.minRange_ValueChanged;
            // 
            // maxRange
            // 
            maxRange.Location = new Point(173, 3);
            maxRange.Name = "maxRange";
            maxRange.Size = new Size(120, 23);
            maxRange.TabIndex = 2;
            maxRange.ValueChanged += this.maxRange_ValueChanged;
            // 
            // RangeSelector
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(maxRange);
            Controls.Add(name);
            Controls.Add(minRange);
            Name = "RangeSelector";
            Size = new Size(299, 35);
            ((System.ComponentModel.ISupportInitialize)minRange).EndInit();
            ((System.ComponentModel.ISupportInitialize)maxRange).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NumericUpDown numericUpDown1;
        private Label name;
        private NumericUpDown minRange;
        private NumericUpDown maxRange;
    }
}
