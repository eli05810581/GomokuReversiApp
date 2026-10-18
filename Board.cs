
namespace GomokuReversiApp
{
    public class Board
    {
        private Piece?[,] cells;

        public int Rows { get; }
        public int Cols { get; }

        public Board(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            cells = new Piece[rows + 1, cols + 1]; // 1-based indexing
        }

        public bool IsWithinBounds(int row, int col)
        {
            return row >= 1 && row <= Rows &&
                   col >= 1 && col <= Cols;
        }

        public bool IsCorner(int row, int col)
        {
            return (row == 1 || row == Rows) &&
                   (col == 1 || col == Cols);
        }

        public int GetOwner(int row, int col)
        {
            Piece? piece = cells[row, col];
            return piece == null ? 0 : piece.Owner;
        }

        public Piece? GetCell(int row, int col)
        {
            return cells[row, col];
        }

        public void SetCell(int row, int col, Piece piece)
        {
            cells[row, col] = piece;
        }

        public void ClearCell(int row, int col)
        {
            cells[row, col] = null;
        }

        public int CountPieces(int owner)
        {
            int countPieces = 0;

            for (int row = 1; row <= Rows; row++)
            {
                for (int col = 1; col <= Cols; col++)
                {
                    if (GetOwner(row, col) == owner)
                    {
                        countPieces++;
                    }
                }
            }

            return countPieces;
        }

        private void ValidatePosition(int row, int col)
        {
            if (!IsWithinBounds(row, col))
                throw new ArgumentOutOfRangeException(
                    nameof(row), "Position is outside the board.");
        }

        public void RestoreRows(IReadOnlyList<string> rows)
        {
            // Check board dimensions
            if (rows.Count != Rows)
                throw new InvalidDataException("Invalid number of board rows.");

            for (int row = 1; row <= Rows; row++)
            {
                string rowData = rows[row - 1];

                if (rowData == null || rowData.Length != Cols)
                    throw new InvalidDataException("Invalid board row length.");

                for (int col = 1; col <= Cols; col++)
                {
                    char symbol = rowData[col - 1];

                    Piece? piece = symbol switch
                    {
                        '.' => null,
                        'X' => new OrdinaryStone(1),
                        'O' => new OrdinaryStone(2),
                        '#' => new HeavyStone(1),
                        '@' => new HeavyStone(2),
                        'B' => new Disc(1),
                        'W' => new Disc(2),

                        _ => throw new InvalidDataException(
                            $"Unknown piece symbol: {symbol}")
                    };

                    if (piece == null)
                    {
                        ClearCell(row, col);
                    }
                    else
                    {
                        SetCell(row, col, piece);
                    }
                }
            }
        }

        public string[] CaptureRows()
        {
            string[] rows = new string[Rows];

            for (int row = 1; row <= Rows; row++)
            {
                char[] rowData = new char[Cols];

                for (int col = 1; col <= Cols; col++)
                {
                    Piece? piece = GetCell(row, col);

                    rowData[col - 1] = piece switch
                    {
                        null => '.',
                        OrdinaryStone { Owner: 1 } => 'X',
                        OrdinaryStone { Owner: 2 } => 'O',
                        HeavyStone { Owner: 1 } => '#',
                        HeavyStone { Owner: 2 } => '@',
                        Disc { Owner: 1 } => 'B',
                        Disc { Owner: 2 } => 'W',

                        _ => throw new InvalidDataException(
                            "Unknown piece type or owner.")
                    };
                }

                rows[row - 1] = new string(rowData);
            }

            return rows;
        }
    }
}
