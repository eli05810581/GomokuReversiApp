namespace GomokuReversiApp
{
        public interface IGameFactory
    {
        // Abstract Factory
        // each concrete factory must implement the following creation methods:
        Game CreateGame(LaunchConfiguration config);
        Board CreateBoard(GameVariant gameVariant);

        public List<Player> CreatePlayers (
            GameMode gameMode,
            AIDifficulty? difficulty
            );
        
        IMoveParser CreateMoveParser(GameVariant gameVariant);

        IMoveProcessor CreateMoveProcessor(GameVariant gameVariant);

        IGameOverChecker CreateGameOverChecker(GameVariant gameVariant);

        IVisibilityModel CreateVisibilityModel(GameVariant gameVariant);

        
// separate
    }

    public class GomokuGameFactory : IGameFactory
    {
        public Board CreateBoard(GameVariant variant) 
        {
            // all gomoku variants use the same size board, for now...
            if (variant == GameVariant.GomokuStandard ||
            variant == GameVariant.GomokuPlus ||
            variant == GameVariant.GomokuFog)
            {
                return new Board(10, 10);
            }

            throw new ArgumentException("Cannot create board: Invalid Gomoku variant.");
        }

        public Game CreateGame(LaunchConfiguration config)
        {
            Board board = CreateBoard(config.Variant);

            List<Player> players = CreatePlayers (
                GameMode gameMode,
                AIDifficulty? difficulty
            );
        
        IMoveParser moveParser = CreateMoveParser(config.Variant);

        IMoveProcessor moveProcessor = CreateMoveProcessor(config.Variant);

        IGameOverChecker gameOverChecker = CreateGameOverChecker(config.Variant);

        IVisibilityModel visibilityModel = CreateVisibilityModel(config.Variant);
        }
        
    }

    public class ReversiGameFactory : IGameFactory
    {
        
    }


}