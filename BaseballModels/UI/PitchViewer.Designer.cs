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
            groupBoxPlayer = new GroupBox();
            playerSearchBar = new UI.Controls.PlayerSearchBar();
            groupBoxFilters = new GroupBox();
            label13 = new Label();
            cbLevel = new ComboBox();
            label2 = new Label();
            label1 = new Label();
            cbSituations = new ComboBox();
            cbPitchSelector = new ComboBox();
            yearSelector = new RangeSelector();
            nudMinPitches = new NumericUpDown();
            pbPitchSearch = new Button();
            label3 = new Label();
            label4 = new Label();
            labelKernel = new Label();
            cbBinSizes = new ComboBox();
            cbKernel = new ComboBox();
            groupBoxPostProcessing = new GroupBox();
            nudMaxScale = new NumericUpDown();
            label7 = new Label();
            groupBoxOutput = new GroupBox();
            label6 = new Label();
            cbOutput = new ComboBox();
            label5 = new Label();
            cbModel = new ComboBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            pitcherArsenal = new UI.Controls.PitcherArsenal();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            pitchBucketViewer = new UI.Controls.PitchBucketViewer();
            tabPage2 = new TabPage();
            stuffModelViewer = new UI.Controls.StuffModelViewer();
            groupBoxPlayer.SuspendLayout();
            groupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPitches).BeginInit();
            groupBoxPostProcessing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxScale).BeginInit();
            groupBoxOutput.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxPlayer
            // 
            groupBoxPlayer.Controls.Add(playerSearchBar);
            groupBoxPlayer.Location = new Point(3, 3);
            groupBoxPlayer.Name = "groupBoxPlayer";
            groupBoxPlayer.Size = new Size(317, 205);
            groupBoxPlayer.TabIndex = 1;
            groupBoxPlayer.TabStop = false;
            groupBoxPlayer.Text = "Player";
            // 
            // playerSearchBar
            // 
            playerSearchBar.Dock = DockStyle.Fill;
            playerSearchBar.Location = new Point(3, 19);
            playerSearchBar.Name = "playerSearchBar";
            playerSearchBar.Size = new Size(311, 183);
            playerSearchBar.TabIndex = 0;
            // 
            // groupBoxFilters
            // 
            groupBoxFilters.AutoSize = true;
            groupBoxFilters.Controls.Add(label13);
            groupBoxFilters.Controls.Add(cbLevel);
            groupBoxFilters.Controls.Add(label2);
            groupBoxFilters.Controls.Add(label1);
            groupBoxFilters.Controls.Add(cbSituations);
            groupBoxFilters.Controls.Add(cbPitchSelector);
            groupBoxFilters.Controls.Add(yearSelector);
            groupBoxFilters.Location = new Point(6, 214);
            groupBoxFilters.Name = "groupBoxFilters";
            groupBoxFilters.Size = new Size(311, 166);
            groupBoxFilters.TabIndex = 2;
            groupBoxFilters.TabStop = false;
            groupBoxFilters.Text = "Pitch Filters";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(6, 124);
            label13.Name = "label13";
            label13.Size = new Size(34, 15);
            label13.TabIndex = 4;
            label13.Text = "Level";
            // 
            // cbLevel
            // 
            cbLevel.FormattingEnabled = true;
            cbLevel.Location = new Point(129, 121);
            cbLevel.Name = "cbLevel";
            cbLevel.Size = new Size(170, 23);
            cbLevel.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 95);
            label2.Name = "label2";
            label2.Size = new Size(54, 15);
            label2.TabIndex = 2;
            label2.Text = "Situation";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 66);
            label1.Name = "label1";
            label1.Size = new Size(34, 15);
            label1.TabIndex = 2;
            label1.Text = "Pitch";
            // 
            // cbSituations
            // 
            cbSituations.FormattingEnabled = true;
            cbSituations.Location = new Point(129, 92);
            cbSituations.Name = "cbSituations";
            cbSituations.Size = new Size(170, 23);
            cbSituations.TabIndex = 1;
            // 
            // cbPitchSelector
            // 
            cbPitchSelector.FormattingEnabled = true;
            cbPitchSelector.Location = new Point(129, 63);
            cbPitchSelector.Name = "cbPitchSelector";
            cbPitchSelector.Size = new Size(170, 23);
            cbPitchSelector.TabIndex = 1;
            // 
            // yearSelector
            // 
            yearSelector.Location = new Point(6, 22);
            yearSelector.Name = "yearSelector";
            yearSelector.Size = new Size(299, 35);
            yearSelector.TabIndex = 0;
            // 
            // nudMinPitches
            // 
            nudMinPitches.Location = new Point(179, 88);
            nudMinPitches.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudMinPitches.Name = "nudMinPitches";
            nudMinPitches.Size = new Size(120, 23);
            nudMinPitches.TabIndex = 4;
            nudMinPitches.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // pbPitchSearch
            // 
            pbPitchSearch.Location = new Point(3, 669);
            pbPitchSearch.Name = "pbPitchSearch";
            pbPitchSearch.Size = new Size(305, 23);
            pbPitchSearch.TabIndex = 3;
            pbPitchSearch.Text = "View";
            pbPitchSearch.UseVisualStyleBackColor = true;
            pbPitchSearch.Click += pbPitchSearch_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 90);
            label3.Name = "label3";
            label3.Size = new Size(69, 15);
            label3.TabIndex = 2;
            label3.Text = "Min Pitches";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 33);
            label4.Name = "label4";
            label4.Size = new Size(52, 15);
            label4.TabIndex = 2;
            label4.Text = "Bin Sizes";
            // 
            // labelKernel
            // 
            labelKernel.AutoSize = true;
            labelKernel.Location = new Point(6, 62);
            labelKernel.Name = "labelKernel";
            labelKernel.Size = new Size(40, 15);
            labelKernel.TabIndex = 2;
            labelKernel.Text = "Kernel";
            // 
            // cbBinSizes
            // 
            cbBinSizes.FormattingEnabled = true;
            cbBinSizes.Location = new Point(129, 30);
            cbBinSizes.Name = "cbBinSizes";
            cbBinSizes.Size = new Size(170, 23);
            cbBinSizes.TabIndex = 1;
            // 
            // cbKernel
            // 
            cbKernel.FormattingEnabled = true;
            cbKernel.Location = new Point(129, 59);
            cbKernel.Name = "cbKernel";
            cbKernel.Size = new Size(170, 23);
            cbKernel.TabIndex = 1;
            // 
            // groupBoxPostProcessing
            // 
            groupBoxPostProcessing.Controls.Add(nudMaxScale);
            groupBoxPostProcessing.Controls.Add(nudMinPitches);
            groupBoxPostProcessing.Controls.Add(cbKernel);
            groupBoxPostProcessing.Controls.Add(cbBinSizes);
            groupBoxPostProcessing.Controls.Add(label7);
            groupBoxPostProcessing.Controls.Add(label3);
            groupBoxPostProcessing.Controls.Add(labelKernel);
            groupBoxPostProcessing.Controls.Add(label4);
            groupBoxPostProcessing.Location = new Point(6, 512);
            groupBoxPostProcessing.Name = "groupBoxPostProcessing";
            groupBoxPostProcessing.Size = new Size(305, 151);
            groupBoxPostProcessing.TabIndex = 3;
            groupBoxPostProcessing.TabStop = false;
            groupBoxPostProcessing.Text = "Post Processing";
            // 
            // nudMaxScale
            // 
            nudMaxScale.Location = new Point(179, 115);
            nudMaxScale.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudMaxScale.Name = "nudMaxScale";
            nudMaxScale.Size = new Size(120, 23);
            nudMaxScale.TabIndex = 4;
            nudMaxScale.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 117);
            label7.Name = "label7";
            label7.Size = new Size(59, 15);
            label7.TabIndex = 2;
            label7.Text = "Max Scale";
            // 
            // groupBoxOutput
            // 
            groupBoxOutput.Controls.Add(label6);
            groupBoxOutput.Controls.Add(cbOutput);
            groupBoxOutput.Controls.Add(label5);
            groupBoxOutput.Controls.Add(cbModel);
            groupBoxOutput.Location = new Point(6, 395);
            groupBoxOutput.Name = "groupBoxOutput";
            groupBoxOutput.Size = new Size(305, 111);
            groupBoxOutput.TabIndex = 4;
            groupBoxOutput.TabStop = false;
            groupBoxOutput.Text = "Output Type";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 65);
            label6.Name = "label6";
            label6.Size = new Size(45, 15);
            label6.TabIndex = 2;
            label6.Text = "Output";
            // 
            // cbOutput
            // 
            cbOutput.FormattingEnabled = true;
            cbOutput.Location = new Point(129, 62);
            cbOutput.Name = "cbOutput";
            cbOutput.Size = new Size(170, 23);
            cbOutput.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 36);
            label5.Name = "label5";
            label5.Size = new Size(41, 15);
            label5.TabIndex = 2;
            label5.Text = "Model";
            // 
            // cbModel
            // 
            cbModel.FormattingEnabled = true;
            cbModel.Location = new Point(129, 33);
            cbModel.Name = "cbModel";
            cbModel.Size = new Size(170, 23);
            cbModel.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 500F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(tabControl1, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1352, 1102);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.AutoSize = true;
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(pitcherArsenal);
            panel1.Controls.Add(groupBoxPlayer);
            panel1.Controls.Add(pbPitchSearch);
            panel1.Controls.Add(groupBoxOutput);
            panel1.Controls.Add(groupBoxPostProcessing);
            panel1.Controls.Add(groupBoxFilters);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(494, 1096);
            panel1.TabIndex = 1;
            // 
            // pitcherArsenal
            // 
            pitcherArsenal.AutoSize = true;
            pitcherArsenal.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pitcherArsenal.Dock = DockStyle.Bottom;
            pitcherArsenal.Location = new Point(0, 942);
            pitcherArsenal.Name = "pitcherArsenal";
            pitcherArsenal.Size = new Size(494, 154);
            pitcherArsenal.TabIndex = 7;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(503, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(846, 1096);
            tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(pitchBucketViewer);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(838, 1068);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Buckets";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // pitchBucketViewer
            // 
            pitchBucketViewer.Dock = DockStyle.Fill;
            pitchBucketViewer.Location = new Point(3, 3);
            pitchBucketViewer.Name = "pitchBucketViewer";
            pitchBucketViewer.Size = new Size(832, 1062);
            pitchBucketViewer.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(stuffModelViewer);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(838, 1068);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Tester";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // stuffModelViewer
            // 
            stuffModelViewer.Dock = DockStyle.Fill;
            stuffModelViewer.Location = new Point(3, 3);
            stuffModelViewer.Name = "stuffModelViewer";
            stuffModelViewer.Size = new Size(832, 1062);
            stuffModelViewer.TabIndex = 0;
            // 
            // PitchViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1352, 1102);
            Controls.Add(tableLayoutPanel1);
            Name = "PitchViewer";
            Text = "Form1";
            Load += Form1_Load;
            groupBoxPlayer.ResumeLayout(false);
            groupBoxFilters.ResumeLayout(false);
            groupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPitches).EndInit();
            groupBoxPostProcessing.ResumeLayout(false);
            groupBoxPostProcessing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxScale).EndInit();
            groupBoxOutput.ResumeLayout(false);
            groupBoxOutput.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pitchDrawingPanel;
        private GroupBox groupBoxPlayer;
        private GroupBox groupBoxFilters;
        private RangeSelector yearSelector;
        private Label label1;
        private ComboBox cbPitchSelector;
        private Label label2;
        private ComboBox cbSituations;
        private NumericUpDown nudMinPitches;
        private Button pbPitchSearch;
        private Label label3;
        private Label labelKernel;
        private ComboBox cbKernel;
        private Label label4;
        private ComboBox cbBinSizes;
        private GroupBox groupBoxPostProcessing;
        private GroupBox groupBoxOutput;
        private Label label6;
        private ComboBox cbOutput;
        private Label label5;
        private ComboBox cbModel;
        private NumericUpDown nudMaxScale;
        private Label label7;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Controls.PlayerSearchBar playerSearchBar;
        private Label label13;
        private ComboBox cbLevel;
        private Controls.PitcherArsenal pitcherArsenal;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Controls.StuffModelViewer stuffModelViewer;
        private Controls.PitchBucketViewer pitchBucketViewer;
    }
}
