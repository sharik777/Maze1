using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Labyrinth
{
    public partial class EndGameDialog : Form
    {
        public EndGameDialog(int Score)
        {
            InitializeComponent();
            label1.Text += "\n" + Score.ToString();

        }

        private void NewGameButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
        }

        private void ExitGameButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
        }

        private void EndGameDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
