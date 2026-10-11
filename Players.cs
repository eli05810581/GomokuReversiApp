namespace GomokuReversiApp
{

    public abstract class Player
    {   
        public string Name {get;}
        public int Number {get;} // 1 or 2
        public Dictionary<char, int?> Inventory { get; private set; } // contains stone/disk symbol and amount remaining. if null amount is unlimited
        protected Player(int number)
        {
            Number = number;
        }
        public abstract Move GetMove(Board board, IGameRules rules); //?????

        public Player(Dictionary<char, int?> inventory)
        {
            Inventory = new Dictionary<char, int?>(inventory);
        }

        public bool HasStoneAvailable(char stoneType)
        {
            if (!Inventory.TryGetValue(stoneType, out int? remaining))
            {
                return false;
            }

            return remaining == null || remaining > 0;
        }

        public void UseStone(char stoneType)
        {
            if (!HasStoneAvailable(stoneType))
            {
                throw new InvalidOperationException("Stone is unavailable.");
            }

            if (Inventory[stoneType] != null)
            {
                Inventory[stoneType]--;
            }
        }

        public void RestoreInventory(Dictionary<char, int?> inventory) // used for Load game
        {
            Inventory = new Dictionary<char, int?>(inventory);
        }
    }


    public class ScriptedPlayer : Player
    {
        public ScriptedPlayer(
            string name,
            Dictionary<char, int?> inventory): base(name, inventory)
        {
        }
    }
    

}