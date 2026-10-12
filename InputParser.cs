
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

                default:
                    // Move parsing will be connected when
                    // IMoveParser is implemented by the team.
                    return new InvalidCommand();
            }
        }
    }

    // Handles invalid or unsupported input.
    public class InvalidCommand : ICommand
    {
        public void Execute(Game game)
        {
            Console.WriteLine(
                "Invalid command. Type 'help' for available commands.");
        }
    }
}
