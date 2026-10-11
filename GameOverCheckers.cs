using System.Buffers;

namespace GomokuReversiApp
{
    // game over checkers check if the game is over, if there is a winner, if there is a draw, if the game is on going.
        public interface IGameOverChecker
    {
        GameResult Check(Board board, bool noMovesLeft);
    }


    public class AlignmentChecker : IGameOverChecker
    {
        private readonly int winLength;

        public AlignmentChecker(int winLength)
        {
            this.winLength = winLength;
        }

        public GameResult Check(Board board, bool noMovesLeft)
        {
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

            return noMovesLeft ? GameResult.Draw : GameResult.OnGoing;
        }

        private int CountLine(Board board, int row, int col, int owner)
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
                total += CountDirection(board, row, col, dr, dc, owner);
                total += CountDirection(board, row, col, -dr, -dc, owner);

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


    public abstract class CountBasedChecker: IGameOverChecker
    {
        public virtual GameResult Check(Board board, bool noMovesLeft)
        {
            if(!noMovesLeft) return GameResult.OnGoing;
            return DecideWinner(board.CountPieces(1), board.CountPieces(2));
        }

        protected abstract GameResult DecideWinner(int p1Count, int p2Count);
    }

    public class MajorityChecker: CountBasedChecker
    {
        protected override GameResult DecideWinner(int p1Count, int p2Count)
        {
            if (p1Count > p2Count) return GameResult.Player1Wins;
            if (p1Count < p2Count) return GameResult.Player2Wins;

            return GameResult.Draw;
        }
    }

    public class MisereChecker: CountBasedChecker
    {
        protected override GameResult DecideWinner(int p1Count, int p2Count)
        {
            if (p1Count < p2Count) return GameResult.Player1Wins;
            if (p1Count > p2Count) return GameResult.Player2Wins;

            return GameResult.Draw;
        }
    }

    public class CornerChecker: CountBasedChecker
    {
        public override GameResult Check(Board board, bool noMovesLeft)
        {
            int[] corners = {board.GetOwner(1,1), board.GetOwner(1, board.Cols), board.GetOwner(board.Rows, 1), board.GetOwner(board.Rows, board.Cols)}; // Top-left, top-right, bottom left, bottom right

            int p1Corner = 0;
            int p2Corner = 0;

            foreach (int owner in corners)
            {
                if (owner == 1) p1Corner++;
                else if (owner == 2) p2Corner++;
            }

            if (p1Corner >= 3) return GameResult.Player1Wins;
            if (p2Corner >= 3) return GameResult.Player2Wins;

            return base.Check(board, noMovesLeft); // No one wins, using CountBasedChecker counting base to check (major or misere)
        }

        protected override GameResult DecideWinner(int p1Count, int p2Count)
        {
            if (p1Count > p2Count) return GameResult.Player1Wins;
            if (p1Count < p2Count) return GameResult.Player2Wins;

            return GameResult.Draw;
        }
    }


}