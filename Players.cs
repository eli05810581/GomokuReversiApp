namespace GomokuReversiApp
{

    public abstract class Player
    {
        public int Number {get;} // 1 or 2
        protected Player(int number)
        {
            Number = number;
        }
        public abstract Move GetMove(Board board, IGameRules rules);
    }

}