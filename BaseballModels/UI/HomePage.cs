namespace UI
{
    public class HomePage : Form
    {
        private readonly FlowLayoutPanel _nav;
        private readonly Panel _content;
        private readonly Label _welcome;

        public HomePage()
        {
            Text = "Home";
            ClientSize = new Size(1100, 750);
            StartPosition = FormStartPosition.CenterScreen;

            _nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(4),
                WrapContents = false
            };

            _welcome = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Select a page above."
            };

            _content = new Panel { Dock = DockStyle.Fill };
            _content.Controls.Add(_welcome);   // first in z-order = on top initially

            Controls.Add(_content);            // content first so the nav bar docks above it
            Controls.Add(_nav);

            var homeButton = new Button { Text = "Home", AutoSize = true };
            homeButton.Click += (_, _) => _welcome.BringToFront();
            _nav.Controls.Add(homeButton);

            Load += HomePage_Load;
        }

        private void HomePage_Load(object? sender, EventArgs e)
        {
            AddPage("Pitch Viewer", new PitchViewer());
            AddPage("Player Viewer", new PlayerViewer());
        }

        private void AddPage(string title, Form page)
        {
            page.TopLevel = false;
            page.FormBorderStyle = FormBorderStyle.None;
            page.Dock = DockStyle.Fill;
            _content.Controls.Add(page);   // appended behind _welcome in the z-order
            page.Show();                   // page is live immediately -> its Load events fire at startup

            var button = new Button { AutoSize = true };
            button.Click += (_, _) => page.BringToFront();
            _nav.Controls.Add(button);

            void SyncButton(object? s, EventArgs e)
            {
                button.Enabled = page.Enabled;
                button.Text = page.Enabled ? title : $"{title} (loading...)";
            }

            SyncButton(null, EventArgs.Empty);
            page.EnabledChanged += SyncButton;

            if (!page.Enabled)
                AlertIfNotLoaded(title, page);
        }

        private void PitchButton_Click(object? sender, EventArgs e)
        {
            OpenChild(new PitchViewer());
        }

        private void PlayerButton_Click(object? sender, EventArgs e)
        {
            OpenChild(new PlayerViewer());
        }

        private void OpenChild(Form child)
        {
            child.FormClosed += (_, _) => Show();
            Hide();
            child.Show();
        }

        private static async void AlertIfNotLoaded(string title, Form page)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));

            if (page.IsDisposed)
                return;

            if (!page.Enabled)
                throw new TimeoutException($"'{title}' loading failed");
        }
    }
}
