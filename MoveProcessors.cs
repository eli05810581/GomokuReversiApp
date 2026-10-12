namespace GomokuReversiApp
{
    using System;
    using System.Collections.Generic;
    // move processors receive a syntactly correct move object, they check if the move is legal then apply the move.
    public interface IMoveProcessor
    {
        bool IsMoveLegal(Game game, Move move);
        void ApplyMove(Game game, Move move);
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

    // **** REVERSI ****
    public class ReversiMoveProcessor : IMoveProcessor
    {   
        // dr = delta row, dc = delta column
        private static readonly (int dr, int dc)[] Directions = {(-1, -1), (-1, 0), (-1, 1),( 0, -1),( 0, 1),( 1, -1), ( 1, 0), ( 1, 1)}; // 8 directions

        public bool IsMoveLegal(Game game, Move move)
        {
            if (move is not ReversiMove reversiMove)
                return false;

            if (!game.Board.IsWithinBounds(reversiMove.Row, reversiMove.Col))
                return false;

            if (game.Board.GetCell(reversiMove.Row, reversiMove.Col) != null)
                return false;

            // Must capture at least one opponent disc
            return GetCapturedPositions(game, reversiMove).Count > 0;
        }

        public void ApplyMove(Game game, Move move)
        {
            // Assume Game.ProcessMove() has already validated the move
            ReversiMove reversiMove = (ReversiMove)move;

            Player player = game.CurrentPlayer;

            // Find all opponent discs that will be flipped
            List<(int Row, int Col)> captured =
                GetCapturedPositions(game, reversiMove);

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

        // Helper methods
        private List<(int Row, int Col)> GetCapturedPositions(Game game,  ReversiMove move) // finds a list of cells that would have their disks captured
        {
            var captured = new List<(int Row, int Col)>();
            Player player = game.CurrentPlayer;

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
    
        public int CountFlips(Game game, ReversiMove move)
        {
            return GetCapturedPositions(game, move).Count;
        }

    }
    }

    // **** Reversi ****
    public class ReversiMoveProcessor : IMoveProcessor
    {
        // Eight possible directions for capturing discs
        private static readonly (int dr, int dc)[] Directions =
        {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),           (0, 1),
            (1, -1),  (1, 0),  (1, 1)
        };

        // Check whether a move is legal
        public bool IsMoveLegal(Game game, Move move)
        {
            // A player may pass only when no legal moves exist
            if (move is PassMove)
            {
                return !HasLegalMoves(game);
            }

            // Only ReversiMove is accepted for disc placement
            if (move is not ReversiMove reversiMove)
                return false;

            Board board = game.Board;

            // Check board boundaries
            if (!board.IsWithinBounds(reversiMove.Row, reversiMove.Col))
                return false;

            // Target cell must be empty
            if (board.GetCell(reversiMove.Row, reversiMove.Col) != null)
                return false;

            // Move must capture at least one opponent disc
            return GetCapturedPositions(game, reversiMove).Count > 0;
        }

        // Apply a validated move
        public void ApplyMove(Game game, Move move)
        {
            // Passing does not change the board
            if (move is PassMove)
                return;

            ReversiMove reversiMove = (ReversiMove)move;

            Player player = game.CurrentPlayer;

            // Find captured discs before modifying the board
            List<(int Row, int Col)> captured =
                GetCapturedPositions(game, reversiMove);

            // Place the current player's disc
            game.Board.SetCell(
                reversiMove.Row,
                reversiMove.Col,
                new Disc(player.Number)
            );

            // Flip captured opponent discs
            foreach (var (row, col) in captured)
            {
                game.Board.SetCell(
                    row,
                    col,
                    new Disc(player.Number)
                );
            }
        }

        // Return the number of discs captured by a proposed move
        public int CountFlips(Game game, ReversiMove move)
        {
            if (!game.Board.IsWithinBounds(move.Row, move.Col))
                return 0;

            if (game.Board.GetCell(move.Row, move.Col) != null)
                return 0;

            return GetCapturedPositions(game, move).Count;
        }

        // Find all opponent discs captured by a proposed move
        private List<(int Row, int Col)> GetCapturedPositions(
            Game game, ReversiMove move)
        {
            List<(int Row, int Col)> captured =
                new List<(int Row, int Col)>();

            Board board = game.Board;

            int player = game.CurrentPlayer.Number;
            int opponent = 3 - player;

            foreach (var (dr, dc) in Directions)
            {
                List<(int Row, int Col)> directionCaptured =
                    new List<(int Row, int Col)>();

                int row = move.Row + dr;
                int col = move.Col + dc;

                // Collect consecutive opponent discs
                while (board.IsWithinBounds(row, col) &&
                       board.GetOwner(row, col) == opponent)
                {
                    directionCaptured.Add((row, col));

                    row += dr;
                    col += dc;
                }

                // Captures require at least one opponent disc,
                // followed by a disc owned by the current player
                if (directionCaptured.Count > 0 &&
                    board.IsWithinBounds(row, col) &&
                    board.GetOwner(row, col) == player)
                {
                    captured.AddRange(directionCaptured);
                }
            }

            return captured;
        }

        // Determine whether the current player has any legal placements
        private bool HasLegalMoves(Game game)
        {
            Board board = game.Board;

            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    if (board.GetCell(row, col) != null)
                        continue;

                    ReversiMove move = new ReversiMove(row, col);

                    if (GetCapturedPositions(game, move).Count > 0)
                        return true;
                }
            }

            return false;
        }
    }
}



