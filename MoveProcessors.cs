namespace GomokuReversiApp
{
    using System;
    using System.Collections.Generic;
    // move processors receive a syntactly correct move object, they check if the move is legal then apply the move.
    public interface IMoveProcessor
    {
        bool IsMoveLegal(Game game, Move move);
        void ApplyMove(Game game, Move move);
        bool HasLegalMoves(Game game, Player player);
        
    }

    // **** GOMOKU STANDARD ****
    public class GomokuMoveProcessor : IMoveProcessor
    {
        public bool IsMoveLegal(Game game, Move move)
        {
            if (move is not GomokuMove gomokuMove)
            return false;

            // Only ordinary stones are permitted
            if (gomokuMove.StoneType != 'O')
            return false;

            // Check board boundaries
            if (!game.Board.IsWithinBounds(gomokuMove.Row, gomokuMove.Col))
                return false;

            // Cell must be empty
            return game.Board.GetCell(gomokuMove.Row, gomokuMove.Col) == null;
        }

        public void ApplyMove(Game game, Move move)
        {
            GomokuMove gomokuMove = (GomokuMove)move; // we are assuming Move supplied is correct

            // Place the current player's ordinary stone
            game.Board.SetCell(
                gomokuMove.Row,
                gomokuMove.Col,
                new OrdinaryStone(game.CurrentPlayer.Number)
            );
        }
    
        public bool HasLegalMoves(Game game, Player player)
        {
            Board board = game.Board;

            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    if (board.GetCell(row, col) == null)
                        return true;
                }
            }

            return false;
        }
    }
    // **** GOMOKU PLUS ****
    public class GomokuPlusMoveProcessor : IMoveProcessor
    {
        public bool IsMoveLegal(Game game, Move move)
        {
            if (move is not GomokuMove gomokuMove)
                return false;

            if (!game.Board.IsWithinBounds(gomokuMove.Row, gomokuMove.Col))
                return false;

            Piece? target = game.Board.GetCell(gomokuMove.Row, gomokuMove.Col);

            Player player = game.CurrentPlayer;

            switch (gomokuMove.StoneType)
            {
                case 'O': // Ordinary stone
                    return target == null;

                case 'H': // Heavy stone
                    return target == null &&
                        player.Inventory.TryGetValue('H', out int? heavyCount) && heavyCount.GetValueOrDefault() > 0;

                case 'E': // Eraser
                    return target is OrdinaryStone &&
                        target.Owner != player.Number &&
                        player.Inventory.TryGetValue('E', out int? eraserCount) &&
                        eraserCount.GetValueOrDefault() > 0;

                default:
                    return false;
            }
        }
        public void ApplyMove(Game game, Move move)
        {
            // Game.ProcessMove() should already have validated this move
            GomokuMove gomokuMove = (GomokuMove)move;
            Player player = game.CurrentPlayer;

            switch (gomokuMove.StoneType)
            {
                case 'O':
                    game.Board.SetCell(
                        gomokuMove.Row,
                        gomokuMove.Col,
                        new OrdinaryStone(player.Number)
                    );
                    break;

                case 'H':
                    game.Board.SetCell(
                        gomokuMove.Row,
                        gomokuMove.Col,
                        new HeavyStone(player.Number)
                    );

                    player.Inventory['H'] =
                        player.Inventory['H']!.Value - 1;
                    break;

                case 'E':
                    // Remove the opponent's ordinary stone
                    game.Board.SetCell(
                        gomokuMove.Row,
                        gomokuMove.Col,
                        null
                    );

                    player.Inventory['E'] =
                        player.Inventory['E']!.Value - 1;
                    break;

                default:
                    throw new ArgumentException("Unknown stone type.");
            }
        }

        public bool HasLegalMoves(Game game, Player player)
        {
            Board board = game.Board;

            bool hasEraser =
                player.Inventory.TryGetValue('E', out int? eraserCount) &&
                (eraserCount == null || eraserCount > 0);

            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    Piece? piece = board.GetCell(row, col);

                    // Ordinary stones can be placed on empty cells
                    if (piece == null)
                        return true;

                    // Erasers can remove opponent's ordinary stones
                    if (hasEraser &&
                        piece is OrdinaryStone &&
                        piece.Owner != player.Number)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
    // **** REVERSI ****
    public class ReversiMoveProcessor : IMoveProcessor
    {
        // dr = delta row, dc = delta column
        private static readonly (int dr, int dc)[] Directions =
        {
            (-1, -1), (-1, 0), (-1, 1),
            ( 0, -1),          ( 0, 1),
            ( 1, -1), ( 1, 0), ( 1, 1)
        };

        public bool IsMoveLegal(Game game, Move move)
        {
            if (move is not ReversiMove reversiMove)
                return false;

            if (!game.Board.IsWithinBounds(reversiMove.Row, reversiMove.Col))
                return false;

            if (game.Board.GetCell(reversiMove.Row, reversiMove.Col) != null)
                return false;

            // Must capture at least one opponent disc
            return GetCapturedPositions(
                game, reversiMove, game.CurrentPlayer
            ).Count > 0;
        }

        public void ApplyMove(Game game, Move move)
        {
            // Assume Game.PlayMove() has already validated the move
            ReversiMove reversiMove = (ReversiMove)move;

            Player player = game.CurrentPlayer;

            // Find all opponent discs that will be flipped
            List<(int Row, int Col)> captured =
                GetCapturedPositions(game, reversiMove, player);

            // Place the current player's disc
            game.Board.SetCell(
                reversiMove.Row,
                reversiMove.Col,
                new Disc(player.Number)
            );

            // Flip all captured opponent discs
            foreach (var (row, col) in captured)
            {
                game.Board.SetCell(
                    row,
                    col,
                    new Disc(player.Number)
                );
            }
        }

        // Check whether a specified player has any legal moves
        public bool HasLegalMoves(Game game, Player player)
        {
            Board board = game.Board;

            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    // A disc can only be placed on an empty cell
                    if (board.GetCell(row, col) != null)
                        continue;

                    ReversiMove move = new ReversiMove(row, col);

                    // A legal move must capture at least one opponent disc
                    if (GetCapturedPositions(game, move, player).Count > 0)
                        return true;
                }
            }

            return false;
        }

        // Find all opponent discs that would be captured
        private List<(int Row, int Col)> GetCapturedPositions(
            Game game, ReversiMove move, Player player)
        {
            var captured = new List<(int Row, int Col)>();

            foreach (var (dr, dc) in Directions)
            {
                var line = new List<(int Row, int Col)>();

                int row = move.Row + dr;
                int col = move.Col + dc;

                while (game.Board.IsWithinBounds(row, col))
                {
                    Piece? stone = game.Board.GetCell(row, col);

                    // Empty square breaks the capture line
                    if (stone == null)
                        break;

                    // Friendly disc closes the capture line
                    if (stone.Owner == player.Number)
                    {
                        if (line.Count > 0)
                            captured.AddRange(line);

                        break;
                    }

                    // Opponent disc: potential capture
                    line.Add((row, col));

                    row += dr;
                    col += dc;
                }
            }

            return captured;
        }

        // Count the discs flipped by the current player's proposed move
        public int CountFlips(Game game, ReversiMove move)
        {
            if (!game.Board.IsWithinBounds(move.Row, move.Col))
                return 0;

            if (game.Board.GetCell(move.Row, move.Col) != null)
                return 0;

            return GetCapturedPositions(
                game, move, game.CurrentPlayer
            ).Count;
        }
    }

}





