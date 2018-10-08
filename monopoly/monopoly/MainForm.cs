using System.Windows.Forms;

namespace monopoly
{
    public partial class MainForm : Form
    {
        Game game;
        public MainForm()
        {
            InitializeComponent();
            new_game();
        }

        void new_game()
        {
            game = new Game();
        }
    }
}
