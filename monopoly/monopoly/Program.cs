﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace monopoly
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
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

        static void start_turn ()
        {
            print_to_console(game.CurrentPlayer.Name + ", it is your turn.");
        }
        static void take_turn()
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
                    Property property = ((PropertySpace)game.CurrentPlayer.Space).GetProperty(game);
                    game.CurrentPlayer.BuyProperty(property);
                    game.GiveProperty(property, game.CurrentPlayer);
                }
            }


            // chack if the roll was doubles
            if (die_1.Value == die_2.Value)
                take_turn();
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

        static void print_to_console (string message)
        {
            Console.WriteLine(message);
        }
    }
}
