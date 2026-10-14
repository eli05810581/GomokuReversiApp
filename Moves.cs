namespace GomokuReversiApp
{
    public abstract class Move {}
    public class PassMove: Move {}
    public class GomokuMove: Move
    {
        public int Row {get;}
        public int Col {get;}
        public char StoneType {get;} // O, H, E
        public GomokuMove(int row, int col, char stoneType)
        {
            Row = row;
            Col = col;
            StoneType = stoneType;
        }
        public override bool Equals(object? obj)
        {
            return obj is GomokuMove other && Row == other.Row && Col == other.Col && StoneType == other.StoneType;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col, StoneType);
        }
    }
    public class ReversiMove: Move
    {
        public int Row {get;}
        public int Col {get;}
        public ReversiMove(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override bool Equals(object? obj)
        {
            return obj is ReversiMove other && Row == other.Row && Col == other.Col;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }
    }

}