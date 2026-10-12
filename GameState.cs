
namespace GomokuReversiApp
{
    // Stores a snapshot of the game for undo, redo and saving.
    public class GameState
    {
        public int CurrentPlayerIndex { get; set; }

        public int TurnCount { get; set; }

        public GameResult GameResult { get; set; }

        // Each string represents one row of the board.
        public List<string> BoardRows { get; set; } = new List<string>();

        // Each player's remaining stones/disks.
        // Key = stone/disk symbol
        // Value = remaining quantity (null means unlimited)
        public List<Dictionary<char, int?>> PlayerInventories { get; set; }
            = new List<Dictionary<char, int?>>();
    
    }
}
