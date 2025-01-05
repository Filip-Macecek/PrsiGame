using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types
{
    public abstract class CardTurn : Turn
    {
        public Card Card { get; }
        public Player Player { get; }

        protected CardTurn(Card Card, Player Player) : base(Player)
        {
            this.Card = Card;
            this.Player = Player;
        }

        public Result Validate(Card lastCard, CardColor? colorOverride, bool specialCardApplies)
        {
            return Result
                .FailIf(!Player.CardsOnHand.Contains(Card.Id), () => new InvalidTurnError("Card is not in the player's hand."))
                .Bind(() => ValidateInternal(lastCard, colorOverride, specialCardApplies));
        }

        protected abstract Result ValidateInternal(Card lastCard, CardColor? colorOverride, bool specialCardApplies);
    }
}
