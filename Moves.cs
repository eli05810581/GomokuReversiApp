namespace GomokuReversiApp
{
    public abstract class Move {}
public class PassMove: Move {}
public class GomukuMove: Move
{
    public int Row {get;}
    public int Col {get;}
    public char StoneType {get;} // O, H, E
    public GomukuMove(int row, int col, char stoneType)
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
    public int FlipCount {get;} // the number of opponent's pieces to be flipped, calculated by ReversiRules
    public ReversiMove(int row, int col, int flipCount)
    {
        Row = row;
        Col = col;
        FlipCount = flipCount;
    }
}

}