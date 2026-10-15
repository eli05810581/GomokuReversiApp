
using System;
using System.Collections.Generic;

namespace GomokuReversiApp
{
    // Move processors check whether a move is legal and apply it.
    public interface IMoveProcessor
    {
        List<Move> GetLegalMoves(Game game, Player player);
        bool IsMoveLegal(Game game, Move move);
        void ApplyMove(Game game, Move move);
        bool HasLegalMoves(Game game, Player player);
        bool AllowPass { get; }
    }

    public abstract class MoveProcessorBase : IMoveProcessor
    {
        public virtual bool AllowPass => false;

        public abstract List<Move> GetLegalMoves(Game game, Player player);

        public abstract void ApplyMove(Game game, Move move);

        // FIX: Compare move values instead of object references.
        public bool IsMoveLegal(Game game, Move move)
        {
            List<Move> legalMoves =
                GetLegalMoves(game, game.CurrentPlayer);

            foreach (Move legalMove in legalMoves)
            {
                if (move is ReversiMove requested &&
                    legalMove is ReversiMove legal &&
                    requested.Row == legal.Row &&
                    requested.Col == legal.Col)
                {
                    return true;
                }

                if (move is GomokuMove requestedGomoku &&
                    legalMove is GomokuMove legalGomoku &&
                    requestedGomoku.Row == legalGomoku.Row &&
                    requestedGomoku.Col == legalGomoku.Col &&
                    requestedGomoku.StoneType == legalGomoku.StoneType)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasLegalMoves(Game game, Player player)
        {
            return GetLegalMoves(game, player).Count > 0;
        }
    }

    // **** GOMOKU STANDARD ****
    public class GomokuMoveProcessor : MoveProcessorBase
    {
        public override List<Move> GetLegalMoves(Game game, Player player)
        {
            List<Move> moves = new List<Move>();
            Board board = game.Board;

            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    if (board.GetCell(row, col) == null)
                    {
                        moves.Add(new GomokuMove(row, col, 'O'));
                    }
                }
            }

            return moves;
        }

        public override void ApplyMove(Game game, Move move)
        {
            GomokuMove gomokuMove = (GomokuMove)move;

            game.Board.SetCell(
                gomokuMove.Row,
                gomokuMove.Col,
                new OrdinaryStone(game.CurrentPlayer.Number)
            );
        }
    }

    // **** GOMOKU PLUS ****
    public class GomokuPlusMoveProcessor : MoveProcessorBase
    {
        public override List<Move> GetLegalMoves(Game game, Player player)
        {
            List<Move> moves = new List<Move>();
            Board board = game.Board;

            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    Piece? piece = board.GetCell(row, col);

                    if (piece == null)
                    {
                        moves.Add(new GomokuMove(row, col, 'O'));

                        if (player.HasStoneAvailable('H'))
                        {
                            moves.Add(new GomokuMove(row, col, 'H'));
                        }
                    }
                    else if (player.HasStoneAvailable('E') &&
                             piece is OrdinaryStone &&
                             piece.Owner != player.Number)
                    {
                        moves.Add(new GomokuMove(row, col, 'E'));
                    }
                }
            }

            return moves;
        }

        public override void ApplyMove(Game game, Move move)
        {
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

                    player.UseStone('H');
                    break;

                case 'E':
                    game.Board.ClearCell(
                        gomokuMove.Row,
                        gomokuMove.Col
                    );

                    player.UseStone('E');
                    break;

                default:
                    throw new ArgumentException("Unknown stone type.");
            }
        }
    }

    // **** REVERSI ****
    public class ReversiMoveProcessor : MoveProcessorBase
    {
        public override bool AllowPass => true;

        private static readonly (int dr, int dc)[] Directions =
        {
            (-1, -1), (-1, 0), (-1, 1),
            ( 0, -1),          ( 0, 1),
            ( 1, -1), ( 1, 0), ( 1, 1)
        };

        public override List<Move> GetLegalMoves(Game game, Player player)
        {
            List<Move> moves = new List<Move>();
            Board board = game.Board;

            for (int row = 1; row <= board.Rows; row++)
            {
                for (int col = 1; col <= board.Cols; col++)
                {
                    if (board.GetCell(row, col) != null)
                        continue;

                    ReversiMove move = new ReversiMove(row, col);

                    if (GetCapturedPositions(game, move, player).Count > 0)
                    {
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        public override void ApplyMove(Game game, Move move)
        {
            ReversiMove reversiMove = (ReversiMove)move;
            Player player = game.CurrentPlayer;

            List<(int Row, int Col)> captured =
                GetCapturedPositions(game, reversiMove, player);

            game.Board.SetCell(
                reversiMove.Row,
                reversiMove.Col,
                new Disc(player.Number)
            );

            foreach (var (row, col) in captured)
            {
                game.Board.SetCell(
                    row,
                    col,
                    new Disc(player.Number)
                );
            }
        }

        private List<(int Row, int Col)> GetCapturedPositions(
            Game game,
            ReversiMove move,
            Player player)
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

                    if (stone == null)
                    {
                        break;
                    }

                    if (stone.Owner == player.Number)
                    {
                        if (line.Count > 0)
                        {
                            captured.AddRange(line);
                        }

                        break;
                    }

                    line.Add((row, col));

                    row += dr;
                    col += dc;
                }
            }

            return captured;
        }

        public int CountFlips(Game game, ReversiMove move)
        {
            if (!game.Board.IsWithinBounds(move.Row, move.Col))
            {
                return 0;
            }

            if (game.Board.GetCell(move.Row, move.Col) != null)
            {
                return 0;
            }

            return GetCapturedPositions(
                game,
                move,
                game.CurrentPlayer
            ).Count;
        }
    }
}
