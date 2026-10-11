namespace GomokuReversiApp
{
    public abstract class Piece
    {
        public int Owner {get;}
        protected Piece(int owner)
        {
            Owner = owner;
        }

        public abstract char Symbol {get;}
        public virtual bool CanBeErased
        {
            get
            {
                return false;
            }
        }
    }

    public class OrdinaryStone: Piece
    {
        public OrdinaryStone(int owner): base(owner) {}
        public override char Symbol
        {
            get
            {
                return Owner == 1 ? 'X' : 'O';
            }
        }
        public override bool CanBeErased
        {
            get
            {
                return true;
            }
        }
    }

    public class HeavyStone: Piece
    {
        public HeavyStone(int owner): base(owner) {}
        public override char Symbol
        {
            get
            {
                return Owner == 1 ? '#' : '@';
            }
        }
    }

    public class Disk: Piece
    {
        public Disk(int owner): base(owner) {}
        public override char Symbol
        {
            get
            {
                return Owner == 1 ? 'X' : 'O';
            }
        }
    }
}