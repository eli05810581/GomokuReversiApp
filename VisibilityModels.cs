namespace GomokuReversiApp
{
    // visibility models determines how a board is displayed to a player an displays it 
    public interface IVisibilityModel
    {
        List<List<string>> GetVisibleBoardForPlayer(
            Board board,
            Player player
        );

        void RenderBoard(Board board, Player player);
    }

    public class FullVisibility : IVisibilityModel
    {
        public List<List<string>> GetVisibleBoardForPlayer(
            Board board,
            Player player
        )
        {
            List<List<string>> visibleBoard = new();

            for (int row = 1; row <= board.Rows; row++)
            {
                List<string> visibleRow = new();

                for (int col = 1; col <= board.Cols; col++)
                {
                    Piece? piece = board.GetCell(row, col);

                    string symbol = piece == null
                        ? "."
                        : piece.Symbol.ToString();

                    visibleRow.Add(symbol);
                }

                visibleBoard.Add(visibleRow);
            }

            return visibleBoard;
        }

        public void RenderBoard(Board board, Player player)
        {
            List<List<string>> visibleBoard =
                GetVisibleBoardForPlayer(board, player);

            BoardRenderer.Render(visibleBoard);
        }
    }


    public class FogVisibility : IVisibilityModel
    {
        public List<List<string>> GetVisibleBoardForPlayer(
            Board board,
            Player player
        )
        {
            List<List<string>> visibleBoard = new();

            for (int row = 1; row <= board.Rows; row++)
            {
                List<string> visibleRow = new();

                for (int col = 1; col <= board.Cols; col++)
                {
                    if (IsVisible(board, player, row, col))
                    {
                        Piece? piece = board.GetCell(row, col);

                        string symbol = piece == null
                            ? "."
                            : piece.Symbol.ToString();

                        visibleRow.Add(symbol);
                    }
                    else
                    {
                        visibleRow.Add("?");
                    }
                }

                visibleBoard.Add(visibleRow);
            }

            return visibleBoard;
        }

        private bool IsVisible(
            Board board,
            Player player,
            int row,
            int col
        )
        {
            int owner = player.Number;

            // The player's own stones are always visible.
            if (board.GetOwner(row, col) == owner)
            {
                return true;
            }

            // Check the four orthogonally adjacent cells.
            int[,] directions =
            {
                { -1,  0 }, // Up
                {  1,  0 }, // Down
                {  0, -1 }, // Left
                {  0,  1 }  // Right
            };

            for (int i = 0; i < 4; i++)
            {
                int adjacentRow = row + directions[i, 0];
                int adjacentCol = col + directions[i, 1];

                if (!board.IsWithinBounds(adjacentRow, adjacentCol))
                {
                    continue;
                }

                if (board.GetOwner(adjacentRow, adjacentCol) == owner)
                {
                    return true;
                }
            }

            return false;
        }

        public void RenderBoard(Board board, Player player)
        {
            List<List<string>> visibleBoard =
                GetVisibleBoardForPlayer(board, player);

            BoardRenderer.Render(visibleBoard);
        }
    }

    public static class BoardRenderer
    {
        public static void Render(List<List<string>> visibleBoard)
        {
            int rows = visibleBoard.Count;

            if (rows == 0)
                return;

            int cols = visibleBoard[0].Count;

            // Display column numbers
            Console.Write("    ");

            for (int col = 1; col <= cols; col++)
            {
                Console.Write($" {col,2} ");
            }

            Console.WriteLine();

            // Display board rows
            for (int row = 1; row <= rows; row++)
            {
                PrintDivider(cols);

                // Display row number
                Console.Write($"{row,3} ");

                for (int col = 1; col <= cols; col++)
                {
                    string symbol = visibleBoard[row - 1][col - 1];

                    Console.Write($"| {symbol} ");
                }

                Console.WriteLine("|");
            }

            // Bottom border
            PrintDivider(cols);
        }

        private static void PrintDivider(int cols)
        {
            Console.Write("    ");

            for (int col = 0; col < cols; col++)
            {
                Console.Write("+---");
            }

            Console.WriteLine("+");
        }
    }
}


