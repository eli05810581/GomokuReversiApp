namespace GomokuReversiApp
{
        public class Game
    {
        // game components
        public Board Board { get; private set;}
        public List<Player> Players {get; private set;}

        // Strategies
        public IMoveParser MoveParser { get; private set; } 
        public IMoveProcessor MoveProcessor { get; protected set; }
        public IGameOverChecker GameOverChecker { get; protected set; }
        public IVisibilityModel VisibilityModel { get; protected set; }
        public ICommandSource CommandSource { get; protected set; }


        public Player CurrentPlayer { get; private set;}
        public int TurnCount { get; private set;}
        public GameResult gameResult {get; private set;}
        private bool isRunning;
        private bool LoadRequested;

        // undo/redo history
        private List<GameState> gameHistory;
        private List<GameState> redoHistory;

        // constructor
        public Game(
            Board board,
            List<Player> players,
            IMoveParser moveParser,
            IMoveProcessor moveProcessor,
            IGameOverChecker gameOverChecker,
            IVisibilityModel visibilityModel,
            ICommandSource commandSource
        )
        {
            Board = board;
            Players = players;

            MoveParser = moveParser;
            MoveProcessor = moveProcessor;
            GameOverChecker = gameOverChecker;
            VisibilityModel = visibilityModel;
            CommandSource = commandSource;

            CurrentPlayer = Players[0];
            TurnCount = 0;
            gameResult = GameResult.OnGoing;

            gameHistory = new List<GameState>();
            redoHistory = new List<GameState>();
        }

        // important!
        public bool PlayGame()
        {
            if (CommandSource == null)
            {
                throw new InvalidOperationException(
                    "Command source has not been configured."
                );
            }

            isRunning = true;
            LoadRequested = false;

            while (isRunning && gameResult == GameResult.OnGoing)
            {
                DisplayGame();

                ICommand command = CommandSource.GetNextCommand(this);

                command.Execute(this);
            }

            if (gameResult != GameResult.OnGoing)
            {
                Console.WriteLine($"Game Over: {gameResult}");
            }

            return LoadRequested;
        }

        // implements a single move; strategy pattern handles game-specific rules
        public bool PlayMove(Move move)
        {
            if (gameResult != GameResult.OnGoing)
                return false;

            if (move is PassMove)
                return PassTurn();

            if (MustPass())
                return false;

            if (!MoveProcessor.IsMoveLegal(this, move))
                return false;

            SaveHistory();

            MoveProcessor.ApplyMove(this, move);

            TurnCount++;

            gameResult = GameOverChecker.Check(this);

            if (gameResult == GameResult.OnGoing)
                NextPlayer();

            return true;
        }

        public void DisplayGame()
        {
            
            VisibilityModel.RenderBoard(Board, CurrentPlayer);

            Console.WriteLine(
                $"Player {CurrentPlayer.Name}'s turn."
            );

            if (MustPass())
            {
                Console.WriteLine(
                    "No legal moves available. You must pass."
                );
            }
        }

        private void NextPlayer()  
        {
            // changes current player to next player in Players list,
            // when it reaches end of list it goes back to Players[0] 
            int currentIndex = Players.IndexOf(CurrentPlayer);

            int nextIndex = (currentIndex + 1) % Players.Count;

            CurrentPlayer = Players[nextIndex];
        }

        // Check if the current player must pass
        public bool MustPass()
        {
            if (gameResult != GameResult.OnGoing)
                return false;

            // Only Reversi allows passing
            if (MoveProcessor is not ReversiMoveProcessor)
                return false;

            return !MoveProcessor.HasLegalMoves(this, CurrentPlayer);

        }

        // Explicit pass command
        public bool PassTurn()
        {
            if (!MustPass())
                return false;

            SaveHistory();

            NextPlayer();

            gameResult = GameOverChecker.Check(this);

            return true;
        }

        public void UndoTurn()
        {
            // Restore previous GameState
        }

        public void RedoTurn()
        {
            // Restore next GameState
        }

        private void SaveHistory()
        {
            // Capture current GameState
        }
        public void SaveGame(string filePath)
        {
            // Save complete game data
        }

        public void RequestLoad()
        {
            LoadRequested = true;
            isRunning = false;
        }

        public void DisplayHelp()
        {
            // Show help for current game variant
        }    

        public void QuitGame()
        {
            isRunning = false;
        } 

        public void RestoreState(GameState state)
        {
            // Validate the active player
            if (state.CurrentPlayerIndex < 0 ||
                state.CurrentPlayerIndex >= Players.Count)
            {
                throw new InvalidDataException("Invalid player index.");
            }

            // Validate the number of inventories
            if (state.PlayerInventories.Count != Players.Count)
            {
                throw new InvalidDataException("Invalid number of player inventories.");
            }
            // Restore the board
            Board.RestoreRows(state.BoardRows);

            // Restore the current player
            CurrentPlayer = Players[state.CurrentPlayerIndex];

            // Restore turn count and game result
            TurnCount = state.TurnCount;
            gameResult = state.GameResult;

            // Restore each player's inventory
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].RestoreInventory(state.PlayerInventories[i]);
            }

        } 

        public void RestoreHistory( List<GameState> undoHistory, List<GameState> savedRedoHistory)
        {
            gameHistory.Clear(); // clears list
            gameHistory.AddRange(undoHistory); // adds to the end of the list

            redoHistory.Clear();
            redoHistory.AddRange(savedRedoHistory);
        }
    }
}