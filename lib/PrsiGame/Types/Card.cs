using FluentResults;
using PrsiGame.Common;
using PrsiGame.Errors;

namespace PrsiGame.Types
{
    public abstract class Card
    {
        protected Card(CardId cardId, CardType cardType, CardColor color, CardValue cardValue)
        {
            Id = cardId;
            CardType = cardType;
            Color = color;
            CardValue = cardValue;
        }

        public CardId Id { get; }

        public CardType CardType { get; }

        public CardColor Color { get; }

        public CardValue CardValue { get; }

        protected static Result CheckType(CardType expectedType, CardId cardId)
        {
            if (cardId.ToCardType() != expectedType)
            {
                return new InvalidCardTypeError($"Expected {CardType.Regular.ToString()} but got {cardId.ToCardType().ToString()} instead.");
            }

            return Result.Ok();
        }
    }
}
