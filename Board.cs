namespace GomokuReversiApp
{
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

}