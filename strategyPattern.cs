using System.Data;
using System.Security;
public enum GameResult { OnGoing, Player1Wins, Player2Wins, Draw}
public abstract class Move {}
public class PassMove: Move {}
public class GomukuMove: Move
{
    public int Row {get;}
    public int Col {get;}
    public char StoneType {get;} // O, H, E
    public GomukuMove(int row, int col, char stoneType)
    {
        Row = row;
        Col = col;
        StoneType = stoneType;
    }
}
public class ReversiMove: Move
{
    public int Row {get;}
    public int Col {get;}
    public int FlipCount {get;} // the number of opponent's pieces to be flipped, calculated by ReversiRules
    public ReversiMove(int row, int col, int flipCount)
    {
        Row = row;
        Col = col;
        FlipCount = flipCount;
    }
}
public abstract class Player
{
    public int Number {get;} // 1 or 2
    protected Player(int number)
    {
        Number = number;
    }
    public abstract Move GetMove(Board board, IGameRules rules);
}

public class Board
{
    private int[,] cells;
    public int Rows {get;}
    public int Cols {get;}

    public Board(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        cells = new int[rows + 1, cols + 1]; // Using based 1 index
    }

    public bool IsBounds(int row, int col)
    {
        return row >= 1 && row <= Rows && col >= 1 && col <= Cols;
    }

    public int GetOwner(int row, int col)
    {
        return cells[row, col];
    }

    public int LongestLine(int row, int col, int owner)
    {
        int[] dRow = { 0, 1, 1, 1};
        int[] dCol = { 1, 0, 1, -1};

        int best = 1;
        for (int d = 0; d < 4; d++)
        {
            int total = 1 + CountDirection(row, col, dRow[d], dCol[d], owner) + CountDirection(row, col, -dRow[d], -dCol[d], owner);

            if (total > best) best = total;
        }

        return best;
    }

    private int CountDirection(int row, int col, int dRow, int dCol, int owner)
    {
        int count = 0;
        row += dRow;
        col += dCol;
        while (IsBounds(row, col) && cells[row, col] == owner)
        {
            count++;
            row += dRow;
            col += dCol;
        }

        return count;
    }
}

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
public interface IMoveStrategy
{
    Move ChooseMove(Board board, IGameRules rules, Player AIPlayer);
}
public class RandomMoveStrategy: IMoveStrategy
{
    private Random random = new Random();

    public Move ChooseMove(Board board, IGameRules rules, Player AIPlayer)
    {
        List<Move> moves = rules.GetLegalMoves(board, AIPlayer);
        if (moves.Count == 0) return new PassMove(); // only occurs in Reversi games
        return moves[random.Next(moves.Count)];
    }
}

public class GomokuSmartStrategy: IMoveStrategy
{
    private int winLength;
    private Random random = new Random();
    public Move ChooseMove(Board board, IGameRules rules, Player AIPlayer)
    {
        List<Move> moves = rules.GetLegalMoves(board, AIPlayer);
        int opponent = 3 - AIPlayer.Number;

        // 1. Immediate win move
        foreach (Move move in moves)
        {
            GomukuMove gomukuMove = (GomukuMove)move;
            if (gomukuMove.StoneType == 'O' && board.LongestLine(gomukuMove.Row, gomukuMove.Col, AIPlayer.Number) >= winLength)
            {
                return gomukuMove;
            }
        }

        // 2. Block win move of opponent
        foreach (Move move in moves)
        {
            GomukuMove gomokuMove = (GomukuMove)move;
            if (gomokuMove.StoneType == 'O' && board.LongestLine(gomokuMove.Row, gomokuMove.Col, opponent) >= winLength)
            {
                return gomokuMove;
            }
        }

        // 3. Random move
        return moves[random.Next(moves.Count)];
    }
}

public abstract class ReversiScoringStrategy: IMoveStrategy
{
    public Move ChooseMove(Board board, IGameRules rules, Player AIPlayer)
    {
        List<Move> moves = rules.GetLegalMoves(board, AIPlayer);

        if (moves.Count == 0) return new PassMove();

        ReversiMove best = null;
        int bestScore = int.MinValue;
        foreach(Move move in moves)
        {
            ReversiMove reversiMove = (ReversiMove)move;
            int score = Score(board, reversiMove);
            if (score > bestScore)
            {
                bestScore = score;
                best = reversiMove;
            }
        }

        return best;
    }

    protected abstract int Score(Board board, ReversiMove move);
}

