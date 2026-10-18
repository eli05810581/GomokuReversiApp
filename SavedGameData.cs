
using System.Collections.Generic;

namespace GomokuReversiApp
{
    public class SavedGameData
    {
        // Stores the game configuration
        public LaunchConfiguration Configuration { get; set; } = null!;

        // Stores the current game state
        public GameState CurrentState { get; set; } = null!;

        // Stores the undo history
        public List<GameState> UndoHistory { get; set; } = new List<GameState>();

        // Stores the redo history
        public List<GameState> RedoHistory { get; set; } = new List<GameState>();
    }
}
