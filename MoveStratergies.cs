namespace GomokuReversiApp
{
    public interface IMoveStrategy
    {
        Move ChooseMove(Game game);
        // List<Move> GetLegalMoves(Game game);
    }

    // public abstract class MoveStrategyBase : IMoveStrategy
    // {
    //     public abstract Move ChooseMove(Game game);

    //     public List<Move> GetLegalMoves(Game game)
    //     {
    //         List<Move> moves = new List<Move>();

    //         IMoveProcessor processor = game.MoveProcessor;

    //         for (int row = 1; row <= game.Board.Rows; row++)
    //         {
    //             for (int col = 1; col <= game.Board.Cols; col++)
    //             {
    //                 if (processor is GomokuPlusMoveProcessor)
    //                 {
    //                     char[] stoneTypes = { 'O', 'H', 'E' };

    //                     foreach (char stoneType in stoneTypes)
    //                     {
    //                         GomokuMove move =
    //                             new GomokuMove(row, col, stoneType);

    //                         if (processor.IsMoveLegal(game, move))
    //                             moves.Add(move);
    //                     }
    //                 }
    //                 else if (processor is GomokuMoveProcessor)
    //                 {
    //                     GomokuMove move =
    //                         new GomokuMove(row, col, 'O');

    //                     if (processor.IsMoveLegal(game, move))
    //                         moves.Add(move);
    //                 }
    //                 else if (processor is ReversiMoveProcessor)
    //                 {
    //                     ReversiMove move =
    //                         new ReversiMove(row, col);

    //                     if (processor.IsMoveLegal(game, move))
    //                         moves.Add(move);
    //                 }
    //                 else
    //                 {
    //                     throw new NotSupportedException(
    //                         "Unknown move processor.");
    //                 }
    //             }
    //         }

    //         return moves;
    //     }
    // }

    public class RandomMoveStrategy : IMoveStrategy
    {
        private readonly Random random = new Random();

        public Move ChooseMove(Game game)
        {
            List<Move> moves = game.MoveProcessor.GetLegalMoves(game, game.CurrentPlayer);

            if (moves.Count == 0)
            {
                
                // if (game.MoveProcessor is ReversiMoveProcessor) // violation of Open-Closed rule
                return new PassMove(); // Passing only applies to Reversi

                // throw new InvalidOperationException(
                //     "No legal Gomoku moves available.");
            }

            return moves[random.Next(moves.Count)];
        }
    }

    public class GomokuSmartStrategy : IMoveStrategy
    {
        private readonly int winLength;
        private readonly Random random = new Random();

        public GomokuSmartStrategy(int winLength = 5)
        {
            this.winLength = winLength;
        }

        public Move ChooseMove(Game game)
        {
            List<Move> moves = game.MoveProcessor.GetLegalMoves(game, game.CurrentPlayer);

            if (moves.Count == 0)
                throw new InvalidOperationException(
                    "No legal Gomoku moves available.");

            Board board = game.Board;
            int player = game.CurrentPlayer.Number;
            int opponent = 3 - player;

            // 1. Immediate winning move
            foreach (Move move in moves)
            {
                if (move is GomokuMove gomokuMove &&
                    gomokuMove.StoneType != 'E' &&
                    CountLine(board, gomokuMove.Row, gomokuMove.Col,
                            player) >= winLength)
                {
                    return gomokuMove;
                }
            }

            // 2. Block opponent's winning move
            foreach (Move move in moves)
            {
                if (move is GomokuMove gomokuMove &&
                    gomokuMove.StoneType == 'O' &&
                    CountLine(board, gomokuMove.Row, gomokuMove.Col,
                            opponent) >= winLength)
                {
                    return gomokuMove;
                }
            }

            // 3. Random legal move
            return moves[random.Next(moves.Count)];
        }

        private int CountLine(Board board, int row, int col, int owner)
        {
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
    
    public abstract class ReversiScoringStrategy : IMoveStrategy
    {
        public Move ChooseMove(Game game)
        {
            List<Move> moves = game.MoveProcessor.GetLegalMoves(game, game.CurrentPlayer);

            if (moves.Count == 0)
                return new PassMove();

            if (game.MoveProcessor is not ReversiMoveProcessor processor)
                throw new InvalidOperationException(
                    "Reversi strategy requires ReversiMoveProcessor.");

            ReversiMove best = (ReversiMove)moves[0];
            int bestScore = int.MinValue;

            foreach (Move move in moves)
            {
                ReversiMove reversiMove = (ReversiMove)move;

                int flips = processor.CountFlips(game, reversiMove);

                int score = Score(game.Board, reversiMove, flips);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = reversiMove;
                }
            }

            return best;
        }

        protected abstract int Score(
            Board board, ReversiMove move, int flips);
    }
    
    public class GreedyReversiStrategy : ReversiScoringStrategy
    {
        protected override int Score(
            Board board, ReversiMove move, int flips)
        {
            return flips;
        }
    }

    public class CornerReversiStrategy : ReversiScoringStrategy
    {
        protected override int Score(
            Board board, ReversiMove move, int flips)
        {
            int bonus = board.IsCorner(move.Row, move.Col) ? 1000 : 0;

            return bonus + flips;
        }
    }
    public class AntiReversiStrategy : ReversiScoringStrategy
    {
        protected override int Score(
            Board board, ReversiMove move, int flips)
        {
            int penalty = board.IsCorner(move.Row, move.Col) ? 1000 : 0;

            return -flips - penalty;
        }
    }
}