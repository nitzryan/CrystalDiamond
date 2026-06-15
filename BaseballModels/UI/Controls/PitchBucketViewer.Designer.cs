namespace UI.Controls
{
    partial class PitchBucketViewer
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
            labelStuff = new Label();
            pitchPanel = new PitchPanel();
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
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(panelPitchStats, 0, 0);
            tableLayoutPanel2.Controls.Add(pitchPanel, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(1015, 828);
            tableLayoutPanel2.TabIndex = 4;
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
            panelPitchStats.Size = new Size(1009, 230);
            panelPitchStats.TabIndex = 5;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Dock = DockStyle.Fill;
            labelName.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelName.Location = new Point(255, 0);
            labelName.Name = "labelName";
            labelName.Size = new Size(246, 30);
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
            groupBox3.Location = new Point(507, 33);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(246, 194);
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
            tableLayoutPanel5.Size = new Size(240, 160);
            tableLayoutPanel5.TabIndex = 1;
            // 
            // labelInPlay
            // 
            labelInPlay.AutoSize = true;
            labelInPlay.Dock = DockStyle.Fill;
            labelInPlay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelInPlay.Location = new Point(123, 120);
            labelInPlay.Name = "labelInPlay";
            labelInPlay.Size = new Size(114, 40);
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
            label17.Size = new Size(114, 40);
            label17.TabIndex = 3;
            label17.Text = "IP%";
            label17.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelFoul
            // 
            labelFoul.AutoSize = true;
            labelFoul.Dock = DockStyle.Fill;
            labelFoul.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelFoul.Location = new Point(123, 80);
            labelFoul.Name = "labelFoul";
            labelFoul.Size = new Size(114, 40);
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
            label15.Size = new Size(114, 40);
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
            _labelWhiffRate.Size = new Size(114, 40);
            _labelWhiffRate.TabIndex = 0;
            _labelWhiffRate.Text = "Whiff%";
            _labelWhiffRate.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelCSW
            // 
            labelCSW.AutoSize = true;
            labelCSW.Dock = DockStyle.Fill;
            labelCSW.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelCSW.Location = new Point(123, 40);
            labelCSW.Name = "labelCSW";
            labelCSW.Size = new Size(114, 40);
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
            _labelCSW.Size = new Size(114, 40);
            _labelCSW.TabIndex = 0;
            _labelCSW.Text = "CSW%";
            _labelCSW.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelWhiff
            // 
            labelWhiff.AutoSize = true;
            labelWhiff.Dock = DockStyle.Fill;
            labelWhiff.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelWhiff.Location = new Point(123, 0);
            labelWhiff.Name = "labelWhiff";
            labelWhiff.Size = new Size(114, 40);
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
            groupBox1.Size = new Size(246, 154);
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
            tableLayoutPanel3.Size = new Size(240, 120);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // labelMoveZ
            // 
            labelMoveZ.AutoSize = true;
            labelMoveZ.Dock = DockStyle.Fill;
            labelMoveZ.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelMoveZ.Location = new Point(123, 80);
            labelMoveZ.Name = "labelMoveZ";
            labelMoveZ.Size = new Size(114, 40);
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
            _labelVelocity.Size = new Size(114, 40);
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
            _labelMoveX.Size = new Size(114, 40);
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
            _labelMoveZ.Size = new Size(114, 40);
            _labelMoveZ.TabIndex = 0;
            _labelMoveZ.Text = "Move Z";
            _labelMoveZ.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelMoveX
            // 
            labelMoveX.AutoSize = true;
            labelMoveX.Dock = DockStyle.Fill;
            labelMoveX.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelMoveX.Location = new Point(123, 40);
            labelMoveX.Name = "labelMoveX";
            labelMoveX.Size = new Size(114, 40);
            labelMoveX.TabIndex = 0;
            labelMoveX.Text = "XX.X";
            labelMoveX.TextAlign = ContentAlignment.BottomRight;
            // 
            // labelVel
            // 
            labelVel.AutoSize = true;
            labelVel.Dock = DockStyle.Fill;
            labelVel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelVel.Location = new Point(123, 0);
            labelVel.Name = "labelVel";
            labelVel.Size = new Size(114, 40);
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
            groupBox2.Location = new Point(255, 33);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(246, 194);
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
            tableLayoutPanel4.Size = new Size(240, 160);
            tableLayoutPanel4.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Dock = DockStyle.Fill;
            label8.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label8.Location = new Point(3, 120);
            label8.Name = "label8";
            label8.Size = new Size(114, 40);
            label8.TabIndex = 0;
            label8.Text = "SLG";
            label8.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelABPA
            // 
            labelABPA.AutoSize = true;
            labelABPA.Dock = DockStyle.Fill;
            labelABPA.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelABPA.Location = new Point(123, 0);
            labelABPA.Name = "labelABPA";
            labelABPA.Size = new Size(114, 40);
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
            _labelABPA.Size = new Size(114, 40);
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
            _labelAVG.Size = new Size(114, 40);
            _labelAVG.TabIndex = 0;
            _labelAVG.Text = "AVG";
            _labelAVG.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelAVG
            // 
            labelAVG.AutoSize = true;
            labelAVG.Dock = DockStyle.Fill;
            labelAVG.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelAVG.Location = new Point(123, 40);
            labelAVG.Name = "labelAVG";
            labelAVG.Size = new Size(114, 40);
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
            label9.Size = new Size(114, 40);
            label9.TabIndex = 0;
            label9.Text = "OBP";
            label9.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelSLG
            // 
            labelSLG.AutoSize = true;
            labelSLG.Dock = DockStyle.Fill;
            labelSLG.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelSLG.Location = new Point(123, 120);
            labelSLG.Name = "labelSLG";
            labelSLG.Size = new Size(114, 40);
            labelSLG.TabIndex = 0;
            labelSLG.Text = "XXX";
            labelSLG.TextAlign = ContentAlignment.BottomRight;
            // 
            // labelOBP
            // 
            labelOBP.AutoSize = true;
            labelOBP.Dock = DockStyle.Fill;
            labelOBP.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelOBP.Location = new Point(123, 80);
            labelOBP.Name = "labelOBP";
            labelOBP.Size = new Size(114, 40);
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
            groupBox4.Location = new Point(759, 33);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(247, 154);
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
            tableLayoutPanel6.Controls.Add(labelActual, 1, 2);
            tableLayoutPanel6.Controls.Add(label11, 0, 2);
            tableLayoutPanel6.Controls.Add(labelPitch, 1, 1);
            tableLayoutPanel6.Controls.Add(label14, 0, 1);
            tableLayoutPanel6.Controls.Add(label10, 0, 0);
            tableLayoutPanel6.Controls.Add(labelStuff, 1, 0);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(3, 31);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 3;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel6.Size = new Size(241, 120);
            tableLayoutPanel6.TabIndex = 1;
            // 
            // labelActual
            // 
            labelActual.AutoSize = true;
            labelActual.Dock = DockStyle.Fill;
            labelActual.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelActual.Location = new Point(123, 80);
            labelActual.Name = "labelActual";
            labelActual.Size = new Size(115, 40);
            labelActual.TabIndex = 4;
            labelActual.Text = "XXX.X";
            labelActual.TextAlign = ContentAlignment.BottomRight;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Dock = DockStyle.Fill;
            label11.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label11.Location = new Point(3, 80);
            label11.Name = "label11";
            label11.Size = new Size(114, 40);
            label11.TabIndex = 3;
            label11.Text = "Actual";
            label11.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelPitch
            // 
            labelPitch.AutoSize = true;
            labelPitch.Dock = DockStyle.Fill;
            labelPitch.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPitch.Location = new Point(123, 40);
            labelPitch.Name = "labelPitch";
            labelPitch.Size = new Size(115, 40);
            labelPitch.TabIndex = 2;
            labelPitch.Text = "XXX.X";
            labelPitch.TextAlign = ContentAlignment.BottomRight;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Dock = DockStyle.Fill;
            label14.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label14.Location = new Point(3, 40);
            label14.Name = "label14";
            label14.Size = new Size(114, 40);
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
            label10.Size = new Size(114, 40);
            label10.TabIndex = 0;
            label10.Text = "Stuff";
            label10.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelStuff
            // 
            labelStuff.AutoSize = true;
            labelStuff.Dock = DockStyle.Fill;
            labelStuff.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelStuff.Location = new Point(123, 0);
            labelStuff.Name = "labelStuff";
            labelStuff.Size = new Size(115, 40);
            labelStuff.TabIndex = 0;
            labelStuff.Text = "XXX.X";
            labelStuff.TextAlign = ContentAlignment.BottomRight;
            // 
            // pitchPanel
            // 
            pitchPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pitchPanel.BackColor = SystemColors.Control;
            pitchPanel.Dock = DockStyle.Fill;
            pitchPanel.Location = new Point(3, 239);
            pitchPanel.Name = "pitchPanel";
            pitchPanel.Size = new Size(1009, 586);
            pitchPanel.TabIndex = 0;
            // 
            // PitchBucketViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel2);
            Name = "PitchBucketViewer";
            Size = new Size(1015, 828);
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
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel panelPitchStats;
        private Label labelName;
        private GroupBox groupBox3;
        private TableLayoutPanel tableLayoutPanel5;
        private Label labelInPlay;
        private Label label17;
        private Label labelFoul;
        private Label label15;
        private Label _labelWhiffRate;
        private Label labelCSW;
        private Label _labelCSW;
        private Label labelWhiff;
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel3;
        private Label labelMoveZ;
        private Label _labelVelocity;
        private Label _labelMoveX;
        private Label _labelMoveZ;
        private Label labelMoveX;
        private Label labelVel;
        private GroupBox groupBox2;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label8;
        private Label labelABPA;
        private Label _labelABPA;
        private Label _labelAVG;
        private Label labelAVG;
        private Label label9;
        private Label labelSLG;
        private Label labelOBP;
        private GroupBox groupBox4;
        private TableLayoutPanel tableLayoutPanel6;
        private Label labelActual;
        private Label label11;
        private Label labelPitch;
        private Label label14;
        private Label label10;
        private Label labelStuff;
        private PitchPanel pitchPanel;
    }
}
