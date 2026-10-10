namespace GomokuReversiApp {
    public class Program
    {
        public static void Main(string[] args)
        {
            LaunchConfiguration config;

            // Check whether command-line arguments were supplied
            if (args.Length > 0)
            {
                CommandLineParser parser = new CommandLineParser();
                config = parser.Parse(args);
            }
            else
            {
                MainMenu();
            }

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

            if (loadRequested)
            {
                // Load another game
            }
            else
            {
                // Return to main menu
            }
        }

        private static void MainMenu()
        {
            bool running = true;

            while (running)
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
    
        private static LaunchConfiguration ConfigureNewGame()
        {
            LaunchConfiguration config = new LaunchConfiguration();

            // Ask for game type
            // Ask for game variant
            // Ask for game mode
            // Ask for AI difficulty if applicable

            return config;
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

        private static void LoadGame()
        {
            Console.WriteLine("Enter saved game filename:");
            string filename = Console.ReadLine() ?? "";

            // Load the saved game and reconstruct its state.
            // This needs to return a Game instance.

            // Game game = ...
            // game.PlayGame();
        }     
    }
    
    public class LaunchConfiguration
    {
        public required GameType GameType { get; init; }
        public required GameVariant Variant { get; init; }
        public required GameMode GameMode { get; init; }

        public AIDifficulty? AIDifficulty { get; init; }

        public List<string> MoveSequence { get; init; } = new();
            
    }







}
