using System.Collections.Generic;

namespace PrsiGame.Types
{
    public sealed class Player
    {
        public List<CardId> CardsOnHand { get; }

        public Player(List<CardId> CardsOnHand)
        {
            this.CardsOnHand = CardsOnHand;
        }
    }
}
