﻿using FluentResults;
using PrsiGame.Common;

namespace PrsiGame.Types;

public sealed record SevenCard : Card
{
    private SevenCard(CardId cardId, CardType cardType, CardColor color, CardValue cardValue) : base(cardId, cardType, color, cardValue)
    {
    }

    public static Result<SevenCard> Create(CardId cardId)
    {
        var checkType = CheckType(CardType.Seven, cardId);
        if (checkType.IsFailed)
        {
            return checkType;
        }

        return new SevenCard(cardId, cardId.ToCardType(), cardId.ToColor(), cardId.ToCardValue());
    }
}
