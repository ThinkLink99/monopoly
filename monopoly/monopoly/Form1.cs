using System.Threading;
using System.Windows.Forms;
using linkopoly_api;

namespace monopoly
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Game game;

        Die die_1 = new Die();
        Die die_2 = new Die();

        // --- Gameplay --- \\
        void play_game(string[] players)
        {
            string play_again = "Y";
            short players_remaining = 0;
            do
            {
                // create a new game
                new_game(players);
                do
                {
                    clear_console();
                    if (game.CurrentPlayer.Name != "")
                    {
                        start_turn();
                        take_turn();
                        end_turn();

                        print_to_console("Player 2, press any key to begin your turn...");
                        //Console.ReadKey();

                        players_remaining = get_number_of_players_left();
                        // loop through taking turns until only one person is left in the game.
                    }

                } while (players_remaining > 1);
                get_input(new string[] { "Would you like to play again? [Y/N]: " }, new string[] { "y", "n" }, ref play_again);

            } while (play_again.ToString().ToUpper() == "Y");
        }
        void new_game(string[] players)
        {
            game = new Game(players);
        }

        void start_turn()
        {
            print_to_console(game.CurrentPlayer.Name + ", it is your turn.");
        }
        void take_turn()
        {
            string choice = "";

            // see if the user would like to roll or trade
            if (game.CurrentPlayer.IsInJail)
            {
                jail_check();
            }
            else
            {
                get_input(new string[] { "Would you like to roll the dice or make a trade with a player?[Roll / Trade]: " }, new string[] { "roll", "trade" }, ref choice);

                if (choice.ToUpper() == "ROLL")
                {
                    roll_dice();
                }

                if (choice.ToUpper() == "TRADE")
                {
                    trade();
                }
            }
        }

        /// <summary>
        /// this function calls the game.RollDice function and moves the player to that BoardSpace. 
        /// This function then checks which type of of space the player is currently on.
        /// </summary>
        void roll_dice()
        {
            print_to_console("");
            print_to_console("--- Rolling ---");
            short roll = 0;

            // roll dice to determine spaces moved
            roll = game.RollDice(ref die_1, ref die_2);
            print_to_console("You rolled a " + roll.ToString() + "!");

            // move spaces
            game.CurrentPlayer.Move(roll, game);

            // check what the current tile is

            //is the tile an owned property?

            if (game.CurrentPlayer.Space.Type == TileType.PROPERTY)
            {
                print_to_console("");
                print_to_console("--- Property ---");
                property_check(ref roll);
            }
            else if (game.CurrentPlayer.Space.Type == TileType.CHANCE)
            {
                // draw a chance card
                print_to_console("You landed on Chance!");
            }
            else if (game.CurrentPlayer.Space.Type == TileType.COMMUNITY)
            {
                // draw a community chest card
                print_to_console("You landed on Community Chest!");
            }
            else if (game.CurrentPlayer.Space.Type == TileType.GO_TO_JAIL)
            {
                print_to_console("You landed on Go to Jail!");
                game.CurrentPlayer.GoToJail();

                print_to_console("--- Jail Rights ---");
                print_to_console("1) You can attempt to roll doubles once per turn for three turns to get out.");
                print_to_console("   If you do not roll doubles on the third attempt, you must pay $50 or use a Get Out Of Jail Free card.");
                print_to_console("   (Paying or playing a GOOJF card can be done instead of rolling)");
                print_to_console("2) You are allowed to collect rent and make trade deals with other players while in jail.");

            }
            else if (game.CurrentPlayer.Space.Type == TileType.JAIL)
            {
                print_to_console("You landed on Jail! Don't worry, you're just visiting.");

            }
            else if (game.CurrentPlayer.Space.Type == TileType.GO)
            {
                print_to_console("You landed on Go!");

            }
            else if (game.CurrentPlayer.Space.Type == TileType.TAX)
            {
                print_to_console("You landed on Tax!");

            }
            else if (game.CurrentPlayer.Space.Type == TileType.FREE_PARKING)
            {
                print_to_console("You landed on Free Parking!");

            }
            else
            {
                print_to_console("You landed on...");
                print_to_console("wait... how did this happen?");
                print_to_console("we're smarter than this.");
                print_to_console("");
                print_to_console("Congratulations. You broke my game. Your parents must be very proud of you.");
                Application.Exit();
            }

            if (roll_is_doubles())
                take_turn();
        }

        /// <summary>
        /// this functions handles getting a list of players that can be traded with and 
        /// then allowing the current player to choose which properties he wants to give, and which properties he wants to get.
        /// </summary>
        void trade()
        {
            print_to_console("");
            print_to_console("--- Trading ---");
            string choice = "";

            print_to_console("Select a player to trade with: ");
            short i = select_trader();

            print_to_console(string.Concat("Now trading with ", game.Players[i].Name));

            print_to_console("");
            print_to_console("--- This player will give you ---");
            int[] trader_gives = select_trades(game.Players[i]);

            print_to_console("");
            print_to_console("--- You will give this player ---");
            int[] trader_gets = select_trades(game.CurrentPlayer);

            if (trader_gives.Length > 0 && trader_gets.Length > 0)
            {
                get_input(new string[] { game.Players[i].Name + ", do you wish to accept this trade, reject his trade or change this trade( [Accept/Reject/Change]: " }, new string[] { "accept", "reject", "change" }, ref choice);

                if (choice.ToUpper() == "ACCEPT")
                {
                    trader_accepts(ref game.Players[i], trader_gives, trader_gets);
                }
                else if (choice.ToUpper() == "REJECT")
                {
                    print_to_console(game.Players[i].Name + " rejected the trade");
                }
                else if (choice.ToUpper() == "CHANGE")
                {

                }
                else
                {

                }
            }
            else
            {
                if (trader_gets.Length == 0)
                {
                    print_to_console("You have nothing to trade.");
                }
                if (trader_gives.Length == 0)
                {
                    print_to_console("This player has nothing to trade");
                }
            }

            print_to_console("Now taking turn...");
            // when done trading, roll dice
            roll_dice();
        }
        /// <summary>
        /// asks the user to input the number of the desired trade partner. 
        /// </summary>
        /// <returns>returns the parsed input text - 1 to use as the index of the player in game.Players</returns>
        short select_trader()
        {
            string choice = "";
            // ask the player to select a player to trade with
            int i = 0;
            foreach (Player player in game.Players)
            {
                if (player != game.CurrentPlayer && player != null )
                    print_to_console(string.Concat("[", i + 1, "]: ", player.Name));
                i++;
            }

            PopupWindow pw = new PopupWindow();
            read_from_console(ref choice, pw);

            return (short)(int.Parse(choice) - 1);
        }
        /// <summary>
        /// gets all properties available to trade from the current trader
        /// and asks the current user to input all properties he wants to trade
        /// </summary>
        /// <param name="trader">the player whose properties currently need to be accessed</param>
        /// <returns>returns an array of choices as indicies</returns>
        int[] select_trades(Player trader)
        {
            int i = 0;
            string choice = "";
            int[] trades = new int[0];
            if (trader.Properties[0] != null)
            {
                print_to_console("Here is a list of all the items currently owned by " + trader.Name + ": ");

                print_to_console("Enter the number of each item you wish to trade. enter done when finished.");

                i = 0;
                foreach (Property property in trader.Properties)
                {
                    if (property != null)
                        print_to_console(string.Concat("[", i, "]: ", property.Name));
                    i++;
                }

                trades = new int[28];
                i = 0;
                do
                {
                    PopupWindow pw = new PopupWindow();
                    read_from_console(ref choice, pw);
                    try
                    {
                        trades[i] = int.Parse(choice);
                    }
                    catch { }
                    i++;
                }
                while (choice.ToUpper() != "DONE");
            }
            return trades;
        }
        /// <summary>
        /// this function handles moving properties from one player's property list to the other.
        /// </summary>
        /// <param name="trader">the person the current player is trading with. best if calling from the array of players by reference</param>
        /// <param name="trader_gives">the array of choices that the current player will recieve from the trader</param>
        /// <param name="trader_gets">the array of choices that the current player will  give to the trader</param>
        void trader_accepts(ref Player trader, int[] trader_gives, int[] trader_gets)
        {
            int i = 0;
            Property[] temp = new Property[trader_gets.Length];

            foreach (int trade in trader_gives)
            {
                if (trade != 0)
                {
                    temp[i] = game.Players[i].Properties[trade];
                    game.Players[i].Properties[trade] = null;
                }
                i++;
            }
            foreach (int trade in trader_gets)
            {
                int j = 0;
                if (trade != 0)
                {
                    foreach (Property property in game.Players[i].Properties)
                    {
                        if (property == null)
                        {
                            game.Players[i].Properties[j] = game.CurrentPlayer.Properties[trade];
                            break;
                        }
                        j++;
                    }
                }
            }
            foreach (int trade in trader_gives)
            {
                int j = 0;
                if (trade != 0)
                {
                    foreach (Property property in game.CurrentPlayer.Properties)
                    {
                        if (property == null)
                        {
                            game.CurrentPlayer.Properties[j] = temp[trade];
                            break;
                        }
                        j++;
                    }
                }
            }
        }

        /// <summary>
        /// this function will handle checking which actions the current player would like to take while still in jail
        /// </summary>
        void jail_check()
        {
            string choice = "";
            short roll = 0;

            get_input(new string[] { "Would you like to leave jail or stay in Jail? [Leave/Stay]: " }, new string[] { "leave", "stay" }, ref choice);

            if (choice.ToUpper() == "LEAVE")
            {
                get_input(new string[] { "Would you like to roll, pay, or use a get out of jail free card. [Roll/Pay/Card]: " }, new string[] { "roll", "pay", "card" }, ref choice);
                if (choice.ToUpper() == "ROLL")
                {
                    roll = game.RollDice(ref die_1, ref die_2);

                    if (roll_is_doubles())
                    {
                        game.CurrentPlayer.GetOutOfJail();
                    }
                }
                else if (choice.ToUpper() == "PAY")
                {
                    game.CurrentPlayer.PayCash(50);
                    game.BankPayin(50);
                    game.CurrentPlayer.GetOutOfJail();
                }
                else if (choice.ToUpper() == "CARD")
                {

                }
                else
                {

                }
            }
            else if (choice.ToUpper() == "STAY")
            {
                get_input(new string[] { "Would you like to trade with another player or end your turn? [Trade/End]: " }, new string[] { "trade", "end" }, ref choice);

                if (choice.ToUpper() == "TRADE")
                {
                    trade();
                }
            }
            if (game.CurrentPlayer.IsInJail)
            {

            }
        }
        /// <summary>
        /// this function will handle the buying or passing of a property the current player is currently on.
        /// </summary>
        /// <param name="roll">the value of the dice currently rolled. used to calculate utility rent if necessary</param>
        void property_check(ref short roll)
        {
            string choice = "";
            Player property_owner = null;
            short index = 0;

            print_to_console("You landed on " + ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game).Name + "!");
            if (game.IsOwnedProperty(ref property_owner, ref index))
            {
                if (!property_owner.Properties[index].Monopolied)
                    game.CurrentPlayer.PayRent(property_owner.Properties[index], property_owner, roll);
                print_to_console("");
                if (game.CurrentPlayer.Cash <= 0)
                {
                    go_bankrupt(property_owner);
                }
            }
            if (game.IsBankProperty())
            {
                get_input(new string[] { "Would you like to purchase this Property? [Y/N]: " }, new string[] { "y", "n" }, ref choice);

                // if yes
                if (choice.ToUpper() == "Y")
                {
                    Property property = ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game);
                    game.GiveProperty(property, game.CurrentPlayer);
                    print_to_console("You bought " + property.Name + " for $" + property.Cost + "!");
                }
                else if (choice.ToUpper() == "N")
                {
                    // property goes to auction
                }
            }
        }

        /// <summary>
        /// checks if the value on both dice are the same
        /// </summary>
        /// <returns>returns true if both values are the same</returns>
        bool roll_is_doubles()
        {
            return die_1.Value == die_2.Value;
        }

        /// <summary>
        /// gives all money, and properties the player owned to the player that bankrupted them
        /// </summary>
        /// <param name="bankrupter">the player that recieved rent from the current player</param>
        void go_bankrupt(Player bankrupter)
        {
            game.CurrentPlayer = game.CurrentPlayer.FileBankruptcy(bankrupter);
        }
        /// <summary>
        /// moves the current player index to the next available not null array index
        /// </summary>
        void end_turn()
        {
            game.NextPlayer();
        }
        /// <summary>
        /// loops through current array of players and gets count of all positions that are not null
        /// </summary>
        /// <returns>returns the number of positions that are not null</returns>
        short get_number_of_players_left()
        {
            short count = 0;
            foreach (Player player in game.Players)
                if (player != null)
                    count++;
            return count;
        }

        // --- Console Commands --- \\
        /// <summary>
        /// prints a string to the console window, then sets a new line
        /// </summary>
        /// <param name="message">the message to print to the console window</param>
        void print_to_console(string message)
        {
            rtbOutput.Text += message + "\n";
        }
        /// <summary>
        /// clears all text from the console window
        /// </summary>
        void clear_console()
        {
            rtbOutput.Text = "";
        }
        /// <summary>
        /// gets the input and validates it against an array of string options given. function will loop until user inputs something from the given array.
        /// </summary>
        /// <param name="messages">the input prompt to write on screen</param>
        /// <param name="options">the array of options to choose input from</param>
        /// <param name="response">the string variable that holds the choice given by reference</param>
        void get_input(string[] messages, string[] options, ref string response)
        {
            PopupWindow pw = new PopupWindow();
            bool valid = false;
            do
            {
                foreach (string message in messages)
                {
                    print_to_console(message);
                    pw.lblPrompt.Text += message + "\n";
                }
                read_from_console(ref response, pw);

                foreach (string option in options)
                {
                    valid = (response.ToUpper() == option.ToUpper());
                    if (valid) break;
                }

                //if (response.ToUpper() == "QUIT") Application.Exit(); break;

            } while (!valid);
        }
        /// <summary>
        /// reads input from the console window and places it into a variable called by reference
        /// </summary>
        /// <param name="response">the string variable by refernce holding the inputted text</param>
        void read_from_console(ref string response, PopupWindow pw)
        {
            pw.ShowDialog();
            response = pw.txtInput.Text;
            pw.Close();
        }

        private void newGameToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            NewGameForm ngf = new NewGameForm();
            ngf.ShowDialog();

            play_game(ngf.names);
        }

        private void quitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void fullscreenToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (goWIndowedToolStripMenuItem.Checked)
            {
                goWIndowedToolStripMenuItem.Checked = false;
            }
            FormBorderStyle = FormBorderStyle.None;
            Size = Screen.PrimaryScreen.Bounds.Size;
        }

        private void goWIndowedToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (fullscreenToolStripMenuItem.Checked)
                fullscreenToolStripMenuItem.Checked = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Size = Screen.PrimaryScreen.WorkingArea.Size;
            Location = Screen.PrimaryScreen.Bounds.Location;
        }
    }
}
