namespace GomokuReversiApp
{
        public interface IGameFactory
    {
        // Abstract Factory
        // each concrete factory must implement the following creation methods:
        Game CreateGame(LaunchConfiguration config);
        Board CreateBoard(GameVariant gameVariant);

        List<Player> CreatePlayers(
            GameVariant variant, // needed for stone types
            GameMode gameMode,
            AIDifficulty? difficulty
        );
        
        IMoveParser CreateMoveParser(GameVariant gameVariant);

        IMoveProcessor CreateMoveProcessor(GameVariant gameVariant);

        IGameOverChecker CreateGameOverChecker(GameVariant gameVariant);

        IVisibilityModel CreateVisibilityModel(GameVariant gameVariant);

        

    }


    public class GomokuGameFactory : IGameFactory
    {
        //
        private const int HeavyStoneLimit = 2;
        private const int EraserStoneLimit = 2;

        public Game CreateGame(LaunchConfiguration config)
        {
            // Create the components
            Board board = CreateBoard(config.Variant);

            List<Player> players = CreatePlayers(
                config.Variant,
                config.GameMode,
                config.AIDifficulty
            );

            IMoveParser moveParser = CreateMoveParser(config.Variant);

            IMoveProcessor moveProcessor = CreateMoveProcessor(config.Variant);

            IGameOverChecker gameOverChecker = CreateGameOverChecker(config.Variant);

            IVisibilityModel visibilityModel = CreateVisibilityModel(config.Variant);
            
            ICommandSource commandSource = CreateCommandSource(config);

            // Assemble and return the game
            return new Game(
                board,
                players,
                moveParser,
                moveProcessor,
                gameOverChecker,
                visibilityModel,
                commandSource
            );
        }

        public Board CreateBoard(GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.GomokuStandard:
                case GameVariant.GomokuPlus:
                case GameVariant.GomokuFog:
                    return new Board(10, 10);

                default:
                    throw new ArgumentException(
                        "Invalid Gomoku variant."
                    );
            }
        }

        public List<Player> CreatePlayers(
            GameVariant variant,
            GameMode gameMode,
            AIDifficulty? difficulty
        )
        {
            List<Player> players = new List<Player>();

            // Player 1
            switch (gameMode)
            {
                case GameMode.HumanVsHuman: // both HumanVsHuman and HumanVsAI have human as player 1
                case GameMode.HumanVsAI:
                    players.Add(
                        new HumanPlayer(
                            "Player 1",
                            1,
                            CreateInventory(variant)
                        )
                    );
                    break;

                case GameMode.Automated:
                    players.Add(
                        new ScriptedPlayer(
                            "Player 1",
                            1,
                            CreateInventory(variant)
                        )
                    );
                    break;

                default:
                    throw new ArgumentException(
                        "Invalid game mode."
                    );
            }

            // Player 2
            switch (gameMode)
            {
                case GameMode.HumanVsHuman:
                    players.Add(
                        new HumanPlayer(
                            "Player 2",
                            2,
                            CreateInventory(variant)
                        )
                    );
                    break;

                case GameMode.HumanVsAI:

                    if (difficulty == null)
                    {
                        throw new ArgumentException(
                            "AI difficulty must be specified."
                        );
                    }

                    MoveStrategyBase strategy;

                    switch (difficulty.Value)
                    {
                        case AIDifficulty.Dumb:
                            strategy = new RandomMoveStrategy();
                            break;

                        case AIDifficulty.Smart:
                            strategy = new GomokuSmartStrategy(5);
                            break;

                        default:
                            throw new ArgumentException(
                                "Invalid AI difficulty."
                            );
                    }

                    players.Add(
                        new AIPlayer(
                            "Computer",
                            2,
                            CreateInventory(variant),
                            strategy
                        )
                    );
                    break;

                case GameMode.Automated:
                    players.Add(
                        new ScriptedPlayer(
                            "Player 2",
                            2,
                            CreateInventory(variant)
                        )
                    );
                    break;
            }

            return players;
        }

        // Create a separate inventory for each player
        private Dictionary<char, int?> CreateInventory(GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.GomokuStandard:
                case GameVariant.GomokuFog:
                    return new Dictionary<char, int?>
                    {
                        { 'O', null }
                    };

                case GameVariant.GomokuPlus:
                    return new Dictionary<char, int?>
                    {
                        { 'O', null },
                        { 'H', HeavyStoneLimit },
                        { 'E', EraserStoneLimit }
                    };

                default:
                    throw new ArgumentException(
                        "Invalid Gomoku variant."
                    );
            }
        }

        public IMoveParser CreateMoveParser(GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.GomokuStandard:
                case GameVariant.GomokuFog:
                    return new GomokuMoveParser();

                case GameVariant.GomokuPlus:
                    return new GomokuPlusMoveParser();

                default:
                    throw new ArgumentException(
                        "Invalid Gomoku variant."
                    );
            }
        }

        public IMoveProcessor CreateMoveProcessor(GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.GomokuStandard:
                case GameVariant.GomokuFog:
                    return new GomokuMoveProcessor();

                case GameVariant.GomokuPlus:
                    return new GomokuPlusMoveProcessor();

                default:
                    throw new ArgumentException(
                        "Invalid Gomoku variant."
                    );
            }
        }

        public IGameOverChecker CreateGameOverChecker(GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.GomokuStandard:
                case GameVariant.GomokuPlus:
                case GameVariant.GomokuFog:
                    return new AlignmentChecker(5);

                default:
                    throw new ArgumentException(
                        "Invalid Gomoku variant."
                    );
            }
        }

        public IVisibilityModel CreateVisibilityModel(GameVariant variant
        )
        {
            switch (variant)
            {
                case GameVariant.GomokuStandard:
                case GameVariant.GomokuPlus:
                    return new FullVisibility();

                case GameVariant.GomokuFog:
                    return new FogVisibility();

                default:
                    throw new ArgumentException(
                        "Invalid Gomoku variant."
                    );
            }
        }
    
        public ICommandSource CreateCommandSource(LaunchConfiguration config)
        {
            // Create the appropriate move parser for the game variant
            IMoveParser moveParser = CreateMoveParser(config.Variant);

            InputParser inputParser = new InputParser(moveParser);

            switch (config.GameMode)
            {
                case GameMode.HumanVsHuman:
                case GameMode.HumanVsAI:
                    return new InteractiveCommandSource(inputParser);

                case GameMode.Automated:
                    return new AutomatedCommandSource(
                        config.MoveSequence,
                        inputParser
                    );

                default:
                    throw new ArgumentException("Invalid game mode.");
            }
        }
    }


    public class ReversiGameFactory : IGameFactory
    {
        public Game CreateGame(LaunchConfiguration config)
        {
            Board board = CreateBoard(config.Variant);

            List<Player> players = CreatePlayers(
                config.Variant,
                config.GameMode,
                config.AIDifficulty
            );

            IMoveParser moveParser =
                CreateMoveParser(config.Variant);

            IMoveProcessor moveProcessor =
                CreateMoveProcessor(config.Variant);

            IGameOverChecker gameOverChecker =
                CreateGameOverChecker(config.Variant);

            IVisibilityModel visibilityModel =
                CreateVisibilityModel(config.Variant);

            ICommandSource commandSource =
                CreateCommandSource(config);

            return new Game(
                board,
                players,
                moveParser,
                moveProcessor,
                gameOverChecker,
                visibilityModel,
                commandSource
            );
        }

        public Board CreateBoard(GameVariant variant)
        {
            ValidateVariant(variant);

            Board board = new Board(8, 8);

            // Initialise the four starting disks.
            // B = Player 1, W = Player 2.
            board.RestoreRows(new List<string>
            {
                "........",
                "........",
                "........",
                "...WB...",
                "...BW...",
                "........",
                "........",
                "........"
            });

            return board;
        }

        public List<Player> CreatePlayers(
            GameVariant variant,
            GameMode gameMode,
            AIDifficulty? difficulty
        )
        {
            ValidateVariant(variant);

            List<Player> players = new List<Player>();

            // Each player has unlimited disks of their colour.
            Dictionary<char, int?> inventory1 =
                new Dictionary<char, int?> { { 'B', null } };

            Dictionary<char, int?> inventory2 =
                new Dictionary<char, int?> { { 'W', null } };

            // Player 1
            switch (gameMode)
            {
                case GameMode.HumanVsHuman:
                case GameMode.HumanVsAI:
                    players.Add(
                        new HumanPlayer(
                            "Player 1", 1, inventory1
                        )
                    );
                    break;

                case GameMode.Automated:
                    players.Add(
                        new ScriptedPlayer(
                            "Player 1", 1, inventory1
                        )
                    );
                    break;

                default:
                    throw new ArgumentException(
                        "Invalid game mode."
                    );
            }

            // Player 2
            switch (gameMode)
            {
                case GameMode.HumanVsHuman:
                    players.Add(
                        new HumanPlayer(
                            "Player 2", 2, inventory2
                        )
                    );
                    break;

                case GameMode.HumanVsAI:
                    if (difficulty == null)
                    {
                        throw new ArgumentException(
                            "AI difficulty must be specified."
                        );
                    }

                    MoveStrategyBase strategy;

                    switch (difficulty.Value)
                    {
                        case AIDifficulty.Dumb:
                            strategy = new RandomMoveStrategy();
                            break;

                        case AIDifficulty.Smart:
                            strategy = CreateSmartStrategy(variant);
                            break;

                        default:
                            throw new ArgumentException(
                                "Invalid AI difficulty."
                            );
                    }

                    players.Add(
                        new AIPlayer(
                            "Computer",
                            2,
                            inventory2,
                            strategy
                        )
                    );
                    break;

                case GameMode.Automated:
                    players.Add(
                        new ScriptedPlayer(
                            "Player 2", 2, inventory2
                        )
                    );
                    break;
            }

            return players;
        }

        // Select the appropriate Smart AI strategy
        private MoveStrategyBase CreateSmartStrategy(
            GameVariant variant
        )
        {
            switch (variant)
            {
                case GameVariant.ReversiStandard:
                    return new GreedyReversiStrategy();

                case GameVariant.ReversiAnti:
                    return new AntiReversiStrategy();

                case GameVariant.ReversiCorner:
                    return new CornerReversiStrategy();

                default:
                    throw new ArgumentException(
                        "Invalid Reversi variant."
                    );
            }
        }

        public IMoveParser CreateMoveParser(
            GameVariant variant
        )
        {
            ValidateVariant(variant);

            return new ReversiMoveParser();
        }

        public IMoveProcessor CreateMoveProcessor(
            GameVariant variant
        )
        {
            ValidateVariant(variant);

            return new ReversiMoveProcessor();
        }

        public IGameOverChecker CreateGameOverChecker( GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.ReversiStandard:
                    return new MajorityChecker();

                case GameVariant.ReversiAnti:
                    return new MisereChecker();

                case GameVariant.ReversiCorner:
                    return new CornerChecker();

                default:
                    throw new ArgumentException(
                        "Invalid Reversi variant."
                    );
            }
        }

        public IVisibilityModel CreateVisibilityModel(
            GameVariant variant
        )
        {
            ValidateVariant(variant);

            return new FullVisibility();
        }

        public ICommandSource CreateCommandSource(
            LaunchConfiguration config
        )
        {
            ValidateVariant(config.Variant);
            // Create the appropriate move parser for the game variant
            IMoveParser moveParser = CreateMoveParser(config.Variant);

            InputParser inputParser = new InputParser(moveParser);
            switch (config.GameMode)
            {
                case GameMode.HumanVsHuman:
                case GameMode.HumanVsAI:
                    return new InteractiveCommandSource(inputParser);

                case GameMode.Automated:
                    return new AutomatedCommandSource(config.MoveSequence,inputParser);

                default:
                    throw new ArgumentException(
                        "Invalid game mode."
                    );
            }
        }

        // Check that the variant belongs to Reversi
        private void ValidateVariant(GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.ReversiStandard:
                case GameVariant.ReversiAnti:
                case GameVariant.ReversiCorner:
                    return;

                default:
                    throw new ArgumentException(
                        "Invalid Reversi variant."
                    );
            }
        }
    }

}