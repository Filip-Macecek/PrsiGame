using FluentResults;
using PrsiGame.Common;

namespace PrsiGame.Types;

public sealed record AceCard : Card
{
    private AceCard(CardId cardId, CardType cardType, CardColor color, CardValue cardValue) : base(cardId, cardType, color, cardValue)
    {
    }

    public static Result<AceCard> Create(CardId cardId)
    {
        var checkType = CheckType(CardType.Ace, cardId);
        if (checkType.IsFailed)
        {
            return checkType;
        }

        return new AceCard(cardId, cardId.ToCardType(), cardId.ToColor(), cardId.ToCardValue());
    }
}
