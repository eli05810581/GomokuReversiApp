
using System;

namespace GomokuReversiApp
{
    // Converts user input into command objects.
    public class InputParser
    {
        private readonly IMoveParser moveParser;

        public InputParser(IMoveParser moveParser)
        {
            this.moveParser = moveParser;
        }

        public ICommand Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new InvalidCommand();
            }

            string trimmed = input.Trim();
            string[] parts = trimmed.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            string command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "undo":
                    return new UndoCommand();

                case "redo":
                    return new RedoCommand();

                // no pass command, only pass move

                case "help":
                    return new HelpCommand();

                case "quit":
                case "exit":
                    return new QuitCommand();

                case "load":
                    return new LoadCommand();

                case "save":
                    if (parts.Length < 2)
                    {
                        return new InvalidCommand();
                    }

                    return new SaveCommand(
                        string.Join(" ", parts, 1, parts.Length - 1));

                default: // if it's not an application command it must be a move, or an invalid command
                    try
                    {
                        Move move = moveParser.ParseMove(input);

                        if (move == null)
                        {
                            return new InvalidCommand(
                                new FormatException("Move parser returned null.")
                            );
                        }

                        return new MoveCommand(move);
                    }
                    catch (Exception ex) when (
                        ex is FormatException ||
                        ex is ArgumentException
                    )
                    {
                        return new InvalidCommand(ex);
                    }
            }
        }
    }

    
    // Handles invalid or unsupported input.
    public class InvalidCommand : ICommand
    {
        private readonly Exception? exception;

        public InvalidCommand(Exception? exception = null)
        {
            this.exception = exception;
        }

        public void Execute(Game game)
        {
            Console.WriteLine(
                "Invalid command. Type 'help' for available commands."
            );

            if (exception != null)
            {
                Console.WriteLine(
                    $"Error Type: {exception.GetType().Name}"
                );

                Console.WriteLine(
                    $"Details: {exception.Message}"
                );
            }
        }
    }

}
