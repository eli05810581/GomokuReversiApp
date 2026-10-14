namespace GomokuReversiApp
{
    // Determines whether the game has ended and who won.
    public interface IGameOverChecker
    {
        GameResult Check(Game game);
    }

    public class AlignmentChecker : IGameOverChecker // For Gomoku Family
    {
        private readonly int winLength;

        public AlignmentChecker(int winLength)
        {
            this.winLength = winLength;
        }

        public GameResult Check(Game game)
        {
            Board board = game.Board;

            // Check for a winning alignment
            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    int owner = board.GetOwner(row, col);

                    if (owner == 0)
                        continue;

                    if (CountLine(board, row, col, owner) >= winLength)
                    {
                        return owner == 1
                            ? GameResult.Player1Wins
                            : GameResult.Player2Wins;
                    }
                }
            }

            // Check if either player can still move
            bool player1CanMove =
                game.MoveProcessor.HasLegalMoves(game, game.Players[0]);

            bool player2CanMove =
                game.MoveProcessor.HasLegalMoves(game, game.Players[1]);

            // Neither player can move, so the game is a draw
            if (!player1CanMove && !player2CanMove)
                return GameResult.Draw;

            return GameResult.OnGoing;
        }

        private int CountLine(
            Board board, int row, int col, int owner)
        {
            // Horizontal, vertical and two diagonals
            (int dr, int dc)[] directions =
            {
                (0, 1),
                (1, 0),
                (1, 1),
                (1, -1)
            };

            int longest = 1;

            foreach (var (dr, dc) in directions)
            {
                int total = 1;

                // Count consecutive stones in both directions
                total += CountDirection(
                    board, row, col, dr, dc, owner);

                total += CountDirection(
                    board, row, col, -dr, -dc, owner);

                if (total > longest)
                    longest = total;
            }

            return longest;
        }

        private int CountDirection(
            Board board, int row, int col,
            int dr, int dc, int owner)
        {
            int count = 0;

            row += dr;
            col += dc;

            while (board.IsWithinBounds(row, col) &&
                   board.GetOwner(row, col) == owner)
            {
                count++;

                row += dr;
                col += dc;
            }

            return count;
        }
    }

    public abstract class CountBasedChecker : IGameOverChecker // For Reversi family. Template method (Decide winner method is varying)
    {
        public virtual GameResult Check(Game game)
        {
            // Check whether either player can make a move
            bool player1CanMove =
                game.MoveProcessor.HasLegalMoves(game, game.Players[0]);

            bool player2CanMove =
                game.MoveProcessor.HasLegalMoves(game, game.Players[1]);

            if (player1CanMove || player2CanMove)
                return GameResult.OnGoing;

            // Neither player can move: determine the winner
            return DecideWinner(
                game.Board.CountPieces(1),
                game.Board.CountPieces(2)
            );
        }

        protected abstract GameResult DecideWinner(
            int p1Count, int p2Count);
    }

    public class MajorityChecker : CountBasedChecker
    {
        protected override GameResult DecideWinner(
            int p1Count, int p2Count)
        {
            if (p1Count > p2Count)
                return GameResult.Player1Wins;

            if (p1Count < p2Count)
                return GameResult.Player2Wins;

            return GameResult.Draw;
        }
    }

    public class MisereChecker : CountBasedChecker
    {
        protected override GameResult DecideWinner(
            int p1Count, int p2Count)
        {
            if (p1Count < p2Count)
                return GameResult.Player1Wins;

            if (p1Count > p2Count)
                return GameResult.Player2Wins;

            return GameResult.Draw;
        }
    }

    public class CornerChecker : CountBasedChecker
    {
        public override GameResult Check(Game game)
        {
            Board board = game.Board;

            // Get the owners of the four corners
            int[] corners =
            {
                board.GetOwner(1, 1),
                board.GetOwner(1, board.Cols),
                board.GetOwner(board.Rows, 1),
                board.GetOwner(board.Rows, board.Cols)
            };

            int p1Corner = 0;
            int p2Corner = 0;

            foreach (int owner in corners)
            {
                if (owner == 1)
                    p1Corner++;

                else if (owner == 2)
                    p2Corner++;
            }

            // Three corners produces an immediate victory
            if (p1Corner >= 3)
                return GameResult.Player1Wins;

            if (p2Corner >= 3)
                return GameResult.Player2Wins;

            // Otherwise use the standard disk-count rules
            return base.Check(game);
        }

        protected override GameResult DecideWinner(
            int p1Count, int p2Count)
        {
            if (p1Count > p2Count)
                return GameResult.Player1Wins;

            if (p1Count < p2Count)
                return GameResult.Player2Wins;

            return GameResult.Draw;
        }
    }
}