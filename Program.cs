namespace GomokuReversiApp {
    public class Program
    {
        private static Game? currentGame;
        public static void Main(string[] args)
        {
            // Check whether command-line arguments were supplied
            if (args.Length > 0)
            {
                CommandLineParser parser = new CommandLineParser();

                LaunchConfiguration config = parser.Parse(args);

                RunGameSession(CreateGame(config));
            }
            else
            {
                MainMenu();
            }
        }

        private static void MainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== GomokuReversi ===");
                Console.WriteLine("1. New Game");
                Console.WriteLine("2. Load Game");
                Console.WriteLine("3. Exit");

                int choice = ReadMenuChoice(1,3);

                switch (choice)
                {
                    case 1:
                        LaunchConfiguration config = ConfigureNewGame();
                        RunGameSession(CreateGame(config));
                        break;

                    case 2:
                        Game? loadedGame = TryLoadGame();

                        if (loadedGame != null)
                        {
                            RunGameSession(loadedGame);
                        }
                        break;

                    case 3:
                        return;

                    default: // possibly can't reach here cause of ReadMenuChoice
                        Console.WriteLine("Invalid selection.");
                        break;
                }
            }
        } 
        private static LaunchConfiguration ConfigureNewGame() // for configuring new interactive games
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

 
        private static Game CreateGame(LaunchConfiguration config)
        {
            IGameFactory factory = GameFactorySelector.Create(config.Variant);

            return factory.CreateGame(config);
        }       
        private static void RunGameSession(Game game)
        {
            while (true)
            {
                // Play until the game ends, quits, or requests a load
                bool loadRequested = game.PlayGame();

                // Quit or game over: return to the main menu
                if (!loadRequested)
                {
                    return;
                }

                // User requested a load
                Game? loadedGame = TryLoadGame();

                if (loadedGame != null)
                {
                    // Replace the active game
                    game = loadedGame;
                    Console.WriteLine("Game loaded successfully.");
                }
                else
                {
                    // Keep the existing game
                    Console.WriteLine("Continuing previous game.");
                }
            }
        }       
        private static Game? TryLoadGame()
        {
            Console.Write("Enter save file name (blank to cancel): ");
            string filePath = (Console.ReadLine() ?? "").Trim();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            try
            {
                // Read and parse the saved data
                SavedGameData data = SaveFileManager.Load(filePath);

                // Construct the correct game variant
                Game game = CreateGame(data.Configuration);

                // Restore the game state
                game.RestoreState(data.CurrentState);

                // Restore undo and redo histories
                game.RestoreHistory(
                    data.UndoHistory,
                    data.RedoHistory
                );

                return game;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to load game: {ex.Message}");
                return null;
            }
        }
    
    }
    
}
