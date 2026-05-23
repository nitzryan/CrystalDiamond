namespace UI.Controls
{
    partial class PlayerSearchBar
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
            tbPlayerName = new TextBox();
            lbNameList = new ListBox();
            panel1 = new Panel();
            nudPlayerId = new NumericUpDown();
            btnSearch = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPlayerId).BeginInit();
            SuspendLayout();
            // 
            // tbPlayerName
            // 
            tbPlayerName.Location = new Point(3, 35);
            tbPlayerName.Name = "tbPlayerName";
            tbPlayerName.Size = new Size(276, 23);
            tbPlayerName.TabIndex = 0;
            tbPlayerName.TextChanged += tbPlayerName_TextChanged;
            // 
            // lbNameList
            // 
            lbNameList.Dock = DockStyle.Fill;
            lbNameList.FormattingEnabled = true;
            lbNameList.ItemHeight = 15;
            lbNameList.Location = new Point(0, 0);
            lbNameList.Name = "lbNameList";
            lbNameList.Size = new Size(276, 109);
            lbNameList.TabIndex = 1;
            lbNameList.SelectedIndexChanged += lbNameList_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(lbNameList);
            panel1.Location = new Point(3, 66);
            panel1.Name = "panel1";
            panel1.Size = new Size(276, 109);
            panel1.TabIndex = 2;
            // 
            // nudPlayerId
            // 
            nudPlayerId.Location = new Point(3, 8);
            nudPlayerId.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            nudPlayerId.Name = "nudPlayerId";
            nudPlayerId.Size = new Size(214, 23);
            nudPlayerId.TabIndex = 3;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(222, 8);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(57, 23);
            btnSearch.TabIndex = 4;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // PlayerSearchBar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnSearch);
            Controls.Add(nudPlayerId);
            Controls.Add(panel1);
            Controls.Add(tbPlayerName);
            Name = "PlayerSearchBar";
            Size = new Size(282, 178);
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudPlayerId).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbPlayerName;
        private ListBox lbNameList;
        private Panel panel1;
        private NumericUpDown nudPlayerId;
        private Button btnSearch;
    }
}
