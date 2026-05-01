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
            nudMinPitches = new NumericUpDown();
            pbPitchSearch = new Button();
            label3 = new Label();
            label4 = new Label();
            labelKernel = new Label();
            label2 = new Label();
            label1 = new Label();
            cbBinSizes = new ComboBox();
            cbKernel = new ComboBox();
            cbSituations = new ComboBox();
            cbPitchSelector = new ComboBox();
            yearSelector = new RangeSelector();
            groupBoxPlayer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)playerIdEntry).BeginInit();
            groupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPitches).BeginInit();
            SuspendLayout();
            // 
            // pitchPanel
            // 
            pitchPanel.Location = new Point(375, 65);
            pitchPanel.Name = "pitchPanel";
            pitchPanel.Size = new Size(599, 655);
            pitchPanel.TabIndex = 0;
            // 
            // groupBoxPlayer
            // 
            groupBoxPlayer.Controls.Add(playerLinkLabel);
            groupBoxPlayer.Controls.Add(playerIdEntry);
            groupBoxPlayer.Controls.Add(pbSearchPlayer);
            groupBoxPlayer.Location = new Point(12, 12);
            groupBoxPlayer.Name = "groupBoxPlayer";
            groupBoxPlayer.Size = new Size(289, 98);
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
            groupBoxFilters.Controls.Add(nudMinPitches);
            groupBoxFilters.Controls.Add(pbPitchSearch);
            groupBoxFilters.Controls.Add(label3);
            groupBoxFilters.Controls.Add(label4);
            groupBoxFilters.Controls.Add(labelKernel);
            groupBoxFilters.Controls.Add(label2);
            groupBoxFilters.Controls.Add(label1);
            groupBoxFilters.Controls.Add(cbBinSizes);
            groupBoxFilters.Controls.Add(cbKernel);
            groupBoxFilters.Controls.Add(cbSituations);
            groupBoxFilters.Controls.Add(cbPitchSelector);
            groupBoxFilters.Controls.Add(yearSelector);
            groupBoxFilters.Location = new Point(12, 152);
            groupBoxFilters.Name = "groupBoxFilters";
            groupBoxFilters.Size = new Size(305, 461);
            groupBoxFilters.TabIndex = 2;
            groupBoxFilters.TabStop = false;
            groupBoxFilters.Text = "Filters";
            // 
            // nudMinPitches
            // 
            nudMinPitches.Location = new Point(179, 403);
            nudMinPitches.Name = "nudMinPitches";
            nudMinPitches.Size = new Size(120, 23);
            nudMinPitches.TabIndex = 4;
            // 
            // pbPitchSearch
            // 
            pbPitchSearch.Location = new Point(224, 432);
            pbPitchSearch.Name = "pbPitchSearch";
            pbPitchSearch.Size = new Size(75, 23);
            pbPitchSearch.TabIndex = 3;
            pbPitchSearch.Text = "Search";
            pbPitchSearch.UseVisualStyleBackColor = true;
            pbPitchSearch.Click += pbPitchSearch_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 405);
            label3.Name = "label3";
            label3.Size = new Size(69, 15);
            label3.TabIndex = 2;
            label3.Text = "Min Pitches";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 348);
            label4.Name = "label4";
            label4.Size = new Size(52, 15);
            label4.TabIndex = 2;
            label4.Text = "Bin Sizes";
            // 
            // labelKernel
            // 
            labelKernel.AutoSize = true;
            labelKernel.Location = new Point(6, 377);
            labelKernel.Name = "labelKernel";
            labelKernel.Size = new Size(40, 15);
            labelKernel.TabIndex = 2;
            labelKernel.Text = "Kernel";
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
            // cbBinSizes
            // 
            cbBinSizes.FormattingEnabled = true;
            cbBinSizes.Location = new Point(129, 345);
            cbBinSizes.Name = "cbBinSizes";
            cbBinSizes.Size = new Size(170, 23);
            cbBinSizes.TabIndex = 1;
            // 
            // cbKernel
            // 
            cbKernel.FormattingEnabled = true;
            cbKernel.Location = new Point(129, 374);
            cbKernel.Name = "cbKernel";
            cbKernel.Size = new Size(170, 23);
            cbKernel.TabIndex = 1;
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
            // PitchViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(986, 783);
            Controls.Add(groupBoxFilters);
            Controls.Add(groupBoxPlayer);
            Controls.Add(pitchPanel);
            Name = "PitchViewer";
            Text = "Form1";
            Load += Form1_Load;
            groupBoxPlayer.ResumeLayout(false);
            groupBoxPlayer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)playerIdEntry).EndInit();
            groupBoxFilters.ResumeLayout(false);
            groupBoxFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPitches).EndInit();
            ResumeLayout(false);
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
    }
}
