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
    }

}