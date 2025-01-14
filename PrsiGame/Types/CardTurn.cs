using FluentResults;
using PrsiGame.Common;
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

        public static Result<CardTurn> CreateCorrectType(CardId cardId, Player player)
        {
            var cardObject = cardId.ToCardObject();

            if (cardObject is AceCard aceCard)
            {
                return AceTurn.Create(player, aceCard);
            }
            if (cardObject is SevenCard sevenCard)
            {
                return SevenTurn.Create(player, sevenCard);
            }
            if (cardObject is QueenCard queenCard)
            {
                return QueenTurn.Create(player, queenCard, queenCard.Color);
            }
            if (cardObject is RegularCard regularCard)
            {
                return RegularTurn.Create(player, regularCard);
            }

            return Result.Fail(new InvalidCardTypeError($"Unknown card type {cardId.ToString()}."));
        }

        protected abstract Result ValidateInternal(Card lastCard, CardColor? colorOverride, bool specialCardApplies);
    }
}
