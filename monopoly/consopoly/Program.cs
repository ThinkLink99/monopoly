using System;
using System.Threading;
using linkopoly_api;

namespace Consopoly
{
    class Program
    {
        static void Main(string[] args)
        {
            main_menu();
        }
        static Game game;

        static Die die_1 = new Die();
        static Die die_2 = new Die();

        static void main_menu()
        {
            string[] players = new string[8];
            string choice = "";

            get_input(new string[]
            {
                " ------------------ ",
                " -    Monopoly    - " ,
                " -    Trey Hall   - " ,
                " ------------------ " ,
                " [1]: Play Game!",
                " [Q]: Quit"
            }, new string[] { "1", "Q" }, ref choice);
            int num_players = 0;

            if (choice == "1")
            {
                do
                {
                    print_to_console("How many players? [Minimum: 2]");
                    read_from_console(ref choice);

                    int.TryParse(choice, out num_players);
                    players = new string[num_players];
                } while (num_players <= 1);

                for (int i = 0; i < num_players; i++)
                {
                    print_to_console("Enter Player " + (i + 1).ToString() + "'s name.");
                    read_from_console(ref players[i]);
                    if (players[i] == "") players[i] = null;
                }

                print_to_console("Starting game with these players...");
                Thread.Sleep(3000);
                play_game(players);
            }
            else if (choice == "Q")
            {
                print_to_console("Goodbye!");
                Environment.Exit(0);
            }
        }

        // --- Gameplay --- \\
        static void play_game(string[] players)
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
                        Console.ReadKey();

                        players_remaining = get_number_of_players_left();
                        // loop through taking turns until only one person is left in the game.
                    }

                } while (players_remaining > 1);
                get_input(new string[] { "Would you like to play again? [Y/N]: " }, new string[] { "y", "n" }, ref play_again);

            } while (play_again.ToString().ToUpper() == "Y");
        }
        static void new_game(string[] players)
        {
            game = new Game(players);
        }

        static void start_turn()
        {
            print_to_console(game.CurrentPlayer.Name + ", it is your turn.");
        }
        static void take_turn()
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
        static void roll_dice()
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
                Environment.Exit(0);
            }

            if (roll_is_doubles())
                take_turn ();
        }

        /// <summary>
        /// this functions handles getting a list of players that can be traded with and 
        /// then allowing the current player to choose which properties he wants to give, and which properties he wants to get.
        /// </summary>
        static void trade()
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
                if(trader_gives.Length == 0)
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
        static short select_trader()
        {
            string choice = "";
            // ask the player to select a player to trade with
            int i = 0;
            foreach (Player player in game.Players)
            {
                if (player != game.CurrentPlayer)
                    print_to_console(string.Concat("[", i + 1, "]: ", player.Name));
                i++;
            }

            read_from_console(ref choice);
            return (short)(int.Parse(choice) - 1);
        }
        /// <summary>
        /// gets all properties available to trade from the current trader
        /// and asks the current user to input all properties he wants to trade
        /// </summary>
        /// <param name="trader">the player whose properties currently need to be accessed</param>
        /// <returns>returns an array of choices as indicies</returns>
        static int[] select_trades(Player trader)
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
                    read_from_console(ref choice);
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
        static void trader_accepts(ref Player trader, int[] trader_gives, int[] trader_gets)
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
        static void jail_check()
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
        static void property_check(ref short roll)
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
        static bool roll_is_doubles()
        {
            return die_1.Value == die_2.Value;
        }

        /// <summary>
        /// gives all money, and properties the player owned to the player that bankrupted them
        /// </summary>
        /// <param name="bankrupter">the player that recieved rent from the current player</param>
        static void go_bankrupt(Player bankrupter)
        {
            game.CurrentPlayer = game.CurrentPlayer.FileBankruptcy(bankrupter);
        }
        /// <summary>
        /// moves the current player index to the next available not null array index
        /// </summary>
        static void end_turn()
        {
            game.NextPlayer();
        }
        /// <summary>
        /// loops through current array of players and gets count of all positions that are not null
        /// </summary>
        /// <returns>returns the number of positions that are not null</returns>
        static short get_number_of_players_left()
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
        static void print_to_console(string message)
        {
            Console.WriteLine(message);
        }
        /// <summary>
        /// clears all text from the console window
        /// </summary>
        static void clear_console()
        {
            Console.Clear();
        }
        /// <summary>
        /// gets the input and validates it against an array of string options given. function will loop until user inputs something from the given array.
        /// </summary>
        /// <param name="messages">the input prompt to write on screen</param>
        /// <param name="options">the array of options to choose input from</param>
        /// <param name="response">the string variable that holds the choice given by reference</param>
        static void get_input (string[] messages, string[] options, ref string response)
        {
            bool valid = false;
            do
            {
                foreach(string message in messages)
                {
                    print_to_console(message);
                }
                read_from_console(ref response);

                foreach(string option in options)
                {
                    valid = (response.ToUpper() == option.ToUpper());
                    if (valid) break;
                }
            } while (!valid);
        }
        /// <summary>
        /// reads input from the console window and places it into a variable called by reference
        /// </summary>
        /// <param name="response">the string variable by refernce holding the inputted text</param>
        static void read_from_console(ref string response)
        {
            response = Console.ReadLine();
        }
    }
}
