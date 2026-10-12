
namespace GomokuReversiApp
{
    public abstract class Player
    {
        public string Name { get; }
        public int Number { get; } // 1 or 2

        // Stone/disk symbol and quantity remaining.
        // null means unlimited.
        public Dictionary<char, int?> Inventory { get; private set; }

        protected Player(
            string name,
            int number,
            Dictionary<char, int?> inventory)
        {
            Name = name;
            Number = number;
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

        // Used by Load Game and Undo/Redo
        public void RestoreInventory(Dictionary<char, int?> inventory)
        {
            Inventory = new Dictionary<char, int?>(inventory);
        }
    }

    // Human moves are obtained through InteractiveCommandSource
    public class HumanPlayer : Player
    {
        public HumanPlayer(
            string name,
            int number,
            Dictionary<char, int?> inventory)
            : base(name, number, inventory)
        {
        }
    }

    // AI moves are generated using a move strategy
    public class AIPlayer : Player
    {
        // private readonly MoveStrategyBase moveStrategy;
        private readonly IMoveStrategy moveStrategy;

        public AIPlayer(
            string name,
            int number,
            Dictionary<char, int?> inventory,
            IMoveStrategy moveStrategy) // Change from MoveStrategyBase
            : base(name, number, inventory)
        {
            this.moveStrategy = moveStrategy;
        }

        public Move GetMove(Game game)
        {
            return moveStrategy.ChooseMove(game);
        }
    }

    // Automated moves are obtained through AutomatedCommandSource
    public class ScriptedPlayer : Player
    {
        public ScriptedPlayer(
            string name,
            int number,
            Dictionary<char, int?> inventory)
            : base(name, number, inventory)
        {
        }
    }
}
