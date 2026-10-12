namespace GomokuReversiApp
{
    public abstract class Piece
    {
        public int Owner { get; }

        protected Piece(int owner)
        {
            Owner = owner;
        }

        public abstract char Symbol { get; }
    }

    public class OrdinaryStone : Piece
    {
        public OrdinaryStone(int owner) : base(owner) { }

        public override char Symbol => Owner == 1 ? 'X' : 'O';
    }

    public class HeavyStone : Piece
    {
        public HeavyStone(int owner) : base(owner) { }

        public override char Symbol => Owner == 1 ? '#' : '@';
    }

    public class Disc : Piece
    {
        public Disc(int owner) : base(owner) { }

        public override char Symbol => Owner == 1 ? 'X' : 'O';
    }
}