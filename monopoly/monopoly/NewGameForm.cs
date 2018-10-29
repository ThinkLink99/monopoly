using System;
using System.Drawing;
using System.Windows.Forms;

namespace monopoly
{
    public partial class NewGameForm : Form
    {
        public string[] names;
        public NewGameForm()
        {
            InitializeComponent();
            panel1.Controls.Clear();
        }
        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            for (int i = 0; i < int.Parse(ddlPlayers.SelectedItem.ToString()); i++)
            {
                Panel pnl = new Panel();
                Label lbl = new Label();
                TextBox txt = new TextBox();

                lbl.Parent = pnl;
                txt.Parent = pnl;

                pnl.Size = new Size(264, 27);
                lbl.Size = new Size(66, 13);
                txt.Size = new Size(121, 20);

                lbl.Location = new Point(0, 0);
                txt.Location = new Point(66, 0);

                lbl.Text = "Enter Name: ";

                pnl.Controls.Add(lbl);
                pnl.Controls.Add(txt);

                pnl.Name = "pnl " + i.ToString();
                if (i != 0)
                    pnl.Location = new Point(pnl.Location.X, panel1.Controls[i - 1].Location.Y + pnl.Height);
                pnl.Show();
                panel1.Controls.Add(pnl);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool broke = false;
            names = new string[8];
            for (int i = 0; i < int.Parse(ddlPlayers.SelectedItem.ToString()); i++)
            {
                names[i] = panel1.Controls[i].Controls[1].Text;
                if (names[i] == "")
                {
                    broke = true;
                    break;
                }
            }
            if (!broke) Hide();
        }
    }
}
