using UI.Python;

namespace UI.Controls
{
    public partial class DebugControl : UserControl
    {
        public DebugControl()
        {
            InitializeComponent();

            PyCore.PythonMessage += AddMessage;
        }

        public void AddMessage(string msg)
        {
            listBox1.Items.Add(msg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
    }
}
