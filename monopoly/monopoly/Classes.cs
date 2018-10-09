using System;

namespace monopoly
{
    /// <summary>
    /// Colors is the property colors in game, including railroads and utilities
    /// </summary>
    public enum Colors { Brown, LightBlue, Magenta, Orange, Red, Yellow, Green, DarkBlue, Railroad, Utility }
    /// <summary>
    /// Tokens is the player tokens that can be used in game
    /// </summary>
    public enum Tokens { NONE, DOG, CAT, HAT, CAR, THIMBLE, BOOT}
    /// <summary>
    /// CardType is the the type of cards the player could recieve in the game
    /// </summary>
    public enum CardType { CHANCE, COMMUNITY }

    public enum TileType { PROPERTY, CHANCE, COMMUNITY, FREE_PARKING, GO_TO_JAIL, JAIL, TAX, GO}

    /// <summary>
    /// Game controls all functions the game handle, such as keeping track of players and properties
    /// </summary>
    public class Game
    {
        const short MAX_PLAYERS = 8;
        const int STARTING_BANK = 20000; // Bank only has 20,000 in it

        protected int current = 0;
        protected int bank = 20000;

        /// <summary>
        /// The current amount of money in the game Bank. Bank starts with 20,000 dollars in it.
        /// </summary>
        public int Bank { get { return bank; } }

        /// <summary>
        /// An array of all players in the game
        /// </summary>
        public Player[] Players { get; set; }
        /// <summary>
        /// An array of all properties still owned by the bank
        /// </summary>
        public Property[] Properties { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Card[] ChancePile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Card[] CommunityChestPile { get; set; }

        public Player CurrentPlayer { get { return Players[current]; } set { Players[current] = value; } }

        void get_players ()
        {
            Players = new Player[MAX_PLAYERS];
        }
        void replenish_bank ()
        {
            bank = STARTING_BANK;
        }
        void pay_starting_cash ()
        {
            foreach (Player player in Players)
            {
                // give each player 1500 dollars
                player.RecieveCash(1500);

                // take that money from the bank, there should never be more or less than 20000 in play
                BankPayout(1500);
            }
        }

        public Game()
        {
            NewGame();
        }

        public void NewGame()
        {
            get_players();
            replenish_bank();
            pay_starting_cash();
        }

        /// <summary>
        /// BankPayout will subtract money from the bank total
        /// </summary>
        /// <param name="amount_out">The amount the bank loses</param>
        public void BankPayout (int amount_out)
        {
            bank -= amount_out;
        }
        /// <summary>
        /// BankPayin will add money back into the bank total
        /// </summary>
        /// <param name="amount_in">The amount the bank recieves</param>
        public void BankPayin (int amount_in)
        {
            bank += amount_in;
        }

        /// <summary>
        /// GiveProperty will call the PLayer.BuyProperty function and set the index of the bank owned properties to null
        /// </summary>
        /// <param name="property"></param>
        /// <param name="player"></param>
        public void GiveProperty (Property property, Player player)
        {
            player.BuyProperty(property);

            // Pay the bank
            BankPayin(property.Cost);

            int i = 0;
            foreach (Property prop in Properties)
            {
                if (prop == property)
                {
                    Properties[i] = null;
                }
                i++;
            }
        }

        public Card DrawChance ()
        {
            int i = 0;
            foreach (Card chance in ChancePile)
                if (chance != null)
                {
                    ChancePile[i] = null;
                    return chance;
                }
                else
                {
                    i++;
                }
            return null;
        }
        public Card DrawCommunityChest ()
        {
            int i = 0;
            foreach (Card CC in CommunityChestPile)
                if (CC != null)
                {
                    CommunityChestPile[i] = null;
                    return CC;
                }
                else
                {
                    i++;
                }
            return null;
        }

        /// <summary>
        /// Roll takes two dice by reference, gives a random number between 1 and six on both, adds them up and returns the value. 
        /// Dice are given to the function so that the value of each can be stored and checked for doubles.
        /// </summary>
        /// <param name="die_1"></param>
        /// <param name="die_2"></param>
        /// <returns></returns>
        public short Roll (ref Die die_1, ref Die die_2)
        {
            die_1.Roll();
            die_2.Roll();

            return (short)(die_1.Value + die_2.Value);
        }

        public bool IsOwnedProperty (ref Player property_owner, ref short property_index)
        {
            foreach (Player player in Players)
            {
                if (player != CurrentPlayer)
                {
                    short i = 0;
                    foreach (Property property in player.Properties)
                    {
                        if (property.Space == CurrentPlayer.Space)
                        {
                            property_owner = player;
                            property_index = i;
                            return true;
                        }
                        i++;
                    }
                }
            }
            return false;
        }

        public bool IsBankProperty()
        {
            for(int i = 0; i < Properties.Length; i++)
            {
                if (Properties[i].Space == CurrentPlayer.Space)
                    return true;
            }
            return false;
        }

        public void NextPlayer ()
        {
            if (current++ >= Players.Length)
                current = 0;
        }
    }

    /// <summary>
    /// The object the user controls in game. 
    /// Contains Cash balance, property list, and current board space index
    /// The Player class also contains methods to move cash and collect properties.
    /// </summary>
    public class Player : IDisposable
    {
        protected string name = "";
        protected Tokens token = Tokens.NONE;
        protected int cash = 0;
        protected BoardSpace space;
        protected short rr = 0, utils = 0;
        protected Property[] properties = new Property[27];

        /// <summary>
        /// THe Name of the Player, Used for identification in-game
        /// </summary>
        public string Name { get { return name; } }
        /// <summary>
        /// 
        /// </summary>
        public Tokens Token { get { return token; } }
        /// <summary>
        /// The integer amount of cash this player has
        /// </summary>
        public int Cash { get { return cash; } }
        /// <summary>
        /// The current tile the player is at on the board.
        /// </summary>
        public BoardSpace Space { get { return space; } }
        /// <summary>
        /// The Int16 amount of railroads this plyer owns
        /// </summary>
        public short Railroads { get { return rr; } }
        /// <summary>
        /// the Int16 amount of Utilities this player owns.
        /// </summary>
        public short Utilities { get { return utils; } }
        /// <summary>
        /// An Array of all properties this player owns
        /// </summary>
        public Property[] Properties { get { return properties; } }

        /// <summary>
        /// Initializes a new Player object with a given name, 1500 cash, and starting index of 0
        /// </summary>
        /// <param name="_name">The name of the Player</param>
        public Player(string _name)
        {
            name = _name;

            cash = 1500;
            space = 0;
        }

        public void PayCash(int amount)
        {
            cash -= amount;
        }
        public void RecieveCash(int amount)
        {
            cash += amount;
        }

        /// <summary>
        /// BuyProperty subtracts the cost of the property from the player's current balance and then puts the property into the first null index in the array
        /// </summary>
        /// <param name="property"></param>
        public void BuyProperty(Property property)
        {
            // Pay the buying price for the property
            PayCash(property.Cost);

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
            MonopolyCheck(property.Color);
        }

        public void RecieveProperties (Player recieving_from)
        {
            for (int j = 0; j < Properties.Length; j++)
            {
                if (Properties[j] == null)
                {
                    recieving_from.Properties.CopyTo(Properties, j);
                }
            }
        }

        /// <summary>
        /// PayRent will check if the tile that was landed was a railroad, utility, or regualr tile, then check for the rent modifers.  
        /// </summary>
        /// <param name="property"></param>
        /// <param name="property_owner"></param>
        /// <param name="roll"></param>
        /// <returns>Returns false if the player is out of money</returns>
        public bool PayRent(Property property, Player property_owner, short roll)
        {
            switch (property.Color)
            {
                case Colors.Railroad:
                    PayCash(property.Rent(property_owner.Railroads));
                    property_owner.RecieveCash(property.Rent(property_owner.Railroads));
                    break;
                case Colors.Utility:
                    PayCash(property.Rent(roll, property_owner.Utilities));
                    property_owner.RecieveCash(property.Rent(roll, property_owner.Utilities));
                    break;
                default:
                    PayCash(property.Rent());
                    property_owner.RecieveCash(property.Rent());
                    break;
            }
            if (Cash < 0)
                return false;
            else return true;
        }

        public void MonopolyCheck(Colors color)
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
                case Colors.Brown:
                case Colors.DarkBlue:
                case Colors.Utility:
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

        public void Move(short spaces)
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

        public Player FileBankruptcy (Player bankrupter)
        {
            bankrupter.RecieveProperties(this);
            Dispose();
            return null;
        }

    #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Player() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class Property 
    {
        protected Colors color;

        protected string name = "";
        protected short space = 0;
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

        protected bool mortaged = false;

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
        public short Space { get { return space; } }
        public Colors Color { get { return color; } }
        /// <summary>
        /// The cost of buying the property from the bank
        /// </summary>
        public int Cost { get { return cost; } }

        /// <summary>
        /// The cost of buying one house on this property
        /// </summary>
        public int House_Cost { get { return house_cost; } }
        public short TotalHouses { get { return num_houses; } }
        public short TotalHotels { get { return num_hotels; } }

        /// <summary>
        /// Boolean Flag declaring whether or not a property has been monopolied, all properties of the same color are owned.
        /// </summary>
        public bool Monopolied { get; set; }

        public bool Mortaged { get { return mortaged; } }

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
        public Property(string _name, Colors _color, int _rent, int _one, int _two, int _three, int _four, int _hotel, int _cost, int _cost_house)
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

        /// <summary>
        /// Rent, with no parameters, will check for a monopoly and number of houses to determine rent
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roll"></param>
        /// <param name="utils"></param>
        /// <returns></returns>
        public int Rent(short roll, short utils)
        {
            return UtilityRent(roll, utils);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rr"></param>
        /// <returns></returns>
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

    public class Card
    {
        protected CardType type;
        protected string description;

        public CardType Type { get { return type; } }
        public string Description { get { return description; } }
    }

    public class Die
    {
        protected short value;

        public short Value { get { return value; } }

        public void Roll ()
        {
            System.Random rand = new System.Random();

            value = (short)(rand.Next(6) + 1);
        }
    }

    public class BoardSpace
    {
        protected short index = 0;
        protected TileType type;

        public short Index { get { return index; } }
        public TileType Type { get { return type; } }

        public BoardSpace(TileType type, short index)
        {
            this.type = type;
            this.index = index;
        }
    }

    public class  PropertySpace : BoardSpace
    {
        protected string property_name = "";

        public Property GetProperty(Game currentGame)
        {
            for(int i = 0; i < currentGame.Properties.Length; i++)
            {
                if (currentGame.Properties[i].Name == property_name)
                    return currentGame.Properties[i];
            }
            return null;
        }

        public PropertySpace(short space_index, string property_name) : base (TileType.PROPERTY, space_index)
        {
            this.property_name = property_name;
        }
    }
}
