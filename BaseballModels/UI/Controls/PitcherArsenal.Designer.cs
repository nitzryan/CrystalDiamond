namespace UI.Controls
{
    partial class PitcherArsenal
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
            groupBox5 = new GroupBox();
            tableArsenal = new TableLayoutPanel();
            label3 = new Label();
            label1 = new Label();
            label20 = new Label();
            label19 = new Label();
            label18 = new Label();
            label16 = new Label();
            cbArsenalYear = new ComboBox();
            label2 = new Label();
            groupBox5.SuspendLayout();
            tableArsenal.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox5
            // 
            groupBox5.AutoSize = true;
            groupBox5.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox5.Controls.Add(tableArsenal);
            groupBox5.Dock = DockStyle.Bottom;
            groupBox5.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox5.Location = new Point(0, 41);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(326, 154);
            groupBox5.TabIndex = 7;
            groupBox5.TabStop = false;
            groupBox5.Text = "Arsenal";
            // 
            // tableArsenal
            // 
            tableArsenal.AutoSize = true;
            tableArsenal.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableArsenal.ColumnCount = 7;
            tableArsenal.ColumnStyles.Add(new ColumnStyle());
            tableArsenal.ColumnStyles.Add(new ColumnStyle());
            tableArsenal.ColumnStyles.Add(new ColumnStyle());
            tableArsenal.ColumnStyles.Add(new ColumnStyle());
            tableArsenal.ColumnStyles.Add(new ColumnStyle());
            tableArsenal.ColumnStyles.Add(new ColumnStyle());
            tableArsenal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableArsenal.Controls.Add(label3, 5, 0);
            tableArsenal.Controls.Add(label1, 4, 0);
            tableArsenal.Controls.Add(label20, 3, 0);
            tableArsenal.Controls.Add(label19, 2, 0);
            tableArsenal.Controls.Add(label18, 1, 0);
            tableArsenal.Controls.Add(label16, 0, 0);
            tableArsenal.Dock = DockStyle.Fill;
            tableArsenal.Location = new Point(3, 31);
            tableArsenal.Name = "tableArsenal";
            tableArsenal.RowCount = 3;
            tableArsenal.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableArsenal.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableArsenal.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableArsenal.Size = new Size(320, 120);
            tableArsenal.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(232, 0);
            label3.Name = "label3";
            label3.Size = new Size(34, 40);
            label3.TabIndex = 6;
            label3.Text = "Cnt";
            label3.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(194, 0);
            label1.Name = "label1";
            label1.Size = new Size(32, 40);
            label1.TabIndex = 5;
            label1.Text = "Act";
            label1.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Dock = DockStyle.Fill;
            label20.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label20.Location = new Point(144, 0);
            label20.Name = "label20";
            label20.Size = new Size(44, 40);
            label20.TabIndex = 4;
            label20.Text = "Pitch";
            label20.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Dock = DockStyle.Fill;
            label19.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label19.Location = new Point(96, 0);
            label19.Name = "label19";
            label19.Size = new Size(42, 40);
            label19.TabIndex = 3;
            label19.Text = "Stuff";
            label19.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Dock = DockStyle.Fill;
            label18.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label18.Location = new Point(53, 0);
            label18.Name = "label18";
            label18.Size = new Size(37, 40);
            label18.TabIndex = 2;
            label18.Text = "Usg";
            label18.TextAlign = ContentAlignment.BottomCenter;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Dock = DockStyle.Fill;
            label16.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label16.Location = new Point(3, 0);
            label16.Name = "label16";
            label16.Size = new Size(44, 40);
            label16.TabIndex = 1;
            label16.Text = "Pitch";
            label16.TextAlign = ContentAlignment.BottomCenter;
            // 
            // cbArsenalYear
            // 
            cbArsenalYear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cbArsenalYear.FormattingEnabled = true;
            cbArsenalYear.Location = new Point(118, 3);
            cbArsenalYear.Name = "cbArsenalYear";
            cbArsenalYear.Size = new Size(197, 23);
            cbArsenalYear.TabIndex = 8;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(8, 5);
            label2.Name = "label2";
            label2.Size = new Size(40, 21);
            label2.TabIndex = 6;
            label2.Text = "Year";
            // 
            // PitcherArsenal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(cbArsenalYear);
            Controls.Add(groupBox5);
            Controls.Add(label2);
            Name = "PitcherArsenal";
            Size = new Size(326, 195);
            Load += PitcherArsenal_Load;
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            tableArsenal.ResumeLayout(false);
            tableArsenal.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox5;
        private TableLayoutPanel tableArsenal;
        private Label label20;
        private Label label19;
        private Label label18;
        private Label label16;
        private ComboBox cbArsenalYear;
        private Label label1;
        private Label label2;
        private Label label3;
    }
}
