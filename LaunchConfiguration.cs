namespace GomokuReversiApp
{
    public class LaunchConfiguration // this class has all the information needed to start a new game, whether automated or selected from menu
    {
        public GameType GameType { get; set; }
        public GameVariant Variant { get; set; }
        public GameMode GameMode { get; set; }

        public AIDifficulty? AIDifficulty { get; set; }

        public List<string> MoveSequence { get; set; } = new();
    }

}