namespace GomokuReversiApp
{
        public enum GameType
    {
        Gomoku,
        Reversi
    }

    public enum GameMode
    {
        HumanVsHuman,
        HumanVsAI,
        Automated
    }

    public enum AIDifficulty
    {
        Dumb,
        Smart
    }

    public enum GameVariant
    {
        GomokuStandard,
        GomokuPlus,
        GomokuFog,
        ReversiStandard,
        ReversiAnti,
        ReversiCorner
    }

    public enum GameResult { OnGoing, Player1Wins, Player2Wins, Draw}
}