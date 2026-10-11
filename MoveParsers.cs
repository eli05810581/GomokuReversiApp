namespace GomokuReversiApp
{
    // move parsers receive strings that are not commands, and return game appropriate move objects
        public interface IMoveParser
    {
       Move ParseMove(string input);
    }

    public class GomokuMoveParser : IMoveParser
    {
        public Move ParseMove(string input)
        {
            // TODO: Parse Gomoku move syntax
            throw new NotImplementedException();
        }
    }

    public class ReversiMoveParser : IMoveParser
    {
        public Move ParseMove(string input)
        {
            // TODO: Parse Reversi move syntax
            throw new NotImplementedException();
        }
    }
}