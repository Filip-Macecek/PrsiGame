namespace PrsiGame.Types
{
    public class GameSetup
    {
        public ushort PlayerCount { get; }
        public ushort PlayerCardCount { get; }

        public GameSetup(ushort PlayerCount, ushort PlayerCardCount)
        {
            this.PlayerCount = PlayerCount;
            this.PlayerCardCount = PlayerCardCount;
        }
    }
}
