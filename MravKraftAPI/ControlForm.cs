using System;
using System.Windows.Forms;

namespace MravKraftAPI
{
    using Map;

    public partial class ControlForm : Form
    {
        private string p1, p2;

        internal ControlForm()
        {
            InitializeComponent();
        }

        internal void SetNames(string player1, string player2)
        {
            CBoxPlayer1.Text = p1 = player1;
            CBoxPlayer2.Text = p2 = player2;

            Text = $"{player1} vs {player2}";
        }

        private void CBoxPlayer1_CheckedChanged(object sender, EventArgs e)
        {
            if (CBoxPlayer1.Checked) Patch.PlayerVision |= 1;
            else Patch.PlayerVision -= 1;
        }

        private void CBoxPlayer2_CheckedChanged(object sender, EventArgs e)
        {
            if (CBoxPlayer2.Checked) Patch.PlayerVision |= 2;
            else Patch.PlayerVision -= 2;
        }

        private void ControlForm_Activated(object sender, EventArgs e)
        {
            Opacity = 1.0;
        }

        private void ControlForm_Deactivate(object sender, EventArgs e)
        {
            Opacity = 0.8;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            CBoxPlayer1.Text = $"{p1} [HP: {Baze.Baza.Baze[0].Health}]";
            CBoxPlayer2.Text = $"{p2} [HP: {Baze.Baza.Baze[1].Health}]";
        }
    }

}
