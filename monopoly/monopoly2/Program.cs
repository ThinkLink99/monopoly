using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace monopoly2
{
    class Program
    {
        static void Main(string[] args)
        {
            play_game();
        }
        static Game game;

        static void play_game()
        {
            string play_again = "Y";
            short players_remaining = 0;
            do
            {
                // create a new game
                new_game();
                do
                {
                    start_turn();
                    take_turn();
                    end_turn();

                    players_remaining = get_number_of_players_left();
                    // loop through taking turns until only one person is left in the game.
                } while (players_remaining > 1);
                print_to_console("Play Again? [Y/N]: ");
                play_again = Console.ReadLine();

            } while (play_again.ToString().ToUpper() == "Y");
        }
        static void new_game()
        {
            game = new Game(new string[] { "Trey", "Trey2" });
        }

        static void start_turn()
        {
            print_to_console(game.CurrentPlayer.Name + ", it is your turn.");
        }
        static void take_turn()
        {
            string choice = "";
            Die die_1 = new Die();
            Die die_2 = new Die();

            // see if the user would like to roll or trade
            print_to_console("Would you like to roll the dice or make a trade with a player? [Roll/Trade]: ");
            choice = Console.ReadLine();

            if (choice.ToUpper() == "ROLL")
            {
                // roll dice to determine spaces moved
                short roll = game.Roll(ref die_1, ref die_2);
                print_to_console("You rolled a " + roll.ToString() + "!");

                // move spaces
                game.CurrentPlayer.Move(roll, game);

                // check what the current tile is

                //is the tile an owned property?
                Player property_owner = null;
                short index = 0;
                if (game.CurrentPlayer.Space.Type == TileType.PROPERTY)
                {
                    print_to_console("You landed on " + ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game).Name + "!");
                    if (game.IsOwnedProperty(ref property_owner, ref index))
                    {
                        if (!property_owner.Properties[index].Monopolied)
                            if (!game.CurrentPlayer.PayRent(property_owner.Properties[index], property_owner, roll))
                                go_bankrupt(property_owner);
                    }
                    if (game.IsBankProperty())
                    {
                        do
                        {
                            // would the player like to buy the property?
                            print_to_console("Would you like to purchase this Property? [Y/N]: ");
                            choice = Console.ReadLine();

                            // if yes
                            if (choice.ToUpper() == "Y")
                            {
                                Property property = ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game);
                                game.CurrentPlayer.BuyProperty(property);
                                game.GiveProperty(property, game.CurrentPlayer);
                                print_to_console("You bought " + property.Name + "!");
                            }
                            else if (choice.ToUpper() == "N")
                            {
                                // property goes to auction
                            }
                            else
                            {
                                print_to_console("You were given two options...");
                                print_to_console("How did you screw that up?");
                                print_to_console("Let's try that again.");

                            }
                        } while (!(choice.ToUpper() == "Y" || choice.ToUpper() == "N"));
                    }
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

                }
                else if (game.CurrentPlayer.Space.Type == TileType.JAIL)
                {
                    print_to_console("You landed on Jail!");

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
            }

            if (choice.ToUpper() == "TRADE")
            {

            }

            // chack if the roll was doubles
            if (die_1.Value == die_2.Value) { }
                //take_turn();
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

        static void print_to_console(string message)
        {
            Console.WriteLine(message);
        }
    }
}
