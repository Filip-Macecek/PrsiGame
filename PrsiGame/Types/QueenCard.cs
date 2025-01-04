using FluentResults;
using PrsiGame.Common;

namespace PrsiGame.Types;

public sealed record QueenCard : Card
{
    private QueenCard(CardId cardId, CardType cardType, CardColor color, CardValue cardValue) : base(cardId, cardType, color, cardValue)
    {
    }

    public static Result<QueenCard> Create(CardId cardId)
    {
        var checkType = CheckType(CardType.Queen, cardId);
        if (checkType.IsFailed)
        {
            return checkType;
        }

        return new QueenCard(cardId, cardId.ToCardType(), cardId.ToColor(), cardId.ToCardValue());
    }
}
