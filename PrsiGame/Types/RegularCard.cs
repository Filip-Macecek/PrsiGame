using FluentResults;
using PrsiGame.Common;

namespace PrsiGame.Types
{
    public class RegularCard : Card
    {
        private RegularCard(CardId cardId, CardType cardType, CardColor color, CardValue cardValue) : base(cardId, cardType, color, cardValue)
        {
        }

        public static Result<RegularCard> Create(CardId cardId)
        {
            var checkType = CheckType(CardType.Regular, cardId);
            if (checkType.IsFailed)
            {
                return checkType;
            }

            return new RegularCard(cardId, cardId.ToCardType(), cardId.ToColor(), cardId.ToCardValue());
        }

        // public Result ValidateAsTopOfPile(RegularCard previousTop)
        // {
        //     if (previousTop.Color != Color || previousTop.CardValue != CardValue)
        //     {
        //         return new InvalidTurnError("Color or value does not match the top of the discard pile.");
        //     }
        //
        //     return Result.Ok();
        // }
        //
        // public Result ValidateAsTopOfPile(AceCard previousTop, Turn previousTurn)
        // {
        //     if (previousTop.Color != Color)
        //     {
        //         return new InvalidTurnError("Color does not match the top of the discard pile.");
        //     }
        //
        //     if (previousTurn.TurnType != TurnType.Skip)
        //     {
        //         return new InvalidTurnError("");
        //     }
        // }
    }
}
