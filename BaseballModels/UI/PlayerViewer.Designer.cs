using UI.Controls;

namespace UI
{
    partial class PlayerViewer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            flpContent = new FlowLayoutPanel();
            playerSearchBar = new PlayerSearchBar();
            tblModelPlayers = new EntityTableView();
            tblHitterMonthStats = new EntityTableView();
            tblPitcherMonthStats = new EntityTableView();
            flpContent.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle  (pinned to the very top of the page)
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Height = 45;
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(10, 8, 0, 0);
            lblTitle.Text = "";
            // 
            // flpContent
            // 
            flpContent.AutoScroll = true;
            flpContent.Controls.Add(playerSearchBar);
            flpContent.Controls.Add(tblModelPlayers);
            flpContent.Controls.Add(tblHitterMonthStats);
            flpContent.Controls.Add(tblPitcherMonthStats);
            flpContent.Dock = DockStyle.Fill;
            flpContent.FlowDirection = FlowDirection.TopDown;
            flpContent.Name = "flpContent";
            flpContent.WrapContents = false;
            // 
            // playerSearchBar
            // 
            playerSearchBar.Name = "playerSearchBar";
            playerSearchBar.TabIndex = 0;
            // 
            // tblModelPlayers
            // 
            tblModelPlayers.Name = "tblModelPlayers";
            tblModelPlayers.Size = new Size(1150, 140);
            tblModelPlayers.Visible = false;
            // 
            // tblHitterMonthStats
            // 
            tblHitterMonthStats.Name = "tblHitterMonthStats";
            tblHitterMonthStats.Size = new Size(1150, 260);
            tblHitterMonthStats.Visible = false;
            // 
            // tblPitcherMonthStats
            // 
            tblPitcherMonthStats.Name = "tblPitcherMonthStats";
            tblPitcherMonthStats.Size = new Size(1150, 260);
            tblPitcherMonthStats.Visible = false;
            // 
            // PlayerViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(flpContent);   // added first so Fill yields to the Top label
            Controls.Add(lblTitle);
            Name = "PlayerViewer";
            Text = "Player Viewer";
            flpContent.ResumeLayout(false);
            ResumeLayout(false);
        }
        private Label lblTitle;
        private FlowLayoutPanel flpContent;
        private PlayerSearchBar playerSearchBar;
        private EntityTableView tblModelPlayers;
        private EntityTableView tblHitterMonthStats;
        private EntityTableView tblPitcherMonthStats;
    }
}