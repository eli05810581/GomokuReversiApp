
namespace GomokuReversiApp
{
    // Stores a snapshot of the game for undo, redo and saving.
    public class GameState
    {
        public int CurrentPlayerIndex { get; set; }

        public int TurnCount { get; set; }

        public GameResult GameResult { get; set; }

        // Each string represents one row of the board.
        public string[] BoardRows { get; set; } =
            Array.Empty<string>();

        // Remaining special stones for players 1 and 2.
        public int[] HeavyStones { get; set; } = new int[2];

        public int[] EraserStones { get; set; } = new int[2];
    }
}
