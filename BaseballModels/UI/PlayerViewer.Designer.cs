using UI.Controls;

namespace UI
{
    public class NoAutoScrollPanel : FlowLayoutPanel
    {
        // Needed so first button press to delete row from table works
        protected override Point ScrollToControl(Control activeControl) => DisplayRectangle.Location;
    }

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
            flpContent = new NoAutoScrollPanel();
            playerSearchBar = new PlayerSearchBar();
            btnModelData = new Button();
            tblModelPlayers = new EntityTableView();
            tblHitterMonthStats = new EntityTableView();
            tblPitcherMonthStats = new EntityTableView();
            flpContent.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(10, 8, 0, 0);
            lblTitle.Size = new Size(1200, 45);
            lblTitle.TabIndex = 1;
            // 
            // flpContent
            // 
            flpContent.AutoScroll = true;
            flpContent.Controls.Add(playerSearchBar);
            flpContent.Controls.Add(btnModelData);
            flpContent.Controls.Add(tblModelPlayers);
            flpContent.Controls.Add(tblHitterMonthStats);
            flpContent.Controls.Add(tblPitcherMonthStats);
            flpContent.Dock = DockStyle.Fill;
            flpContent.FlowDirection = FlowDirection.TopDown;
            flpContent.Location = new Point(0, 45);
            flpContent.Name = "flpContent";
            flpContent.Size = new Size(1200, 755);
            flpContent.TabIndex = 0;
            flpContent.WrapContents = false;
            // 
            // playerSearchBar
            // 
            playerSearchBar.Location = new Point(3, 3);
            playerSearchBar.Name = "playerSearchBar";
            playerSearchBar.Size = new Size(282, 178);
            playerSearchBar.TabIndex = 0;
            // 
            // btnModelData
            // 
            btnModelData.Location = new Point(3, 187);
            btnModelData.Name = "btnModelData";
            btnModelData.Size = new Size(177, 23);
            btnModelData.TabIndex = 4;
            btnModelData.Text = "Get Model Data";
            btnModelData.UseVisualStyleBackColor = true;
            btnModelData.Click += btnModelData_Click;
            // 
            // tblModelPlayers
            // 
            tblModelPlayers.Location = new Point(3, 216);
            tblModelPlayers.Name = "tblModelPlayers";
            tblModelPlayers.Size = new Size(1150, 140);
            tblModelPlayers.TabIndex = 1;
            tblModelPlayers.Visible = false;
            // 
            // tblHitterMonthStats
            // 
            tblHitterMonthStats.Location = new Point(3, 362);
            tblHitterMonthStats.Name = "tblHitterMonthStats";
            tblHitterMonthStats.Size = new Size(1150, 260);
            tblHitterMonthStats.TabIndex = 2;
            tblHitterMonthStats.Visible = false;
            // 
            // tblPitcherMonthStats
            // 
            tblPitcherMonthStats.Location = new Point(3, 628);
            tblPitcherMonthStats.Name = "tblPitcherMonthStats";
            tblPitcherMonthStats.Size = new Size(1150, 260);
            tblPitcherMonthStats.TabIndex = 3;
            tblPitcherMonthStats.Visible = false;
            // 
            // PlayerViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(flpContent);
            Controls.Add(lblTitle);
            Name = "PlayerViewer";
            Text = "Player Viewer";
            flpContent.ResumeLayout(false);
            ResumeLayout(false);
        }
        private Label lblTitle;
        private PlayerSearchBar playerSearchBar;
        private EntityTableView tblModelPlayers;
        private EntityTableView tblHitterMonthStats;
        private EntityTableView tblPitcherMonthStats;
        private Button btnModelData;
        private NoAutoScrollPanel flpContent;
    }
}