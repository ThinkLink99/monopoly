using System.Windows.Forms;

namespace monopoly
{
    public enum Colors
    {
        Brown,
        LightBlue,
        Magenta,
        Orange,
        Red,
        Yellow,
        Green,
        DarkBlue,
        Railroad,
        Utility
    }

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Game controls all functions the game handle, such as keeping track of players and properties
    /// </summary>
    public class Game
    {
        public Player[] Players { get; set; }

        public Game ()
        {

        }
    }

    /// <summary>
    /// The object the user controls in game. 
    /// Contains Cash balance, property list, and current board space index
    /// The monopoly.Player class also contains methods to move cash and collect properties.
    /// </summary>
    public class Player
    {
        protected string name = "";
        protected int cash = 0;
        protected short space = 0;
        protected short rr = 0, utils = 0;
        protected Property[] properties = new Property[27];

        public string Name { get { return name; } }
        public int Cash { get { return cash; } }
        public short Space { get { return space; } }
        public short Railroads { get { return rr; } }
        public short Utilities { get { return utils; } }
        public Property[] Properties { get { return properties; } }

        /// <summary>
        /// Initializes a new Player object with a given name, 1500 cash, and starting index of 0
        /// </summary>
        /// <param name="_name">The name of the Player</param>
        public Player (string _name)
        {
            name = _name;

            cash = 1500;
            space = 0;
        }

        public void PayCash (int amount)
        {
            cash -= amount;
        }
        public void RecieveCash (int amount)
        {
            cash += amount;
        }

        public void BuyProperty (Property property)
        {
            // Pay the buying price for the property
            PayCash (property.Cost);

            // add the property to the properties array
            for (int i = 0; i < properties.Length; i++)
            {
                // find an empty spot in the array
                if (properties[i] == null)
                {
                    properties[i] = property;
                }
            }

            // check for a monopoly on that color
            MonopolyCheck (property.Color);
        }

        public void PayRent (Property property, Player player, short roll)
        {
            switch (property.Color)
            {
                case Colors.Railroad:
                    PayCash(property.Rent(player.Railroads));
                    player.RecieveCash(property.Rent(player.Railroads));
                    break;
                case Colors.Utility:
                    PayCash (property.Rent(roll, player.Utilities));
                    player.RecieveCash (property.Rent(roll, player.Utilities));
                    break;
                default:
                    PayCash (property.Rent());
                    player.RecieveCash (property.Rent());
                    break;
            }
        }

        public void MonopolyCheck (Colors color)
        {
            int count = 0;

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].Color == color)
                {
                    count++;
                }
            }
            switch (color)
            {
                case Colors.Brown: case Colors.DarkBlue: case Colors.Utility:
                    if (count == 2)
                        foreach (Property prop in properties)
                            if (prop.Color == color)
                                prop.Monopolied = true;
                    break;
                default:
                    if (count == 3)
                        foreach (Property prop in properties)
                            if (prop.Color == color)
                                prop.Monopolied = true;
                    break;
            }
        }

        public void Move (short spaces)
        {
            if (spaces > 0)
            {
                // increase space
                space++;

                // if it is more than 36 (or maximum spaces in one go around)
                if (space > 36)
                {
                    // reset it to 0
                    space = 0;
                }

                // move again
                Move((short)(spaces - 1));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Property
    {
        protected Colors color;

        protected string name = "";
        protected int cost = 0;

        protected int house_cost = 0;
        protected short num_houses = 0;
        protected short num_hotels = 0;

        protected int rent = 0;
        protected int one_house = 0;
        protected int two_house = 0;
        protected int three_house = 0;
        protected int four_house = 0;
        protected int hotel = 0;

        int RailroadRent(short rr)
        {
            return rent * rr;
        }
        int UtilityRent(short roll, short utils)
        {
            if (utils == 1)
            {
                return roll * 4;
            }
            if (utils == 2)
            {
                return roll * 10;
            }

            // if you made it here, you have no utilities, just pay normal rent.
            return rent;

        }
        int MonopolyRent()
        {
            return rent * 2;
        }
        int HousingRent()
        {
            if (num_hotels == 1)
            {
                return hotel;
            }
            else
            {
                if (num_houses > 0)
                {
                    switch (num_houses)
                    {
                        case 1:
                            return one_house;
                        case 2:
                            return two_house;
                        case 3:
                            return three_house;
                        case 4:
                            return four_house;
                    }
                }
            }
            return rent;
        }

        public string Name { get { return name; } }
        public Colors Color { get { return color; } }
        public int Cost { get { return cost; } }

        public int House_Cost { get { return house_cost; } }
        public short TotalHouses { get { return num_houses; } }
        public short TotalHotels { get { return num_hotels; } }

        public bool Monopolied { get; set; }

        /// <summary>
        /// Constructs a new Property object
        /// </summary>
        /// <param name="_name">The name of the property</param>
        /// <param name="_rent">The starting rent of the property</param>
        /// <param name="_one">The rent with one house</param>
        /// <param name="_two">The rent with two houses</param>
        /// <param name="_three">The rent with three houses</param>
        /// <param name="_four">the rent with four houses</param>
        /// <param name="_hotel">The rent with one hotel</param>
        /// <param name="_cost">The buy price of the property</param>
        /// <param name="_cost_house">The cost of buying a house on this property</param>
        public Property (string _name, Colors _color, int _rent, int _one, int _two, int _three, int _four, int _hotel, int _cost, int _cost_house)
        {
            name = _name;
            color = _color;
            rent = _rent;
            one_house = _one;
            two_house = _two;
            three_house = _three;
            four_house = _four;
            hotel = _hotel;
            cost = _cost;
            house_cost = _cost_house;
        }

        public int Rent()
        {
            if (Monopolied)
                if (num_houses > 0 || num_hotels > 0)
                    HousingRent();
                else
                    MonopolyRent();

            // If the code made it this far, the property has no houses or hotels, normal rent
            return rent;
        }
        public int Rent(short roll, short utils)
        {
            return UtilityRent(roll, utils);
        }
        public int Rent(short rr)
        {
            return RailroadRent(rr);
        }

        public void AddHouse(short num)
        {
            if (color != Colors.Railroad || color != Colors.Utility)
                if (num_hotels == 0)
                    if (num_houses < 5)
                        num_houses += num;
                    else
                        AddHotel(1);
        }
        public void AddHotel(short num)
        {
            if (color != Colors.Railroad || color != Colors.Utility)
                if (num_hotels < 1 && num_houses == 4)
                {
                    num_hotels += num;
                    num_houses = 0;
                }
        }
    }
}
