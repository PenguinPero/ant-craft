using System;
using System.Windows.Forms;

namespace MravKraftAPI
{
    using Map;

    public partial class ControlForm : Form
    {
        internal ControlForm()
        {
            InitializeComponent();
        }

        internal void SetNames(string player1, string player2)
        {
            CBoxPlayer1.Text = player1;
            CBoxPlayer2.Text = player2;

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

    }

}
