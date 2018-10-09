using System.Windows.Forms;

namespace monopoly
{
    public partial class MainForm : Form
    {
        Game game;
        public MainForm()
        {
            InitializeComponent();
            play_game();
        }

        void play_game ()
        {
            bool play_again = false;
            short players_remaining = 0;
            do
            {
                // create a new game
                new_game();
                do
                {
                    take_turn();
                    end_turn();

                    players_remaining = get_number_of_players_left();
                // loop through taking turns until only one person is left in the game.
                } while (players_remaining > 1);

            } while (play_again);
        }
        void new_game()
        {
            game = new Game();
        }

        void take_turn ()
        {
            Die die_1 = new Die();
            Die die_2 = new Die();

            // roll dice to determine spaces moved
            short roll = game.Roll(ref die_1, ref die_2);

            // move spaces
            game.CurrentPlayer.Move(roll);

            // check what the current tile is

            //is the tile an owned property?
            Player property_owner = null;
            short index = 0;
            if (game.CurrentPlayer.Space.Type == TileType.PROPERTY)
            {
                ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game);

                if (game.IsOwnedProperty(ref property_owner, ref index))
                {
                    if (!property_owner.Properties[index].Monopolied)
                        if (!game.CurrentPlayer.PayRent(property_owner.Properties[index], property_owner, roll))
                            go_bankrupt(property_owner);
                }
                if (game.IsBankProperty())
                {
                    // would the player like to buy the property?
                    // if yes
                    game.CurrentPlayer.BuyProperty()
                }
            }
            

            // chack if the roll was doubles
            if (die_1.Value == die_2.Value)
                take_turn();
        }
        
        void go_bankrupt(Player bankrupter)
        {
            game.CurrentPlayer = game.CurrentPlayer.FileBankruptcy(bankrupter);
        }

        void end_turn()
        {
            game.NextPlayer();
        }

        short get_number_of_players_left ()
        {
            short count = 0;
            foreach (Player player in game.Players)
                if (player != null)
                    count++;
            return count;
        }
    }
}
