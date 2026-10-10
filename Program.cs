namespace GomokuReversiApp {
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                // logic for automated testing mode
            }
            else
            {
                // logic for normal interactive mode
            }
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
