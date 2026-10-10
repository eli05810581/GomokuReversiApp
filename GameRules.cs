namespace GomokuReversiApp
{
    // I don't know if we need these classes - chris
public interface IGameRules
{
    List<Move> GetLegalMoves(Board board, Player player);
}

public class GomokuRules: IGameRules
{
    public List<Move> GetLegalMoves(Board board, Player player)
    {
        List<Move> moves = new List<Move>();

        for (int row = 1; row <= board.Rows; row++)
        {
            for (int col = 1; col <= board.Cols; col++)
            {
                if (board.GetOwner(row, col) == 0) // Empty cell
                {
                    moves.Add(new GomukuMove(row, col, 'O'));
                }
            }
        }

        return moves;
    }
}

public class ReversiRules: IGameRules
{
    private static int[] dRow = { -1, -1, -1, 0, 0, 1, 1, 1 };
    private static int[] dCol = { -1, 0, 1, -1, 1, -1, 0, 1 };
    public List<Move> GetLegalMoves(Board board, Player player)
    {
        List<Move> moves = new List<Move>();
        for (int row = 1; row <= board.Rows; row++)
        {
            for (int col = 1; col <= board.Cols; col++)
            {
                if (board.GetOwner(row, col) != 0) continue;
                int flips = CountFlips(board, row, col, player.Number);
                if (flips > 0)
                {
                    moves.Add(new ReversiMove(row, col, flips));
                }
            }
        }
        return moves;
    }

    private int CountFlips(Board board, int row, int col, int currentPlayer)
    {
        int total = 0;
        for (int d = 0; d < 8; d++)
        {
            total = CountFlipsInDirection(board, row, col, dRow[d], dCol[d], currentPlayer);
        }
        return total;
    }

    private int CountFlipsInDirection(Board board, int row, int col, int drow, int dcol, int currentPlayer)
    {
        int opponent = 3 - currentPlayer;
        int count = 0;

        row += drow;
        col += dcol;
        while (board.IsBounds(row, col) && board.GetOwner(row, col) == opponent)
        {
            count++;
            row += drow;
            col += dcol;
        }
        if (board.IsBounds(row, col) && board.GetOwner(row, col) == currentPlayer)
        {
            return count;
        }

        return 0;
    }
    

}

}