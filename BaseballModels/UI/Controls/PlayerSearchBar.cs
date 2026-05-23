using Db;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI.Controls
{
    public partial class PlayerSearchBar : UserControl
    {
        private List<Player> PlayerList;
        private bool StopTextChangeEvent;

        private class PlayerBoxItem
        {
            public Player p { get; set; }

            public override string ToString()
            {
                return p.MlbId.ToString() + " " + p.UseFirstName + " " + p.UseLastName;
            }
        }

        public PlayerSearchBar()
        {
            PlayerList = new();
            StopTextChangeEvent = false;

            InitializeComponent();
        }

        public void SetPlayerList(List<Player> pl)
        {
            PlayerList = pl;
        }

        public event EventHandler<Player> PlayerSelected;

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Player? p = PlayerList
                .Where(f => f.MlbId == Convert.ToInt32(nudPlayerId.Value))
                .SingleOrDefault();

            StopTextChangeEvent = true;
            if (p == null)
            {
                tbPlayerName.Text = "";
                lbNameList.Items.Clear();
            }
            else
            {
                tbPlayerName.Text = p.UseFirstName + " " + p.UseLastName;
                lbNameList.Items.Clear();

                PlayerSelected(this, p);
            }
            StopTextChangeEvent = false;
        }

        private void tbPlayerName_TextChanged(object sender, EventArgs e)
        {
            lbNameList.Items.Clear();

            if (StopTextChangeEvent)
                return;

            // Get Valid Players
            string text = tbPlayerName.Text.ToLower();
            if (text.Length <= 2)
                return;

            var firstNamePlayers = PlayerList
                .Where(f =>
                    (f.UseFirstName + " " + f.UseLastName)
                    .StartsWith(text, StringComparison.CurrentCultureIgnoreCase));
            var lastNamePlayers = PlayerList
                .Where(f =>
                    (f.UseLastName + " " + f.UseFirstName)
                    .StartsWith(text, StringComparison.CurrentCultureIgnoreCase));

            // Avoid spam
            if (firstNamePlayers.Count() + lastNamePlayers.Count() > 100)
                return;

            // Add names to listbox
            foreach (var p in firstNamePlayers)
                lbNameList.Items.Add(new PlayerBoxItem { p = p });
            foreach (var p in lastNamePlayers)
                lbNameList.Items.Add(new PlayerBoxItem { p = p });

        }

        private void lbNameList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbNameList.SelectedItem is PlayerBoxItem pbi)
            {
                PlayerSelected(this, pbi.p);
                StopTextChangeEvent = true;
                lbNameList.Items.Clear();
                nudPlayerId.Value = pbi.p.MlbId;
                tbPlayerName.Text = pbi.p.UseFirstName + " " + pbi.p.UseLastName;
                StopTextChangeEvent = false;
            }
        }
    }
}
