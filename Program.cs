namespace GomokuReversiApp {
    public class Program
    {
        private static Game? currentGame;
        public static void Main(string[] args)
        {
            LaunchConfiguration config;

            // Check whether command-line arguments were supplied
            if (args.Length > 0)
            {
                CommandLineParser parser = new CommandLineParser();
                config = parser.Parse(args); //???

                // Select the factory
                IGameFactory factory;

                if (config.GameType == GameType.Gomoku)
                {
                    factory = new GomokuGameFactory();
                }
                else if (config.GameType == GameType.Reversi)
                {
                    factory = new ReversiGameFactory();
                }
                else
                {
                    throw new ArgumentException("Invalid game type.");
                }

                // Create game
                Game game = factory.CreateGame(config);

                // Start game
                bool loadRequested = game.PlayGame();
            }
            else
            {
                MainMenu();
            }



        }

        private static void MainMenu()
        {
            bool running = true;

            while (running)
            {
                if (currentGame != null)
                {
                    Game game = currentGame; // game is a local variable that remembers which game was running when PlayGame() was started

                    game.PlayGame();

                    // If the game wasn't replaced by loading,
                    // return to the main menu.
                    if (currentGame == game)
                    {
                        currentGame = null;
                    }
                }
                else
                {
                    Console.WriteLine("=== GomokuReversi ===");
                    Console.WriteLine("1. New Game");
                    Console.WriteLine("2. Load Game");
                    Console.WriteLine("3. Exit");

                    string choice = Console.ReadLine() ?? "";

                    switch (choice)
                    {
                        case "1":
                            LaunchConfiguration config = ConfigureNewGame();
                            StartNewGame(config);
                            break;

                        case "2":
                            LoadGame();
                            break;

                        case "3":
                            running = false;
                            break;

                        default:
                            Console.WriteLine("Invalid selection.");
                            break;
                    } 
                }

            }
        }
    
        private static LaunchConfiguration ConfigureNewGame() // for configureing new interactive games
        {
            LaunchConfiguration config = new LaunchConfiguration();

            // Select game type
            Console.WriteLine("\nSelect Game Type:");
            Console.WriteLine("1. Gomoku");
            Console.WriteLine("2. Reversi");

            int gameChoice = ReadMenuChoice(1, 2);

            switch (gameChoice)
            {
                case 1:
                    config.GameType = GameType.Gomoku;
                    break;

                case 2:
                    config.GameType = GameType.Reversi;
                    break;
            }

            // Select game variant
            Console.WriteLine("\nSelect Game Variant:");

            if (config.GameType == GameType.Gomoku)
            {
                Console.WriteLine("1. Standard Gomoku");
                Console.WriteLine("2. GomokuPlus");
                Console.WriteLine("3. GomokuFog");

                int variantChoice = ReadMenuChoice(1, 3);

                switch (variantChoice)
                {
                    case 1:
                        config.Variant = GameVariant.GomokuStandard;
                        break;

                    case 2:
                        config.Variant = GameVariant.GomokuPlus;
                        break;

                    case 3:
                        config.Variant = GameVariant.GomokuFog;
                        break;
                }
            }
            else
            {
                Console.WriteLine("1. Standard Reversi");
                Console.WriteLine("2. Anti Reversi");
                Console.WriteLine("3. Corner Reversi");

                int variantChoice = ReadMenuChoice(1, 3);

                switch (variantChoice)
                {
                    case 1:
                        config.Variant = GameVariant.ReversiStandard;
                        break;

                    case 2:
                        config.Variant = GameVariant.ReversiAnti;
                        break;

                    case 3:
                        config.Variant = GameVariant.ReversiCorner;
                        break;
                }
            }

            // Select game mode
            Console.WriteLine("\nSelect Game Mode:");
            Console.WriteLine("1. Human vs Human");
            Console.WriteLine("2. Human vs Dumb AI");
            Console.WriteLine("3. Human vs Smart AI");

            int modeChoice = ReadMenuChoice(1, 3);

            switch (modeChoice)
            {
                case 1:
                    config.GameMode = GameMode.HumanVsHuman;
                    break;

                case 2:
                    config.GameMode = GameMode.HumanVsAI;
                    config.AIDifficulty = AIDifficulty.Dumb;
                    break;

                case 3:
                    config.GameMode = GameMode.HumanVsAI;
                    config.AIDifficulty = AIDifficulty.Smart;
                    break;
            }
            return config;
        }

        private static int ReadMenuChoice(int min, int max)
        {
            while (true)
            {
                Console.Write("\nEnter choice: ");

                string input = Console.ReadLine() ?? "";

                if (int.TryParse(input, out int choice) &&
                    choice >= min &&
                    choice <= max)
                {
                    return choice;
                }

                Console.WriteLine(
                    $"Invalid choice. Enter a number between {min} and {max}.");
            }
        }

        private static void StartNewGame(LaunchConfiguration config)
        {
            IGameFactory factory;

            if (config.GameType == GameType.Gomoku)
            {
                factory = new GomokuGameFactory();
            }
            else
            {
                factory = new ReversiGameFactory();
            }

            Game game = factory.CreateGame(config);

            game.PlayGame();
        }   

        public static void LoadGame()
        {
            // Read saved file and reconstruct the game
            Game? loadedGame = ReadSavedGame();

            if (loadedGame == null)
            {
                return;
            }

            // Stop the existing game
            currentGame?.QuitGame();

            // Replace it with the loaded game
            currentGame = loadedGame;
        }

        private static Game? ReadSavedGame()
        {
            Console.Write("Enter save file name: ");
            string fileName = Console.ReadLine() ?? "";

            if (!File.Exists(fileName))
            {
                Console.WriteLine("Save file not found.");
                return null;
            }
            try
            {
                // Read the saved data
                string[] lines = File.ReadAllLines(fileName);

                // Parse the saved configuration and game state
                SavedGameData savedData = ParseSavedGame(lines);

                if (savedData == null)
                {
                    Console.WriteLine("Invalid save file.");
                    return null;
                }

                // Reconstruct the game's configuration
                LaunchConfiguration config = savedData.Configuration;

                // Select the appropriate factory
                IGameFactory factory;

                if (savedData.Configuration.GameType == GameType.Gomoku)
                {
                    factory = new GomokuGameFactory();
                }
                else if (savedData.Configuration.GameType == GameType.Reversi)
                {
                    factory = new ReversiGameFactory();
                }
                else
                {
                    throw new FormatException("Invalid game type.");
                }

                // Create the game with the correct components
                Game game = factory.CreateGame(savedData.Configuration);

                // Restore the saved game state
                game.RestoreState(savedData.CurrentState);

                // Restore undo and redo histories
                game.RestoreHistory(
                    savedData.UndoHistory,
                    savedData.RedoHistory
                );

                // Return the reconstructed game
                return game;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to load game: {ex.Message}");
                return null;
            }
        }

        private static SavedGameData ParseSavedGame(string[] lines)
        {
            Dictionary<string, string> configFields = new();

            Dictionary<string, string>? currentStateFields = null;

            List<Dictionary<string, string>> undoStates = new();
            List<Dictionary<string, string>> redoStates = new();

            Dictionary<string, string>? activeSection = null;

            // Read each section from the text file
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line == "" || line.StartsWith("#"))
                {
                    continue;
                }

                // Identify section headings
                switch (line)
                {
                    case "[CONFIGURATION]":
                        activeSection = configFields;
                        continue;

                    case "[CURRENT_STATE]":
                        currentStateFields = new Dictionary<string, string>();
                        activeSection = currentStateFields;
                        continue;

                    case "[UNDO_STATE]":
                        activeSection = new Dictionary<string, string>();
                        undoStates.Add(activeSection);
                        continue;

                    case "[REDO_STATE]":
                        activeSection = new Dictionary<string, string>();
                        redoStates.Add(activeSection);
                        continue;
                }

                if (activeSection == null)
                {
                    throw new FormatException("Data found outside a section.");
                }

                // Split each line into key and value
                int separator = line.IndexOf('=');

                if (separator <= 0)
                {
                    throw new FormatException($"Invalid line: {line}");
                }

                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim();

                activeSection[key] = value;
            }

            if (currentStateFields == null)
            {
                throw new FormatException("Missing current game state.");
            }

            // Reconstruct launch configuration
            LaunchConfiguration config = new LaunchConfiguration
            {
                GameType = Enum.Parse<GameType>(
                    configFields["GameType"]),

                Variant = Enum.Parse<GameVariant>(
                    configFields["Variant"]),

                GameMode = Enum.Parse<GameMode>(
                    configFields["GameMode"])
            };

            if (configFields.TryGetValue("AIDifficulty", out string? difficulty))
            {
                config.AIDifficulty = Enum.Parse<AIDifficulty>(difficulty);
            }

            // Construct the saved data object
            SavedGameData savedData = new SavedGameData
            {
                Configuration = config,

                CurrentState = ParseGameState(currentStateFields),

                UndoHistory = undoStates
                    .Select(fields => ParseGameState(fields))
                    .ToList(),

                RedoHistory = redoStates
                    .Select(fields => ParseGameState(fields))
                    .ToList()
            };

            return savedData;
        } 

        private static GameState ParseGameState(
            Dictionary<string, string> fields)
        {
            GameState state = new GameState
            {
                CurrentPlayerIndex = int.Parse(
                    fields["CurrentPlayerIndex"]),

                TurnCount = int.Parse(
                    fields["TurnCount"]),

                GameResult = Enum.Parse<GameResult>(
                    fields["GameResult"]),

                BoardRows = fields["Board"].Split('/').ToList(),

                PlayerInventories = new List<Dictionary<char, int?>>
                {
                    ParseInventory(fields["Player1Inventory"]),
                    ParseInventory(fields["Player2Inventory"])
                }
            };

            return state;
        }

        // helper methods
        private static Dictionary<char, int?> ParseInventory(string input)
        {
            Dictionary<char, int?> inventory =
                new Dictionary<char, int?>();

            string[] items = input.Split(',');

            foreach (string item in items)
            {
                string[] parts = item.Split(':');

                char symbol = char.Parse(parts[0]);

                int? amount = parts[1] == "null"
                    ? null
                    : int.Parse(parts[1]);

                inventory.Add(symbol, amount);
            }

            return inventory;
        }

    }
    





}
