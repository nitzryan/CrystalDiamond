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
            pitchPanel = new PitchPanel();
            groupBoxPlayer = new GroupBox();
            playerLinkLabel = new LinkLabel();
            playerIdEntry = new NumericUpDown();
            pbSearchPlayer = new Button();
            groupBoxFilters = new GroupBox();
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
            groupBoxPlayer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)playerIdEntry).BeginInit();
            groupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPitches).BeginInit();
            groupBoxPostProcessing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxScale).BeginInit();
            groupBoxOutput.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pitchPanel
            // 
            pitchPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pitchPanel.Dock = DockStyle.Fill;
            pitchPanel.Location = new Point(332, 3);
            pitchPanel.Name = "pitchPanel";
            pitchPanel.Size = new Size(651, 777);
            pitchPanel.TabIndex = 0;
            // 
            // groupBoxPlayer
            // 
            groupBoxPlayer.Controls.Add(playerLinkLabel);
            groupBoxPlayer.Controls.Add(playerIdEntry);
            groupBoxPlayer.Controls.Add(pbSearchPlayer);
            groupBoxPlayer.Location = new Point(3, 3);
            groupBoxPlayer.Name = "groupBoxPlayer";
            groupBoxPlayer.Size = new Size(317, 98);
            groupBoxPlayer.TabIndex = 1;
            groupBoxPlayer.TabStop = false;
            groupBoxPlayer.Text = "Player";
            // 
            // playerLinkLabel
            // 
            playerLinkLabel.AutoSize = true;
            playerLinkLabel.LinkColor = Color.Black;
            playerLinkLabel.Location = new Point(6, 62);
            playerLinkLabel.Name = "playerLinkLabel";
            playerLinkLabel.Size = new Size(60, 15);
            playerLinkLabel.TabIndex = 2;
            playerLinkLabel.TabStop = true;
            playerLinkLabel.Text = "linkLabel1";
            playerLinkLabel.VisitedLinkColor = Color.Black;
            // 
            // playerIdEntry
            // 
            playerIdEntry.InterceptArrowKeys = false;
            playerIdEntry.Location = new Point(6, 24);
            playerIdEntry.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            playerIdEntry.Name = "playerIdEntry";
            playerIdEntry.Size = new Size(120, 23);
            playerIdEntry.TabIndex = 1;
            // 
            // pbSearchPlayer
            // 
            pbSearchPlayer.Location = new Point(208, 22);
            pbSearchPlayer.Name = "pbSearchPlayer";
            pbSearchPlayer.Size = new Size(75, 23);
            pbSearchPlayer.TabIndex = 0;
            pbSearchPlayer.Text = "Search";
            pbSearchPlayer.UseVisualStyleBackColor = true;
            pbSearchPlayer.Click += pbSearchPlayer_Click;
            // 
            // groupBoxFilters
            // 
            groupBoxFilters.AutoSize = true;
            groupBoxFilters.Controls.Add(label2);
            groupBoxFilters.Controls.Add(label1);
            groupBoxFilters.Controls.Add(cbSituations);
            groupBoxFilters.Controls.Add(cbPitchSelector);
            groupBoxFilters.Controls.Add(yearSelector);
            groupBoxFilters.Location = new Point(9, 107);
            groupBoxFilters.Name = "groupBoxFilters";
            groupBoxFilters.Size = new Size(311, 137);
            groupBoxFilters.TabIndex = 2;
            groupBoxFilters.TabStop = false;
            groupBoxFilters.Text = "Pitch Filters";
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
            pbPitchSearch.Location = new Point(9, 518);
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
            groupBoxPostProcessing.Location = new Point(9, 361);
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
            groupBoxOutput.Location = new Point(9, 244);
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
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pitchPanel, 1, 0);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(986, 783);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(groupBoxPlayer);
            panel1.Controls.Add(pbPitchSearch);
            panel1.Controls.Add(groupBoxOutput);
            panel1.Controls.Add(groupBoxPostProcessing);
            panel1.Controls.Add(groupBoxFilters);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(323, 777);
            panel1.TabIndex = 1;
            // 
            // PitchViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(986, 783);
            Controls.Add(tableLayoutPanel1);
            Name = "PitchViewer";
            Text = "Form1";
            Load += Form1_Load;
            groupBoxPlayer.ResumeLayout(false);
            groupBoxPlayer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)playerIdEntry).EndInit();
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
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pitchDrawingPanel;
        private PitchPanel pitchPanel;
        private GroupBox groupBoxPlayer;
        private Button pbSearchPlayer;
        private NumericUpDown playerIdEntry;
        private LinkLabel playerLinkLabel;
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
    }
}
