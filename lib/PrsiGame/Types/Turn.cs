namespace PrsiGame.Types
{
    public abstract class Turn
    {
        public Player Player { get; }

        protected Turn(Player Player)
        {
            this.Player = Player;
        }
    }
}
