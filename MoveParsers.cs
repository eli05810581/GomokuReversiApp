namespace GomokuReversiApp
{
    // Parses move strings into game-specific Move objects
    public interface IMoveParser
    {
        Move ParseMove(string input);
    }

    public class GomokuMoveParser : IMoveParser
    {
        public Move ParseMove(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new FormatException("Move cannot be empty.");

            input = input.Trim().ToUpper();

            // First character identifies the stone type
            char stoneType = input[0];

            if (!IsAllowedStone(stoneType))
            {
                throw new FormatException(
                    $"Invalid stone type: {stoneType}"
                );
            }

            // Remaining characters contain row and column
            string position = input.Substring(1);
            string[] parts = position.Split(':');

            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int row) ||
                !int.TryParse(parts[1], out int col))
            {
                throw new FormatException(
                    "Invalid move format. Use O5:3."
                );
            }

            if (row < 1 || col < 1)
            {
                throw new FormatException(
                    "Row and column must be positive."
                );
            }

            return new GomokuMove(row, col, stoneType);
        }

        // Standard Gomoku and Fog only allow Ordinary stones
        protected virtual bool IsAllowedStone(char stoneType)
        {
            return stoneType == 'O';
        }
    }

    public class GomokuPlusMoveParser : GomokuMoveParser
    {
        // Plus allows Ordinary, Heavy and Eraser stones
        protected override bool IsAllowedStone(char stoneType)
        {
            return stoneType == 'O' ||
                   stoneType == 'H' ||
                   stoneType == 'E';
        }
    }

    public class ReversiMoveParser : IMoveParser
    {
        public Move ParseMove(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new FormatException("Move cannot be empty.");

            input = input.Trim().ToUpper();

            // Reversi moves must start with P
            if (input[0] != 'P')
            {
                throw new FormatException(
                    "Invalid Reversi move. Use P3:4."
                );
            }

            // Remove the P prefix
            string position = input.Substring(1);

            string[] parts = position.Split(':');

            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int row) ||
                !int.TryParse(parts[1], out int col))
            {
                throw new FormatException(
                    "Invalid move format. Use P3:4."
                );
            }

            if (row < 1 || col < 1)
            {
                throw new FormatException(
                    "Row and column must be positive."
                );
            }

            return new ReversiMove(row, col);
        }
    }
}