using System.ComponentModel.Design;
using System.Text.RegularExpressions;

namespace GomokuReversiApp
{
        public class Game
    {
        // game components
        public LaunchConfiguration Configuration {get;}
        public Board Board { get; private set;}
        public List<Player> Players {get; private set;}

        // Strategies / objects supplied by the factory
        public IMoveParser MoveParser { get; private set; } 
        public IMoveProcessor MoveProcessor { get; protected set; }
        public IGameOverChecker GameOverChecker { get; protected set; }
        public IVisibilityModel VisibilityModel { get; protected set; }
        public ICommandSource CommandSource { get; protected set; }
        public IHelpProvider HelpProvider { get; protected set; }


        public Player CurrentPlayer { get; private set;}
        public int TurnCount { get; private set;}
        public GameResult Result {get; private set;}
        private bool isRunning;
        private bool loadRequested;

        // undo/redo history
        private List<GameState> gameHistory = new List<GameState>();
        private List<GameState> redoHistory = new List<GameState>();

        // constructor
        public Game(
            LaunchConfiguration configuration,
            Board board,
            List<Player> players,
            IMoveParser moveParser,
            IMoveProcessor moveProcessor,
            IGameOverChecker gameOverChecker,
            IVisibilityModel visibilityModel,
            IHelpProvider helpProvider,
            ICommandSource commandSource
        )
        {
            Configuration = configuration;
            Board = board;
            Players = players;

            MoveParser = moveParser;
            MoveProcessor = moveProcessor;
            GameOverChecker = gameOverChecker;
            VisibilityModel = visibilityModel;
            CommandSource = commandSource;
            HelpProvider = helpProvider;

            CurrentPlayer = Players[0];
            TurnCount = 0;
            Result = GameResult.OnGoing;

            // gameHistory = new List<GameState>();
            // redoHistory = new List<GameState>();
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
            loadRequested = false;
            bool automated = Configuration.GameMode == GameMode.Automated;

            while (isRunning && Result == GameResult.OnGoing)
            {

                if (!automated) DisplayGame();

                ICommand command = CommandSource.GetNextCommand(this);

                command.Execute(this);
            }

            if (!loadRequested && (automated || Result != GameResult.OnGoing)) // Scripts show only the final state; finished games show the result
            {
                DisplayGame();
                // Console.WriteLine($"Game Over: {gameResult}");
            }

            return loadRequested;
        }

        // implements a single move; strategy pattern handles game-specific rules
        public bool PlayMove(Move move)
        {
            if (Result != GameResult.OnGoing)
                return false;

            if (move is PassMove)
                return PassTurn();

            if (MustPass())
                return false;

            if (!MoveProcessor.IsMoveLegal(this, move))
                return false;

            SaveHistory();

            MoveProcessor.ApplyMove(this, move);

            Result = GameOverChecker.Check(this);
            EndTurn(); // this will increment the turn count
            // if (gameResult == GameResult.OnGoing)
            //     NextPlayer();

            return true;
        }

        private void EndTurn() // Check the result, then hand the turn to the next player
        {
            TurnCount++;
            Result = GameOverChecker.Check(this);

            if (Result == GameResult.OnGoing)
                NextPlayer();
        }

        public void DisplayGame()
        {

                VisibilityModel.RenderBoard(Board, CurrentPlayer);
            

            if (Result != GameResult.OnGoing)
            {
                Console.WriteLine($"Game over: {Result}");
                return;
            }

            Console.WriteLine(
                $"Player {CurrentPlayer.Name}'s turn."
            );

            if (MustPass())
            {
                Console.WriteLine(
                    "No legal moves available. You must PASS."
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
            if (Result != GameResult.OnGoing)
                return false;

            // Need to change to below, because once another game also has PASS move, need to modify the game engine
            if (!MoveProcessor.AllowPass) return false;

            return !MoveProcessor.HasLegalMoves(this, CurrentPlayer);

        }

        // Explicit pass command
        public bool PassTurn()
        {
            if (!MustPass())
                return false;

            SaveHistory();
            EndTurn();
            // NextPlayer();

            // gameResult = GameOverChecker.Check(this);

            return true;
        }

        public bool UndoTurn()
        {
            // Restore previous GameState
            // Undo a full turn: the opponent's last move and the requester's own move before it
            if (gameHistory.Count < 2) return false;

            redoHistory.Add(CaptureState());

            GameState afterOwnMove = gameHistory[gameHistory.Count - 1];
            gameHistory.RemoveAt(gameHistory.Count - 1);
            redoHistory.Add(afterOwnMove);
            GameState beforeOwnMove = gameHistory[gameHistory.Count - 1];
            gameHistory.RemoveAt(gameHistory.Count - 1);
            RestoreState(beforeOwnMove);

            return true;
        }

        public bool RedoTurn()
        {
            // Restore next GameState
            if (redoHistory.Count < 2) return false;

            gameHistory.Add(CaptureState());

            GameState middle = redoHistory[redoHistory.Count - 1];
            redoHistory.RemoveAt(redoHistory.Count - 1);
            gameHistory.Add(middle);
            GameState target = redoHistory[redoHistory.Count - 1];
            redoHistory.RemoveAt(redoHistory.Count - 1);
            RestoreState(target);

            return true;
        }

        public GameState CaptureState()
        {
            return new GameState
            {
                CurrentPlayerIndex = Players.IndexOf(CurrentPlayer),
                TurnCount = TurnCount,
                Result = Result,
                BoardRows = Board.CaptureRows().ToList(),
                PlayerInventories = Players.Select(p => new Dictionary<char, int?>(p.Inventory)).ToList()
            };
        }

        private void SaveHistory()
        {
            // Capture current GameState
            gameHistory.Add(CaptureState());
            redoHistory.Clear(); // A new move clears the redo history
        }
        public void SaveGame(string filePath)
        {
            try
            {
                SavedGameData data = new SavedGameData
                {
                    Configuration = Configuration,
                    CurrentState = CaptureState(),
                    UndoHistory = new List<GameState>(gameHistory),
                    RedoHistory = new List<GameState>(redoHistory)
                };

                SaveFileManager.Save(filePath, data);
                Console.WriteLine($"Game saved to {filePath}.");
            } catch (Exception ex)
            {
                Console.WriteLine($"Unable to save game: {ex.Message}");
            }
        }

        public void RequestLoad()
        {
            loadRequested = true;
            isRunning = false;
        }

        public void DisplayHelp()
        {
            // Show help for current game variant
            Console.WriteLine(HelpProvider.GetHelp());
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
            Result = state.Result;

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