namespace GomokuReversiApp
{
    public interface IHelpProvider
    {
        String GetHelp();
    }

    public class GomokuStandardHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            throw new NotImplementedException(
                "Standard Gomoku help is not implemented yet."
            );
        }
    }

        public class GomokuPlusHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            throw new NotImplementedException(
                "Gomoku Plus help is not implemented yet."
            );
        }
    }

        public class GomokuFogHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            throw new NotImplementedException(
                "Gomoku Fog help is not implemented yet."
            );
        }
    }

        public class ReversiStandardHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            throw new NotImplementedException(
                "Standard Reversi help is not implemented yet."
            );
        }
    }

        public class ReversiAntiHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            throw new NotImplementedException(
                "Anti Reversi help is not implemented yet."
            );
        }
    }

        public class ReversiCornerHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            throw new NotImplementedException(
                "Corner Reversi help is not implemented yet."
            );
        }
    }
}