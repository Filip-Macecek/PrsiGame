﻿using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record QueenTurn : CardTurn
{
    private QueenTurn(Player player, QueenCard card, CardColor pickedColor) : base(card, player)
    {
        Card = card;
        PickedColor = pickedColor;
    }

    public CardColor PickedColor { get; }

    public static QueenTurn Create(Player player, QueenCard card, CardColor pickedColor)
    {
        return new QueenTurn(player, card, pickedColor);
    }

    protected override Result ValidateInternal(Card card, CardColor? colorOverride, bool applies)
    {
        return card switch
        {
            AceCard aceCard => Validate(aceCard, applies),
            QueenCard queenCard => Result.Ok(),
            RegularCard regularCard => Result.Ok(),
            SevenCard sevenCard => Validate(sevenCard, applies),
            _ => throw new ArgumentOutOfRangeException(nameof(card))
        };
    }

    private Result Validate(AceCard card, bool applies)
    {
        if (applies)
        {
            return new InvalidTurnError("The player is stunned.");
        }

        return Result.Ok();
    }

    private Result Validate(SevenCard card, bool applies)
    {
        if (applies)
        {
            return new InvalidTurnError("The player should lick.");
        }

        return Result.Ok();
    }
}
