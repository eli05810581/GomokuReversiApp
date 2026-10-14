using System.ComponentModel.Design;
using System.Text.RegularExpressions;

namespace GomokuReversiApp
{
    public class Game
    {
        // game components
        public LaunchConfiguration Configuration { get; }
        public Board Board { get; private set; }
        public List<Player> Players { get; private set; }

        // Strategies / objects supplied by the factory
        public IMoveParser MoveParser { get; private set; }
        public IMoveProcessor MoveProcessor { get; protected set; }
        public IGameOverChecker GameOverChecker { get; protected set; }
        public IVisibilityModel VisibilityModel { get; protected set; }
        public ICommandSource CommandSource { get; protected set; }
        public IHelpProvider HelpProvider { get; protected set; }

        public Player CurrentPlayer { get; private set; }
        public int TurnCount { get; private set; }
        public GameResult Result { get; private set; }

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

            // Main game loop
            while (isRunning && Result == GameResult.OnGoing)
            {
                if (!automated)
                {
                    DisplayGame();
                }

                ICommand command = CommandSource.GetNextCommand(this);

                command.Execute(this);
            }

            if (!loadRequested &&
                (automated || Result != GameResult.OnGoing))
            {
                DisplayGame();
            }

            return loadRequested;
        }

        // Implements a single move
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

            EndTurn();

            return true;
        }

        private void EndTurn()
        {
            TurnCount++;

            Result = GameOverChecker.Check(this);

            if (Result == GameResult.OnGoing)
            {
                NextPlayer();
            }
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
            int currentIndex = Players.IndexOf(CurrentPlayer);

            int nextIndex = (currentIndex + 1) % Players.Count;

            CurrentPlayer = Players[nextIndex];
        }

        // Check if the current player must pass
        public bool MustPass()
        {
            if (Result != GameResult.OnGoing)
                return false;

            if (!MoveProcessor.AllowPass)
                return false;

            return !MoveProcessor.HasLegalMoves(this, CurrentPlayer);
        }

        // Explicit pass command
        public bool PassTurn()
        {
            if (!MustPass())
                return false;

            SaveHistory();

            EndTurn();

            return true;
        }

        public bool UndoTurn()
        {
            // Undo a full turn
            if (gameHistory.Count < 2)
                return false;

            redoHistory.Add(CaptureState());

            GameState afterOwnMove =
                gameHistory[gameHistory.Count - 1];

            gameHistory.RemoveAt(gameHistory.Count - 1);

            redoHistory.Add(afterOwnMove);

            GameState beforeOwnMove =
                gameHistory[gameHistory.Count - 1];

            gameHistory.RemoveAt(gameHistory.Count - 1);

            RestoreState(beforeOwnMove);

            return true;
        }

        public bool RedoTurn()
        {
            if (redoHistory.Count < 2)
                return false;

            gameHistory.Add(CaptureState());

            GameState middle =
                redoHistory[redoHistory.Count - 1];

            redoHistory.RemoveAt(redoHistory.Count - 1);

            gameHistory.Add(middle);

            GameState target =
                redoHistory[redoHistory.Count - 1];

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

                PlayerInventories = Players
                    .Select(p =>
                        new Dictionary<char, int?>(p.Inventory))
                    .ToList()
            };
        }

        private void SaveHistory()
        {
            gameHistory.Add(CaptureState());

            redoHistory.Clear();
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

                Console.WriteLine(
                    $"Game saved to {filePath}."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Unable to save game: {ex.Message}"
                );
            }
        }

        public void RequestLoad()
        {
            loadRequested = true;
            isRunning = false;
        }

        // UPDATED: Display Help and remaining inventories
        public void DisplayHelp()
        {
            // Show help for current game variant
            Console.WriteLine(HelpProvider.GetHelp());

            // Show remaining special inventories for GomokuPlus
            if (HelpProvider is GomokuPlusHelpProvider)
            {
                Console.WriteLine(
                    "\nRemaining Special Inventory:"
                );

                foreach (Player player in Players)
                {
                    Console.WriteLine(
                        $"\nPlayer {player.Number} - {player.Name}:"
                    );

                    foreach (char stoneType in new[] { 'H', 'E' })
                    {
                        string stoneName = stoneType == 'H'
                            ? "Heavy"
                            : "Eraser";

                        if (player.Inventory.TryGetValue(
                            stoneType,
                            out int? remaining))
                        {
                            string quantity = remaining.HasValue
                                ? remaining.Value.ToString()
                                : "Unlimited";

                            Console.WriteLine(
                                $"{stoneName} ({stoneType}): {quantity}"
                            );
                        }
                    }
                }
            }
        }

        public void QuitGame()
        {
            isRunning = false;
        }

        public void RestoreState(GameState state)
        {
            // Validate active player
            if (state.CurrentPlayerIndex < 0 ||
                state.CurrentPlayerIndex >= Players.Count)
            {
                throw new InvalidDataException(
                    "Invalid player index."
                );
            }

            // Validate inventory count
            if (state.PlayerInventories.Count != Players.Count)
            {
                throw new InvalidDataException(
                    "Invalid number of player inventories."
                );
            }

            // Restore board
            Board.RestoreRows(state.BoardRows);

            // Restore current player
            CurrentPlayer = Players[state.CurrentPlayerIndex];

            // Restore turn count and result
            TurnCount = state.TurnCount;
            Result = state.Result;

            // Restore inventories
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].RestoreInventory(
                    state.PlayerInventories[i]
                );
            }
        }

        public void RestoreHistory(
            List<GameState> undoHistory,
            List<GameState> savedRedoHistory)
        {
            gameHistory.Clear();
            gameHistory.AddRange(undoHistory);

            redoHistory.Clear();
            redoHistory.AddRange(savedRedoHistory);
        }
    }
}