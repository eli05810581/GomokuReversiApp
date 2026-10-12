
using System;

namespace GomokuReversiApp
{
    // Common interface for all game commands.
    public interface ICommand
    {
        void Execute(Game game);
    }

    // Executes a move using the game's move processor.
    public class MoveCommand : ICommand
    {
        private readonly Move move;

        public MoveCommand(Move move)
        {
            this.move = move;
        }

        public void Execute(Game game)
        {
            if (!game.PlayMove(move))
            {
                Console.WriteLine("Invalid move. Please try again.");
            }
        }
    }

    // no pass command, passMove : Move


    // Requests undo of the previous turn.
    public class UndoCommand : ICommand
    {
        public void Execute(Game game)
        {
            game.UndoTurn();
        }
    }

    // Requests redo of the previously undone turn.
    public class RedoCommand : ICommand
    {
        public void Execute(Game game)
        {
            game.RedoTurn();
        }
    }

    // Saves the current game to a specified file.
    public class SaveCommand : ICommand
    {
        private readonly string filePath;

        public SaveCommand(string filePath)
        {
            this.filePath = filePath;
        }

        public void Execute(Game game)
        {
            game.SaveGame(filePath);
        }
    }

    // Requests loading a saved game.
    public class LoadCommand : ICommand
    {
        public void Execute(Game game)
        {
            game.RequestLoad();
        }
    }

    // Displays help for the current game.
    public class HelpCommand : ICommand
    {
        public void Execute(Game game)
        {
            game.DisplayHelp();
        }
    }

    // Quits the current game session.
    public class QuitCommand : ICommand
    {
        public void Execute(Game game)
        {
            game.QuitGame();
        }
    }
}
