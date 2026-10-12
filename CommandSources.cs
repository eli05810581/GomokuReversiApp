namespace GomokuReversiApp
{
    // this interface defines where the next command comes from; Human(interactive), AI(interactive), or script (automated)
    public interface ICommandSource
    {
        ICommand GetNextCommand(Game game);
    }
    public class InteractiveCommandSource : ICommandSource // for HvH and HvAI games
    {
        private InputParser inputParser;

        public InteractiveCommandSource(InputParser inputParser)
        {
            this.inputParser = inputParser;
        }

        public ICommand GetNextCommand(Game game)
        {
            if (game.CurrentPlayer is AIPlayer aiPlayer)
            {
                if (game.MustPass())
                {
                    return new MoveCommand(new PassMove());
                }

                Move move = aiPlayer.GetMove(game);

                return new MoveCommand(move);
            }
            else // this is for Human player
            {
                Console.Write("Enter command: ");

                string input = Console.ReadLine() ?? "";

                return inputParser.Parse(input);
            }
        }
    }

    public class AutomatedCommandSource : ICommandSource // for automated/scripted games
    {
        private Queue<string> commands;
        private InputParser inputParser;

        public AutomatedCommandSource(
            List<string> moveSequence,
            InputParser inputParser)
        {
            commands = new Queue<string>(moveSequence);
            this.inputParser = inputParser;
        }

        public ICommand GetNextCommand(Game game)
        {
            if (commands.Count == 0)
            {
                return new QuitCommand();
            }

            string input = commands.Dequeue();

            return inputParser.Parse(input);
        }
    }
}