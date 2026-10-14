
namespace GomokuReversiApp
{
    // GoF Abstract Factory: defines the family of components needed by a Game.
    public interface IGameFactory
    {
        GameVariant Variant { get; }

        Game CreateGame(LaunchConfiguration config);
        Board CreateBoard();
        List<Player> CreatePlayers(GameMode gameMode, AIDifficulty? difficulty);
        IMoveParser CreateMoveParser();
        IMoveProcessor CreateMoveProcessor();
        IGameOverChecker CreateGameOverChecker();
        IVisibilityModel CreateVisibilityModel();
        IHelpProvider CreateHelpProvider();
        ICommandSource CreateCommandSource(LaunchConfiguration config, IMoveParser moveParser);
    }


    // Shared assembly logic; concrete variant factories supply the products.
    public abstract class GameFactoryBase : IGameFactory
    {
        public abstract GameVariant Variant { get; }

        public Game CreateGame(LaunchConfiguration config)
        {
            // Do not silently build one variant using another variant's factory.
            if (config.Variant != Variant)
                throw new ArgumentException(
                    $"Factory for {Variant} cannot create {config.Variant}.",
                    nameof(config));

            Board board = CreateBoard();
            List<Player> players = CreatePlayers(config.GameMode, config.AIDifficulty);
            IMoveParser moveParser = CreateMoveParser();
            IMoveProcessor moveProcessor = CreateMoveProcessor();
            IGameOverChecker gameOverChecker = CreateGameOverChecker();
            IVisibilityModel visibilityModel = CreateVisibilityModel();
            IHelpProvider helpProvider = CreateHelpProvider();

            // The exact same move-parser instance serves the Game and its input source.
            ICommandSource commandSource = CreateCommandSource(config, moveParser);

            return new Game(
                config,
                board,
                players,
                moveParser,
                moveProcessor,
                gameOverChecker,
                visibilityModel,
                helpProvider,
                commandSource);
        }

        public abstract Board CreateBoard();
        public abstract IMoveParser CreateMoveParser();
        public abstract IMoveProcessor CreateMoveProcessor();
        public abstract IGameOverChecker CreateGameOverChecker();
        public abstract IVisibilityModel CreateVisibilityModel();
        public abstract IHelpProvider CreateHelpProvider();

        // Player creation is identical across families except for inventory and AI.
        // virtual means derived class can override
        public virtual List<Player> CreatePlayers(GameMode gameMode, AIDifficulty? difficulty)
        {
            
            Dictionary<char, int?> inventory1 = CreateInventory(1);
            Dictionary<char, int?> inventory2 = CreateInventory(2);

            switch (gameMode)
            {
                case GameMode.HumanVsHuman:
                    return new List<Player>
                    {
                        new HumanPlayer("Player 1", 1, inventory1),
                        new HumanPlayer("Player 2", 2, inventory2)
                    };

                case GameMode.HumanVsAI:
                    return new List<Player>
                    {
                        new HumanPlayer("Player 1", 1, inventory1),
                        new AIPlayer("Computer", 2, inventory2, CreateAiStrategy(difficulty))
                    };

                case GameMode.Automated:
                    return new List<Player>
                    {
                        new ScriptedPlayer("Player 1", 1, inventory1),
                        new ScriptedPlayer("Player 2", 2, inventory2)
                    };

                default:
                    throw new ArgumentException("Invalid game mode.", nameof(gameMode));
            }
        }

        // protected means derived classes can access method
        protected abstract Dictionary<char, int?> CreateInventory(int playerNumber);
        protected abstract IMoveStrategy CreateSmartStrategy();

        private IMoveStrategy CreateAiStrategy(AIDifficulty? difficulty)
        {
            if (difficulty == null)
                throw new ArgumentException("AI difficulty must be specified.", nameof(difficulty));

            switch (difficulty.Value)
            {
                case AIDifficulty.Dumb:
                    return new RandomMoveStrategy();
                case AIDifficulty.Smart:
                    return CreateSmartStrategy();
                default:
                    throw new ArgumentException("Invalid AI difficulty.", nameof(difficulty));
            }
        }

        public virtual ICommandSource CreateCommandSource(LaunchConfiguration config,IMoveParser moveParser)
        {
            InputParser inputParser = new InputParser(moveParser);

            switch (config.GameMode)
            {
                case GameMode.HumanVsHuman:
                case GameMode.HumanVsAI:
                    return new InteractiveCommandSource(inputParser);

                case GameMode.Automated:
                    return new AutomatedCommandSource(config.MoveSequence, inputParser);

                default:
                    throw new ArgumentException("Invalid game mode.", nameof(config));
            }
        }
    }




    // Shared defaults for the three Gomoku variants.
    public abstract class GomokuFactoryBase : GameFactoryBase
    {
        public override Board CreateBoard() => new Board(10, 10);

        protected override Dictionary<char, int?> CreateInventory(int playerNumber)
        {
            return new Dictionary<char, int?> { { 'O', null } };
        }

        protected override IMoveStrategy CreateSmartStrategy()
        {
            return new GomokuSmartStrategy(5);
        }

        public override IMoveParser CreateMoveParser() => new GomokuMoveParser();
        public override IMoveProcessor CreateMoveProcessor() => new GomokuMoveProcessor();
        public override IGameOverChecker CreateGameOverChecker() => new AlignmentChecker(5);
        public override IVisibilityModel CreateVisibilityModel() => new FullVisibility();
    }




    // Shared defaults for the three Reversi variants.
    public abstract class ReversiFactoryBase : GameFactoryBase
    {
        public override Board CreateBoard()
        {
            Board board = new Board(8, 8);

            // B = Player 1, W = Player 2. Both centre rows contain 8 cells.
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

        protected override Dictionary<char, int?> CreateInventory(int playerNumber)
        {
            switch (playerNumber)
            {
                case 1:
                    return new Dictionary<char, int?> { { 'B', null } };
                case 2:
                    return new Dictionary<char, int?> { { 'W', null } };
                default:
                    throw new ArgumentException("Invalid player number.", nameof(playerNumber));
            }
        }

        public override IMoveParser CreateMoveParser() => new ReversiMoveParser();
        public override IMoveProcessor CreateMoveProcessor() => new ReversiMoveProcessor();
        public override IVisibilityModel CreateVisibilityModel() => new FullVisibility();
    }



    public class GomokuStandardGameFactory : GomokuFactoryBase
    {
        public override GameVariant Variant => GameVariant.GomokuStandard;
        public override IHelpProvider CreateHelpProvider() => new GomokuStandardHelpProvider();
    }



    public class GomokuPlusGameFactory : GomokuFactoryBase
    {
        private const int HeavyStoneLimit = 2;
        private const int EraserStoneLimit = 2;

        public override GameVariant Variant => GameVariant.GomokuPlus;

        protected override Dictionary<char, int?> CreateInventory(int playerNumber)
        {
            return new Dictionary<char, int?>
            {
                { 'O', null },
                { 'H', HeavyStoneLimit },
                { 'E', EraserStoneLimit }
            };
        }

        public override IMoveParser CreateMoveParser() => new GomokuPlusMoveParser();
        public override IMoveProcessor CreateMoveProcessor() => new GomokuPlusMoveProcessor();
        public override IHelpProvider CreateHelpProvider() => new GomokuPlusHelpProvider();
    }


    public class GomokuFogGameFactory : GomokuFactoryBase
    {
        public override GameVariant Variant => GameVariant.GomokuFog;
        public override IVisibilityModel CreateVisibilityModel() => new FogVisibility();
        public override IHelpProvider CreateHelpProvider() => new GomokuFogHelpProvider();
    }

    public class ReversiStandardGameFactory : ReversiFactoryBase
    {
        public override GameVariant Variant => GameVariant.ReversiStandard;
        protected override IMoveStrategy CreateSmartStrategy() => new GreedyReversiStrategy();
        public override IGameOverChecker CreateGameOverChecker() => new MajorityChecker();
        public override IHelpProvider CreateHelpProvider() => new ReversiStandardHelpProvider();
    }

    public class ReversiAntiGameFactory : ReversiFactoryBase
    {
        public override GameVariant Variant => GameVariant.ReversiAnti;
        protected override IMoveStrategy CreateSmartStrategy() => new AntiReversiStrategy();
        public override IGameOverChecker CreateGameOverChecker() => new MisereChecker();
        public override IHelpProvider CreateHelpProvider() => new ReversiAntiHelpProvider();
    }

    public class ReversiCornerGameFactory : ReversiFactoryBase
    {
        public override GameVariant Variant => GameVariant.ReversiCorner;
        protected override IMoveStrategy CreateSmartStrategy() => new CornerReversiStrategy();
        public override IGameOverChecker CreateGameOverChecker() => new CornerChecker();
        public override IHelpProvider CreateHelpProvider() => new ReversiCornerHelpProvider();
    }


    // Application/bootstrap selection, not part of the Game engine.
    public static class GameFactorySelector
    {
        public static IGameFactory Create(GameVariant variant)
        {
            switch (variant)
            {
                case GameVariant.GomokuStandard:
                    return new GomokuStandardGameFactory();
                case GameVariant.GomokuPlus:
                    return new GomokuPlusGameFactory();
                case GameVariant.GomokuFog:
                    return new GomokuFogGameFactory();
                case GameVariant.ReversiStandard:
                    return new ReversiStandardGameFactory();
                case GameVariant.ReversiAnti:
                    return new ReversiAntiGameFactory();
                case GameVariant.ReversiCorner:
                    return new ReversiCornerGameFactory();
                default:
                    throw new ArgumentException("Unsupported game variant.", nameof(variant));
            }
        }
    }

}
