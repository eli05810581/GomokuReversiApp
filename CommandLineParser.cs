namespace GomokuReversiApp
{
    
public class CommandLineParser // this is for when program is started with args, command line input (> dotnet run -- --game gomoku --variant plus "O3:2,O3:3,O3:1,O4:3,O3:4,O5:3,E3:3,H3:3")
{
    public LaunchConfiguration Parse(string[] args)
    {
        string? gameInput = null;
        string? variantInput = null;
        string? movesInput = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--game":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("Missing game type.");

                    gameInput = args[++i].ToLowerInvariant();
                    break;

                case "--variant":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("Missing game variant.");

                    variantInput = args[++i].ToLowerInvariant();
                    break;

                default:
                    if (args[i].StartsWith("--") || movesInput != null)
                        throw new ArgumentException(
                            $"Unexpected argument: {args[i]}");

                    movesInput = args[i];
                    break;
            }
        }
        
        // determine game type
        GameType gameType;

        if (gameInput == "gomoku")
        {
            gameType = GameType.Gomoku;
        }
        else if (gameInput == "reversi")
        {
            gameType = GameType.Reversi;
        }
        else
        {
            throw new ArgumentException("Invalid game type.");
        }

        // determine game variant
        GameVariant variant;

        if (gameType == GameType.Gomoku)
        {
            if (variantInput == "standard")
            {
                variant = GameVariant.GomokuStandard;
            }
            else if (variantInput == "plus")
            {
                variant = GameVariant.GomokuPlus;
            }
            else if (variantInput == "fog")
            {
                variant = GameVariant.GomokuFog;
            }
            else
            {
                throw new ArgumentException("Invalid Gomoku variant.");
            }
        }
        else if (gameType == GameType.Reversi)
        {
            if (variantInput == "standard")
            {
                variant = GameVariant.ReversiStandard;
            }
            else if (variantInput == "anti")
            {
                variant = GameVariant.ReversiAnti;
            }
            else if (variantInput == "corner")
            {
                variant = GameVariant.ReversiCorner;
            }
            else
            {
                throw new ArgumentException("Invalid Reversi variant.");
            }
        }
        else
        {
            throw new ArgumentException("Invalid game type.");
        }
        if (string.IsNullOrWhiteSpace(movesInput))
        {
            throw new ArgumentException("A move sequence is required.");
        }

        List<string> moves = movesInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return new LaunchConfiguration
        {
            GameType = gameType,
            Variant = variant,
            GameMode = GameMode.Automated,
            AIDifficulty = null,
            MoveSequence = moves
        };
    }
}

}
