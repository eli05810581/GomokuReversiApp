
namespace GomokuReversiApp
{
    public interface IHelpProvider
    {
        string GetHelp();
    }

    public class GomokuStandardHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            return "GOMOKU STANDARD HELP\n" +
                   "Board size: 10 x 10\n" +
                   "Players take turns placing stones.\n" +
                   "Move format: O5:3 (row 5, column 3)\n" +
                   "Get 5 stones in a row to win.\n" +
                   "\nCommands:\n" +
                   "HELP - Show help\n" +
                   "UNDO - Undo a move\n" +
                   "REDO - Redo a move\n" +
                   "SAVE filename - Save the game\n" +
                   "LOAD - Load a game\n" +
                   "QUIT - Exit the game";
        }
    }

    public class GomokuPlusHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            return "GOMOKU PLUS HELP\n" +
                   "Board size: 10 x 10\n" +
                   "Get 5 stones in a row to win.\n" +
                   "\nStone types:\n" +
                   "O - Ordinary stone (unlimited)\n" +
                   "H - Heavy stone (2 per player)\n" +
                   "E - Eraser stone (2 per player)\n" +
                   "\nMove examples:\n" +
                   "O5:3 - Place an ordinary stone\n" +
                   "H5:3 - Place a heavy stone\n" +
                   "E5:3 - Use an eraser\n" +
                   "\nCommands:\n" +
                   "HELP, UNDO, REDO, LOAD, QUIT\n" +
                   "SAVE filename - Save the game";
        }
    }

    public class GomokuFogHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            return "GOMOKU FOG HELP\n" +
                   "Board size: 10 x 10\n" +
                   "Get 5 stones in a row to win.\n" +
                   "Some parts of the board are hidden by fog.\n" +
                   "Move format: O5:3\n" +
                   "\nCommands:\n" +
                   "HELP, UNDO, REDO, LOAD, QUIT\n" +
                   "SAVE filename - Save the game";
        }
    }

    public class ReversiStandardHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            return "REVERSI STANDARD HELP\n" +
                   "Board size: 8 x 8\n" +
                   "Players place disks to capture opponent disks.\n" +
                   "Captured disks change to your colour.\n" +
                   "The player with more disks wins.\n" +
                   "Move format: P3:4 (row 3, column 4)\n" +
                   "\nCommands:\n" +
                   "HELP, UNDO, REDO, LOAD, QUIT\n" +
                   "SAVE filename - Save the game";
        }
    }

    public class ReversiAntiHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            return "REVERSI ANTI HELP\n" +
                   "Board size: 8 x 8\n" +
                   "Players place disks to capture opponent disks.\n" +
                   "Captured disks change to your colour.\n" +
                   "The goal is to have fewer disks at the end.\n" +
                   "Move format: P3:4\n" +
                   "\nCommands:\n" +
                   "HELP, UNDO, REDO, LOAD, QUIT\n" +
                   "SAVE filename - Save the game";
        }
    }

    public class ReversiCornerHelpProvider : IHelpProvider
    {
        public string GetHelp()
        {
            return "REVERSI CORNER HELP\n" +
                   "Board size: 8 x 8\n" +
                   "Players place disks to capture opponent disks.\n" +
                   "Captured disks change to your colour.\n" +
                   "Corners are important in this game variant.\n" +
                   "Move format: P3:4\n" +
                   "\nCommands:\n" +
                   "HELP, UNDO, REDO, LOAD, QUIT\n" +
                   "SAVE filename - Save the game";
        }
    }
}
