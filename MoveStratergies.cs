namespace GomokuReversiApp
{
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

            ReversiMove best = (ReversiMove)moves[0]; // Pick the first move of all legal move to be the best initially
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

    public class GreedyReversiStrategy: ReversiScoringStrategy
    {
        protected override int Score(Board board, ReversiMove move)
        {
            return move.FlipCount;
        }
    }

    public class CornerReversiStrategy: ReversiScoringStrategy
    {
        protected override int Score(Board board, ReversiMove move)
        {
            int bonus = board.IsCorner(move.Row, move.Col) ? 1000 : 0;
            return bonus + move.FlipCount;
        }
    }

    public class AntiReversiStrategy: ReversiScoringStrategy
    {
        protected override int Score(Board board, ReversiMove move)
        {
            int penalty = board.IsCorner(move.Row, move.Col) ? 1000 : 0;
            return -move.FlipCount - penalty;
        }
    }


}