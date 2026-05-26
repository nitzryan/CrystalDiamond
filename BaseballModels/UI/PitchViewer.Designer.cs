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
            tableLayoutPanel2 = new TableLayoutPanel();
            panelPitchStats = new TableLayoutPanel();
            labelName = new Label();
            groupBox3 = new GroupBox();
            tableLayoutPanel5 = new TableLayoutPanel();
            labelInPlay = new Label();
            label17 = new Label();
            labelFoul = new Label();
            label15 = new Label();
            _labelWhiffRate = new Label();
            labelCSW = new Label();
            _labelCSW = new Label();
            labelWhiff = new Label();
            groupBox1 = new GroupBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            labelMoveZ = new Label();
            _labelVelocity = new Label();
            _labelMoveX = new Label();
            _labelMoveZ = new Label();
            labelMoveX = new Label();
            labelVel = new Label();
            groupBox2 = new GroupBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            label8 = new Label();
            labelABPA = new Label();
            _labelABPA = new Label();
            _labelAVG = new Label();
            labelAVG = new Label();
            label9 = new Label();
            labelSLG = new Label();
            labelOBP = new Label();
            groupBox4 = new GroupBox();
            tableLayoutPanel6 = new TableLayoutPanel();
            labelActual = new Label();
            label11 = new Label();
            labelPitch = new Label();
            label14 = new Label();
            label10 = new Label();
            labelLocation = new Label();
            label12 = new Label();
            labelStuff = new Label();
            groupBoxPlayer.SuspendLayout();
            groupBoxFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMinPitches).BeginInit();
            groupBoxPostProcessing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxScale).BeginInit();
            groupBoxOutput.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panelPitchStats.SuspendLayout();
            groupBox3.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            groupBox2.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            groupBox4.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            SuspendLayout();
            // 
            // pitchPanel
            // 
            pitchPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pitchPanel.Dock = DockStyle.Fill;
            pitchPanel.Location = new Point(3, 239);
            pitchPanel.Name = "pitchPanel";
            pitchPanel.Size = new Size(840, 854);
            pitchPanel.TabIndex = 0;
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
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
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
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(panelPitchStats, 0, 0);
            tableLayoutPanel2.Controls.Add(pitchPanel, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(503, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(846, 1096);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // panelPitchStats
            // 
            panelPitchStats.AutoSize = true;
            panelPitchStats.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelPitchStats.ColumnCount = 4;
            panelPitchStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25.0000057F));
            panelPitchStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panelPitchStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panelPitchStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panelPitchStats.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            panelPitchStats.Controls.Add(labelName, 1, 0);
            panelPitchStats.Controls.Add(groupBox3, 2, 1);
            panelPitchStats.Controls.Add(groupBox1, 0, 1);
            panelPitchStats.Controls.Add(groupBox2, 1, 1);
            panelPitchStats.Controls.Add(groupBox4, 3, 1);
            panelPitchStats.Dock = DockStyle.Fill;
            panelPitchStats.Location = new Point(3, 3);
            panelPitchStats.Name = "panelPitchStats";
            panelPitchStats.RowCount = 2;
            panelPitchStats.RowStyles.Add(new RowStyle());
            panelPitchStats.RowStyles.Add(new RowStyle());
            panelPitchStats.Size = new Size(840, 230);
            panelPitchStats.TabIndex = 5;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Dock = DockStyle.Fill;
            labelName.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelName.Location = new Point(213, 0);
            labelName.Name = "labelName";
            labelName.Size = new Size(203, 30);
            labelName.TabIndex = 0;
            labelName.Text = "Player Name";
            labelName.TextAlign = ContentAlignment.TopCenter;
            // 
            // groupBox3
            // 
            groupBox3.AutoSize = true;
            groupBox3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox3.Controls.Add(tableLayoutPanel5);
            groupBox3.Dock = DockStyle.Top;
            groupBox3.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox3.Location = new Point(422, 33);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(203, 194);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Pitch Rates";
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.AutoSize = true;
            tableLayoutPanel5.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel5.ColumnCount = 2;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel5.Controls.Add(labelInPlay, 1, 3);
            tableLayoutPanel5.Controls.Add(label17, 0, 3);
            tableLayoutPanel5.Controls.Add(labelFoul, 1, 2);
            tableLayoutPanel5.Controls.Add(label15, 0, 2);
            tableLayoutPanel5.Controls.Add(_labelWhiffRate, 0, 0);
            tableLayoutPanel5.Controls.Add(labelCSW, 1, 1);
            tableLayoutPanel5.Controls.Add(_labelCSW, 0, 1);
            tableLayoutPanel5.Controls.Add(labelWhiff, 1, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(3, 31);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 4;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel5.Size = new Size(197, 160);
            tableLayoutPanel5.TabIndex = 1;
            // 
            // labelInPlay
            // 
            labelInPlay.AutoSize = true;
            labelInPlay.Dock = DockStyle.Fill;
            labelInPlay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelInPlay.Location = new Point(101, 120);
            labelInPlay.Name = "labelInPlay";
            labelInPlay.Size = new Size(93, 40);
            labelInPlay.TabIndex = 4;
            labelInPlay.Text = "XX.X%";
            labelInPlay.TextAlign = ContentAlignment.BottomRight;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Dock = DockStyle.Fill;
            label17.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label17.Location = new Point(3, 120);
            label17.Name = "label17";
            label17.Size = new Size(92, 40);
            label17.TabIndex = 3;
            label17.Text = "IP%";
            label17.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelFoul
            // 
            labelFoul.AutoSize = true;
            labelFoul.Dock = DockStyle.Fill;
            labelFoul.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelFoul.Location = new Point(101, 80);
            labelFoul.Name = "labelFoul";
            labelFoul.Size = new Size(93, 40);
            labelFoul.TabIndex = 2;
            labelFoul.Text = "XX.X%";
            labelFoul.TextAlign = ContentAlignment.BottomRight;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Dock = DockStyle.Fill;
            label15.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label15.Location = new Point(3, 80);
            label15.Name = "label15";
            label15.Size = new Size(92, 40);
            label15.TabIndex = 1;
            label15.Text = "Foul%";
            label15.TextAlign = ContentAlignment.BottomLeft;
            // 
            // _labelWhiffRate
            // 
            _labelWhiffRate.AutoSize = true;
            _labelWhiffRate.Dock = DockStyle.Fill;
            _labelWhiffRate.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _labelWhiffRate.Location = new Point(3, 0);
            _labelWhiffRate.Name = "_labelWhiffRate";
            _labelWhiffRate.Size = new Size(92, 40);
            _labelWhiffRate.TabIndex = 0;
            _labelWhiffRate.Text = "Whiff%";
            _labelWhiffRate.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelCSW
            // 
            labelCSW.AutoSize = true;
            labelCSW.Dock = DockStyle.Fill;
            labelCSW.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelCSW.Location = new Point(101, 40);
            labelCSW.Name = "labelCSW";
            labelCSW.Size = new Size(93, 40);
            labelCSW.TabIndex = 0;
            labelCSW.Text = "XX.X%";
            labelCSW.TextAlign = ContentAlignment.BottomRight;
            // 
            // _labelCSW
            // 
            _labelCSW.AutoSize = true;
            _labelCSW.Dock = DockStyle.Fill;
            _labelCSW.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _labelCSW.Location = new Point(3, 40);
            _labelCSW.Name = "_labelCSW";
            _labelCSW.Size = new Size(92, 40);
            _labelCSW.TabIndex = 0;
            _labelCSW.Text = "CSW%";
            _labelCSW.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelWhiff
            // 
            labelWhiff.AutoSize = true;
            labelWhiff.Dock = DockStyle.Fill;
            labelWhiff.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelWhiff.Location = new Point(101, 0);
            labelWhiff.Name = "labelWhiff";
            labelWhiff.Size = new Size(93, 40);
            labelWhiff.TabIndex = 0;
            labelWhiff.Text = "XX.X%";
            labelWhiff.TextAlign = ContentAlignment.BottomRight;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox1.Controls.Add(tableLayoutPanel3);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox1.Location = new Point(3, 33);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(204, 154);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Pitch Metrics";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(labelMoveZ, 1, 2);
            tableLayoutPanel3.Controls.Add(_labelVelocity, 0, 0);
            tableLayoutPanel3.Controls.Add(_labelMoveX, 0, 1);
            tableLayoutPanel3.Controls.Add(_labelMoveZ, 0, 2);
            tableLayoutPanel3.Controls.Add(labelMoveX, 1, 1);
            tableLayoutPanel3.Controls.Add(labelVel, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 31);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel3.Size = new Size(198, 120);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // labelMoveZ
            // 
            labelMoveZ.AutoSize = true;
            labelMoveZ.Dock = DockStyle.Fill;
            labelMoveZ.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelMoveZ.Location = new Point(102, 80);
            labelMoveZ.Name = "labelMoveZ";
            labelMoveZ.Size = new Size(93, 40);
            labelMoveZ.TabIndex = 0;
            labelMoveZ.Text = "XX.X";
            labelMoveZ.TextAlign = ContentAlignment.BottomRight;
            // 
            // _labelVelocity
            // 
            _labelVelocity.AutoSize = true;
            _labelVelocity.Dock = DockStyle.Fill;
            _labelVelocity.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _labelVelocity.Location = new Point(3, 0);
            _labelVelocity.Name = "_labelVelocity";
            _labelVelocity.Size = new Size(93, 40);
            _labelVelocity.TabIndex = 0;
            _labelVelocity.Text = "Velocity";
            _labelVelocity.TextAlign = ContentAlignment.BottomLeft;
            // 
            // _labelMoveX
            // 
            _labelMoveX.AutoSize = true;
            _labelMoveX.Dock = DockStyle.Fill;
            _labelMoveX.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _labelMoveX.Location = new Point(3, 40);
            _labelMoveX.Name = "_labelMoveX";
            _labelMoveX.Size = new Size(93, 40);
            _labelMoveX.TabIndex = 0;
            _labelMoveX.Text = "Move X";
            _labelMoveX.TextAlign = ContentAlignment.BottomLeft;
            // 
            // _labelMoveZ
            // 
            _labelMoveZ.AutoSize = true;
            _labelMoveZ.Dock = DockStyle.Fill;
            _labelMoveZ.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _labelMoveZ.Location = new Point(3, 80);
            _labelMoveZ.Name = "_labelMoveZ";
            _labelMoveZ.Size = new Size(93, 40);
            _labelMoveZ.TabIndex = 0;
            _labelMoveZ.Text = "Move Z";
            _labelMoveZ.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelMoveX
            // 
            labelMoveX.AutoSize = true;
            labelMoveX.Dock = DockStyle.Fill;
            labelMoveX.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelMoveX.Location = new Point(102, 40);
            labelMoveX.Name = "labelMoveX";
            labelMoveX.Size = new Size(93, 40);
            labelMoveX.TabIndex = 0;
            labelMoveX.Text = "XX.X";
            labelMoveX.TextAlign = ContentAlignment.BottomRight;
            // 
            // labelVel
            // 
            labelVel.AutoSize = true;
            labelVel.Dock = DockStyle.Fill;
            labelVel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelVel.Location = new Point(102, 0);
            labelVel.Name = "labelVel";
            labelVel.Size = new Size(93, 40);
            labelVel.TabIndex = 0;
            labelVel.Text = "XX.X";
            labelVel.TextAlign = ContentAlignment.BottomRight;
            // 
            // groupBox2
            // 
            groupBox2.AutoSize = true;
            groupBox2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox2.Controls.Add(tableLayoutPanel4);
            groupBox2.Dock = DockStyle.Top;
            groupBox2.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox2.Location = new Point(213, 33);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(203, 194);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Hit Stats";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.AutoSize = true;
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Controls.Add(label8, 0, 3);
            tableLayoutPanel4.Controls.Add(labelABPA, 1, 0);
            tableLayoutPanel4.Controls.Add(_labelABPA, 0, 0);
            tableLayoutPanel4.Controls.Add(_labelAVG, 0, 1);
            tableLayoutPanel4.Controls.Add(labelAVG, 1, 1);
            tableLayoutPanel4.Controls.Add(label9, 0, 2);
            tableLayoutPanel4.Controls.Add(labelSLG, 1, 3);
            tableLayoutPanel4.Controls.Add(labelOBP, 1, 2);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 31);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 4;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel4.Size = new Size(197, 160);
            tableLayoutPanel4.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Dock = DockStyle.Fill;
            label8.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label8.Location = new Point(3, 120);
            label8.Name = "label8";
            label8.Size = new Size(92, 40);
            label8.TabIndex = 0;
            label8.Text = "SLG";
            label8.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelABPA
            // 
            labelABPA.AutoSize = true;
            labelABPA.Dock = DockStyle.Fill;
            labelABPA.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelABPA.Location = new Point(101, 0);
            labelABPA.Name = "labelABPA";
            labelABPA.Size = new Size(93, 40);
            labelABPA.TabIndex = 0;
            labelABPA.Text = "XX/YY";
            labelABPA.TextAlign = ContentAlignment.BottomRight;
            // 
            // _labelABPA
            // 
            _labelABPA.AutoSize = true;
            _labelABPA.Dock = DockStyle.Fill;
            _labelABPA.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _labelABPA.Location = new Point(3, 0);
            _labelABPA.Name = "_labelABPA";
            _labelABPA.Size = new Size(92, 40);
            _labelABPA.TabIndex = 0;
            _labelABPA.Text = "AB/PA";
            _labelABPA.TextAlign = ContentAlignment.BottomLeft;
            // 
            // _labelAVG
            // 
            _labelAVG.AutoSize = true;
            _labelAVG.Dock = DockStyle.Fill;
            _labelAVG.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _labelAVG.Location = new Point(3, 40);
            _labelAVG.Name = "_labelAVG";
            _labelAVG.Size = new Size(92, 40);
            _labelAVG.TabIndex = 0;
            _labelAVG.Text = "AVG";
            _labelAVG.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelAVG
            // 
            labelAVG.AutoSize = true;
            labelAVG.Dock = DockStyle.Fill;
            labelAVG.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelAVG.Location = new Point(101, 40);
            labelAVG.Name = "labelAVG";
            labelAVG.Size = new Size(93, 40);
            labelAVG.TabIndex = 0;
            labelAVG.Text = "XXX";
            labelAVG.TextAlign = ContentAlignment.BottomRight;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Dock = DockStyle.Fill;
            label9.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label9.Location = new Point(3, 80);
            label9.Name = "label9";
            label9.Size = new Size(92, 40);
            label9.TabIndex = 0;
            label9.Text = "OBP";
            label9.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelSLG
            // 
            labelSLG.AutoSize = true;
            labelSLG.Dock = DockStyle.Fill;
            labelSLG.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelSLG.Location = new Point(101, 120);
            labelSLG.Name = "labelSLG";
            labelSLG.Size = new Size(93, 40);
            labelSLG.TabIndex = 0;
            labelSLG.Text = "XXX";
            labelSLG.TextAlign = ContentAlignment.BottomRight;
            // 
            // labelOBP
            // 
            labelOBP.AutoSize = true;
            labelOBP.Dock = DockStyle.Fill;
            labelOBP.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelOBP.Location = new Point(101, 80);
            labelOBP.Name = "labelOBP";
            labelOBP.Size = new Size(93, 40);
            labelOBP.TabIndex = 0;
            labelOBP.Text = "XXX";
            labelOBP.TextAlign = ContentAlignment.BottomRight;
            // 
            // groupBox4
            // 
            groupBox4.AutoSize = true;
            groupBox4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox4.Controls.Add(tableLayoutPanel6);
            groupBox4.Dock = DockStyle.Top;
            groupBox4.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox4.Location = new Point(631, 33);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(206, 194);
            groupBox4.TabIndex = 5;
            groupBox4.TabStop = false;
            groupBox4.Text = "Modeling";
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.AutoSize = true;
            tableLayoutPanel6.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel6.ColumnCount = 2;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.Controls.Add(labelActual, 1, 3);
            tableLayoutPanel6.Controls.Add(label11, 0, 3);
            tableLayoutPanel6.Controls.Add(labelPitch, 1, 2);
            tableLayoutPanel6.Controls.Add(label14, 0, 2);
            tableLayoutPanel6.Controls.Add(label10, 0, 0);
            tableLayoutPanel6.Controls.Add(labelLocation, 1, 1);
            tableLayoutPanel6.Controls.Add(label12, 0, 1);
            tableLayoutPanel6.Controls.Add(labelStuff, 1, 0);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(3, 31);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 4;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel6.Size = new Size(200, 160);
            tableLayoutPanel6.TabIndex = 1;
            // 
            // labelActual
            // 
            labelActual.AutoSize = true;
            labelActual.Dock = DockStyle.Fill;
            labelActual.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelActual.Location = new Point(103, 120);
            labelActual.Name = "labelActual";
            labelActual.Size = new Size(94, 40);
            labelActual.TabIndex = 4;
            labelActual.Text = "XXX.X";
            labelActual.TextAlign = ContentAlignment.BottomRight;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Dock = DockStyle.Fill;
            label11.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label11.Location = new Point(3, 120);
            label11.Name = "label11";
            label11.Size = new Size(94, 40);
            label11.TabIndex = 3;
            label11.Text = "Actual";
            label11.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelPitch
            // 
            labelPitch.AutoSize = true;
            labelPitch.Dock = DockStyle.Fill;
            labelPitch.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPitch.Location = new Point(103, 80);
            labelPitch.Name = "labelPitch";
            labelPitch.Size = new Size(94, 40);
            labelPitch.TabIndex = 2;
            labelPitch.Text = "XXX.X";
            labelPitch.TextAlign = ContentAlignment.BottomRight;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Dock = DockStyle.Fill;
            label14.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label14.Location = new Point(3, 80);
            label14.Name = "label14";
            label14.Size = new Size(94, 40);
            label14.TabIndex = 1;
            label14.Text = "Pitch";
            label14.TextAlign = ContentAlignment.BottomLeft;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Dock = DockStyle.Fill;
            label10.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label10.Location = new Point(3, 0);
            label10.Name = "label10";
            label10.Size = new Size(94, 40);
            label10.TabIndex = 0;
            label10.Text = "Stuff";
            label10.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelLocation
            // 
            labelLocation.AutoSize = true;
            labelLocation.Dock = DockStyle.Fill;
            labelLocation.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelLocation.Location = new Point(103, 40);
            labelLocation.Name = "labelLocation";
            labelLocation.Size = new Size(94, 40);
            labelLocation.TabIndex = 0;
            labelLocation.Text = "XXX.X";
            labelLocation.TextAlign = ContentAlignment.BottomRight;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Dock = DockStyle.Fill;
            label12.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label12.Location = new Point(3, 40);
            label12.Name = "label12";
            label12.Size = new Size(94, 40);
            label12.TabIndex = 0;
            label12.Text = "Location";
            label12.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelStuff
            // 
            labelStuff.AutoSize = true;
            labelStuff.Dock = DockStyle.Fill;
            labelStuff.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelStuff.Location = new Point(103, 0);
            labelStuff.Name = "labelStuff";
            labelStuff.Size = new Size(94, 40);
            labelStuff.TabIndex = 0;
            labelStuff.Text = "XXX.X";
            labelStuff.TextAlign = ContentAlignment.BottomRight;
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
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            panelPitchStats.ResumeLayout(false);
            panelPitchStats.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            tableLayoutPanel6.ResumeLayout(false);
            tableLayoutPanel6.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pitchDrawingPanel;
        private PitchPanel pitchPanel;
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
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel panelPitchStats;
        private Label labelName;
        private GroupBox groupBox3;
        private Label _labelCSW;
        private Label labelCSW;
        private Label labelWhiff;
        private Label _labelWhiffRate;
        private GroupBox groupBox1;
        private Label _labelMoveZ;
        private Label _labelMoveX;
        private Label labelMoveZ;
        private Label labelMoveX;
        private Label labelVel;
        private Label _labelVelocity;
        private GroupBox groupBox2;
        private Label label8;
        private Label label9;
        private Label _labelAVG;
        private Label _labelABPA;
        private Label labelSLG;
        private Label labelOBP;
        private Label labelAVG;
        private Label labelABPA;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel4;
        private GroupBox groupBox4;
        private TableLayoutPanel tableLayoutPanel6;
        private Label label10;
        private Label labelLocation;
        private Label label12;
        private Label labelStuff;
        private Label labelPitch;
        private Label label14;
        private Controls.PlayerSearchBar playerSearchBar;
        private Label labelActual;
        private Label label11;
        private Label label13;
        private ComboBox cbLevel;
        private Label labelInPlay;
        private Label label17;
        private Label labelFoul;
        private Label label15;
        private Controls.PitcherArsenal pitcherArsenal;
    }
}
