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

            input_is_valid(new string[]
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
                print_to_console("Play Again? [Y/N]: ");
                play_again = Console.ReadLine();

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
                input_is_valid(new string[] { "Would you like to roll the dice or make a trade with a player?[Roll / Trade]: " }, new string[] { "roll", "trade" }, ref choice);

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
        static void trade()
        {
            print_to_console("");
            print_to_console("--- Trading ---");
            string choice = "";

            print_to_console("Select a player to trade with: ");
            Player trader = select_trader();

            print_to_console(string.Concat("Now trading with ", trader.Name));

            print_to_console("");
            print_to_console("--- This player will give you ---");
            int[] trader_gives = select_trades(trader);

            print_to_console("");
            print_to_console("--- You will give this player ---");
            int[] trader_gets = select_trades(game.CurrentPlayer);

            if (trader_gives.Length > 0 && trader_gets.Length > 0)
            {
                input_is_valid(new string[] { trader.Name + ", do you wish to accept this trade, reject his trade or change this trade( [Accept/Reject/Change]: " }, new string[] { "accept", "reject", "change" }, ref choice);

                if (choice.ToUpper() == "ACCEPT")
                {
                    trader_accepts(trader, trader_gives, trader_gets);
                }
                else if (choice.ToUpper() == "REJECT")
                {
                    print_to_console(trader.Name + " rejected the trade");
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
        static Player select_trader()
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
            return game.Players[int.Parse(choice) - 1];
        }
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
        static void trader_accepts(Player trader, int[] trader_gives, int[] trader_gets)
        {
            int i = 0;
            Property[] temp = new Property[trader_gets.Length];

            foreach (int trade in trader_gives)
            {
                if (trade != 0)
                {
                    temp[i] = trader.Properties[trade];
                    trader.Properties[trade] = null;
                }
                i++;
            }
            foreach (int trade in trader_gets)
            {
                int j = 0;
                if (trade != 0)
                {
                    foreach (Property property in trader.Properties)
                    {
                        if (property == null)
                        {
                            trader.Properties[j] = game.CurrentPlayer.Properties[trade];
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

        static void jail_check()
        {
            string choice = "";
            short roll = 0;

            input_is_valid(new string[] { "Would you like to leave jail or stay in Jail? [Leave/Stay]: " }, new string[] { "leave", "stay" }, ref choice);

            if (choice.ToUpper() == "LEAVE")
            {
                input_is_valid(new string[] { "Would you like to roll, pay, or use a get out of jail free card. [Roll/Pay/Card]: " }, new string[] { "roll", "pay", "card" }, ref choice);
                if (choice == "ROLL")
                {
                    roll = game.RollDice(ref die_1, ref die_2);

                    if (die_1.Value == die_2.Value)
                    {
                        game.CurrentPlayer.GetOutOfJail();
                    }
                }
            }
            else if (choice.ToUpper() == "STAY")
            {

            }
            if (game.CurrentPlayer.IsInJail)
            {

            }
        }
        static void property_check(ref short roll)
        {
            string choice = "";
            Player property_owner = null;
            short index = 0;

            print_to_console("You landed on " + ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game).Name + "!");
            if (game.IsOwnedProperty(ref property_owner, ref index))
            {
                if (!property_owner.Properties[index].Monopolied)
                    if (!game.CurrentPlayer.PayRent(property_owner.Properties[index], property_owner, roll))
                        go_bankrupt(property_owner);
            }
            if (game.IsBankProperty())
            {
                input_is_valid(new string[] { "Would you like to purchase this Property? [Y/N]: " }, new string[] { "y", "n" }, ref choice);

                // if yes
                if (choice.ToUpper() == "Y")
                {
                    Property property = ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game);
                    game.CurrentPlayer.BuyProperty(property);
                    game.GiveProperty(property, game.CurrentPlayer);
                    print_to_console("You bought " + property.Name + " for $" + property.Cost + "!");
                }
                else if (choice.ToUpper() == "N")
                {
                    // property goes to auction
                }
            }
        }

        static bool roll_is_doubles()
        {
            return die_1.Value == die_2.Value;
        }

        static void go_bankrupt(Player bankrupter)
        {
            game.CurrentPlayer = game.CurrentPlayer.FileBankruptcy(bankrupter);
        }
        static void end_turn()
        {
            game.NextPlayer();
        }
        static short get_number_of_players_left()
        {
            short count = 0;
            foreach (Player player in game.Players)
                if (player != null)
                    count++;
            return count;
        }

        // --- Console Commands --- \\
        static void print_to_console(string message)
        {
            Console.WriteLine(message);
        }
        static void clear_console()
        {
            Console.Clear();
        }
        static void input_is_valid (string[] messages, string[] options, ref string response)
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
        static void read_from_console(ref string response)
        {
            response = Console.ReadLine();
        }
    }
}
