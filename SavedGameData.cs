namespace GomokuReversiApp
{
    public class SavedGameData // this class stores data of a saved game for it to be restored
    {
        public LaunchConfiguration Configuration { get; set; }

        public GameState CurrentState { get; set; }

        public List<GameState> UndoHistory { get; set; }

        public List<GameState> RedoHistory { get; set; }
    }
    
}