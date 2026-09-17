using UI.Controls;
using UI.Controls.TestRunnerGraphViewer;

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
            tblModelPlayers = new EntityTableView();
            tblHitterMonthStats = new EntityTableView();
            tblPitcherMonthStats = new EntityTableView();
            playerSearchBar = new PlayerSearchBar();
            btnModelData = new Button();
            tlpMain = new TableLayoutPanel();
            tlpHeader = new TableLayoutPanel();
            modelResultsPanel = new ModelResultsPanel();
            flpContent.SuspendLayout();
            tlpMain.SuspendLayout();
            tlpHeader.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.None;
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(520, 191);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(0, 8, 0, 0);
            lblTitle.Size = new Size(159, 40);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Player Name";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // flpContent
            // 
            flpContent.AutoScroll = true;
            flpContent.Controls.Add(tblModelPlayers);
            flpContent.Controls.Add(tblHitterMonthStats);
            flpContent.Controls.Add(tblPitcherMonthStats);
            flpContent.Dock = DockStyle.Fill;
            flpContent.FlowDirection = FlowDirection.TopDown;
            flpContent.Location = new Point(0, 231);
            flpContent.Margin = new Padding(0);
            flpContent.Name = "flpContent";
            flpContent.Size = new Size(1200, 569);
            flpContent.TabIndex = 2;
            flpContent.WrapContents = false;
            // 
            // tblModelPlayers
            // 
            tblModelPlayers.Location = new Point(3, 3);
            tblModelPlayers.Name = "tblModelPlayers";
            tblModelPlayers.Size = new Size(1150, 140);
            tblModelPlayers.TabIndex = 1;
            tblModelPlayers.Visible = false;
            // 
            // tblHitterMonthStats
            // 
            tblHitterMonthStats.Location = new Point(3, 149);
            tblHitterMonthStats.Name = "tblHitterMonthStats";
            tblHitterMonthStats.Size = new Size(1150, 260);
            tblHitterMonthStats.TabIndex = 2;
            tblHitterMonthStats.Visible = false;
            // 
            // tblPitcherMonthStats
            // 
            tblPitcherMonthStats.Location = new Point(3, 415);
            tblPitcherMonthStats.Name = "tblPitcherMonthStats";
            tblPitcherMonthStats.Size = new Size(1150, 260);
            tblPitcherMonthStats.TabIndex = 3;
            tblPitcherMonthStats.Visible = false;
            // 
            // playerSearchBar
            // 
            playerSearchBar.Location = new Point(3, 3);
            playerSearchBar.Name = "playerSearchBar";
            tlpHeader.SetRowSpan(playerSearchBar, 2);
            playerSearchBar.Size = new Size(282, 178);
            playerSearchBar.TabIndex = 0;
            // 
            // btnModelData
            // 
            btnModelData.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnModelData.Location = new Point(1014, 159);
            btnModelData.Name = "btnModelData";
            btnModelData.Size = new Size(177, 23);
            btnModelData.TabIndex = 2;
            btnModelData.Text = "Get Model Data";
            btnModelData.UseVisualStyleBackColor = true;
            btnModelData.Click += btnModelData_Click;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(tlpHeader, 0, 0);
            tlpMain.Controls.Add(lblTitle, 0, 1);
            tlpMain.Controls.Add(flpContent, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle());
            tlpMain.RowStyles.Add(new RowStyle());
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Size = new Size(1200, 800);
            tlpMain.TabIndex = 0;
            // 
            // tlpHeader
            // 
            tlpHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tlpHeader.AutoSize = true;
            tlpHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpHeader.ColumnCount = 2;
            tlpHeader.ColumnStyles.Add(new ColumnStyle());
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.Controls.Add(playerSearchBar, 0, 0);
            tlpHeader.Controls.Add(modelResultsPanel, 1, 0);
            tlpHeader.Controls.Add(btnModelData, 1, 1);
            tlpHeader.Location = new Point(3, 3);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.RowCount = 2;
            tlpHeader.RowStyles.Add(new RowStyle());
            tlpHeader.RowStyles.Add(new RowStyle());
            tlpHeader.Size = new Size(1194, 185);
            tlpHeader.TabIndex = 0;
            // 
            // modelResultsPanel
            // 
            modelResultsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            modelResultsPanel.Name = "modelResultsPanel";
            modelResultsPanel.Size = new Size(750, 300);
            modelResultsPanel.TabIndex = 1;
            // 
            // PlayerViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(tlpMain);
            Name = "PlayerViewer";
            Text = "Player Viewer";
            flpContent.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            tlpHeader.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Label lblTitle;
        private PlayerSearchBar playerSearchBar;
        private EntityTableView tblModelPlayers;
        private EntityTableView tblHitterMonthStats;
        private EntityTableView tblPitcherMonthStats;
        private Button btnModelData;
        private NoAutoScrollPanel flpContent;
        private TableLayoutPanel tlpMain;
        private TableLayoutPanel tlpHeader;
        private ModelResultsPanel modelResultsPanel;
    }
}