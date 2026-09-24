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
            tblBaserunningStats = new EntityTableView();
            tblFieldingStats = new EntityTableView();
            playerSearchBar = new PlayerSearchBar();
            btnHitterModelData = new Button();
            btnPitcherModelData = new Button();
            flpModelButtons = new FlowLayoutPanel();
            tlpMain = new TableLayoutPanel();
            tlpHeader = new TableLayoutPanel();
            modelResultsPanel = new ModelResultsPanel();
            flpContent.SuspendLayout();
            flpModelButtons.SuspendLayout();
            tlpMain.SuspendLayout();
            tlpHeader.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.None;
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(553, 341);
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
            flpContent.Controls.Add(tblBaserunningStats);
            flpContent.Controls.Add(tblFieldingStats);
            flpContent.Dock = DockStyle.Fill;
            flpContent.FlowDirection = FlowDirection.TopDown;
            flpContent.Location = new Point(0, 381);
            flpContent.Margin = new Padding(0);
            flpContent.Name = "flpContent";
            flpContent.Size = new Size(1265, 503);
            flpContent.TabIndex = 2;
            flpContent.WrapContents = false;
            // 
            // tblModelPlayers
            // 
            tblModelPlayers.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tblModelPlayers.Location = new Point(3, 3);
            tblModelPlayers.Name = "tblModelPlayers";
            tblModelPlayers.Size = new Size(1243, 200);
            tblModelPlayers.TabIndex = 1;
            tblModelPlayers.Visible = false;
            // 
            // tblHitterMonthStats
            // 
            tblHitterMonthStats.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tblHitterMonthStats.Location = new Point(3, 209);
            tblHitterMonthStats.Name = "tblHitterMonthStats";
            tblHitterMonthStats.Size = new Size(1000, 200);
            tblHitterMonthStats.TabIndex = 2;
            tblHitterMonthStats.Visible = false;
            // 
            // tblPitcherMonthStats
            // 
            tblPitcherMonthStats.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tblPitcherMonthStats.Location = new Point(3, 415);
            tblPitcherMonthStats.Name = "tblPitcherMonthStats";
            tblPitcherMonthStats.Size = new Size(1000, 200);
            tblPitcherMonthStats.TabIndex = 3;
            tblPitcherMonthStats.Visible = false;
            // 
            // tblBaserunningStats
            // 
            tblBaserunningStats.Location = new Point(3, 621);
            tblBaserunningStats.Name = "tblBaserunningStats";
            tblBaserunningStats.Size = new Size(1000, 200);
            tblBaserunningStats.TabIndex = 4;
            tblBaserunningStats.Visible = false;
            // 
            // tblFieldingStats
            // 
            tblFieldingStats.Location = new Point(3, 827);
            tblFieldingStats.Name = "tblFieldingStats";
            tblFieldingStats.Size = new Size(1000, 200);
            tblFieldingStats.TabIndex = 5;
            tblFieldingStats.Visible = false;
            // 
            // playerSearchBar
            // 
            playerSearchBar.Location = new Point(3, 3);
            playerSearchBar.Name = "playerSearchBar";
            tlpHeader.SetRowSpan(playerSearchBar, 2);
            playerSearchBar.Size = new Size(282, 178);
            playerSearchBar.TabIndex = 0;
            // 
            // btnHitterModelData
            // 
            btnHitterModelData.Enabled = false;
            btnHitterModelData.Location = new Point(3, 3);
            btnHitterModelData.Name = "btnHitterModelData";
            btnHitterModelData.Size = new Size(177, 23);
            btnHitterModelData.TabIndex = 0;
            btnHitterModelData.Text = "Get Hitter Model Data";
            btnHitterModelData.UseVisualStyleBackColor = true;
            btnHitterModelData.Click += btnHitterModelData_Click;
            // 
            // btnPitcherModelData
            // 
            btnPitcherModelData.Enabled = false;
            btnPitcherModelData.Location = new Point(186, 3);
            btnPitcherModelData.Name = "btnPitcherModelData";
            btnPitcherModelData.Size = new Size(177, 23);
            btnPitcherModelData.TabIndex = 1;
            btnPitcherModelData.Text = "Get Pitcher Model Data";
            btnPitcherModelData.UseVisualStyleBackColor = true;
            btnPitcherModelData.Click += btnPitcherModelData_Click;
            // 
            // flpModelButtons
            // 
            flpModelButtons.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flpModelButtons.AutoSize = true;
            flpModelButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpModelButtons.Controls.Add(btnHitterModelData);
            flpModelButtons.Controls.Add(btnPitcherModelData);
            flpModelButtons.Location = new Point(893, 306);
            flpModelButtons.Margin = new Padding(0);
            flpModelButtons.Name = "flpModelButtons";
            flpModelButtons.Size = new Size(366, 29);
            flpModelButtons.TabIndex = 2;
            flpModelButtons.WrapContents = false;
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
            tlpMain.Size = new Size(1265, 884);
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
            tlpHeader.Controls.Add(flpModelButtons, 1, 1);
            tlpHeader.Location = new Point(3, 3);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.RowCount = 2;
            tlpHeader.RowStyles.Add(new RowStyle());
            tlpHeader.RowStyles.Add(new RowStyle());
            tlpHeader.Size = new Size(1259, 335);
            tlpHeader.TabIndex = 0;
            // 
            // modelResultsPanel
            // 
            modelResultsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            modelResultsPanel.Location = new Point(291, 3);
            modelResultsPanel.Name = "modelResultsPanel";
            modelResultsPanel.Size = new Size(965, 300);
            modelResultsPanel.TabIndex = 1;
            // 
            // PlayerViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1265, 884);
            Controls.Add(tlpMain);
            Name = "PlayerViewer";
            Text = "Player Viewer";
            flpContent.ResumeLayout(false);
            flpModelButtons.ResumeLayout(false);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            ResumeLayout(false);
        }

        private Label lblTitle;
        private PlayerSearchBar playerSearchBar;
        private EntityTableView tblModelPlayers;
        private EntityTableView tblHitterMonthStats;
        private EntityTableView tblPitcherMonthStats;
        private Button btnHitterModelData;
        private Button btnPitcherModelData;
        private FlowLayoutPanel flpModelButtons;
        private NoAutoScrollPanel flpContent;
        private TableLayoutPanel tlpMain;
        private TableLayoutPanel tlpHeader;
        private ModelResultsPanel modelResultsPanel;
        private EntityTableView tblBaserunningStats;
        private EntityTableView tblFieldingStats;
    }
}