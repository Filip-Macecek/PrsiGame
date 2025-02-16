﻿using PrsiGame.Types;

namespace PrsiGame.Common;

public static class TypeExtensions
{
    private const int CardValueIndexOffset = 6;

    private const int CardColorOffset = 10;

    public static CardColor ToColor(this CardId cardId)
    {
        return ((int)cardId / CardColorOffset) switch
        {
            0 => CardColor.Hearts,
            1 => CardColor.Diamonds,
            2 => CardColor.Clubs,
            3 => CardColor.Spades,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static CardValue ToCardValue(this CardId cardId)
    {
        var cardColorDiscard = (int)cardId % CardColorOffset;
        return (CardValue)(cardColorDiscard + CardValueIndexOffset);
    }

    public static CardType ToCardType(this CardId cardId)
    {
        switch (cardId)
        {
            case CardId.AceOfClubs:
            case CardId.AceOfSpades:
            case CardId.AceOfHearts:
            case CardId.AceOfDiamonds:
                return CardType.Ace;
            case CardId.SevenOfClubs:
            case CardId.SevenOfDiamonds:
            case CardId.SevenOfHearts:
            case CardId.SevenOfSpades:
                return CardType.Seven;
            case CardId.QueenOfDiamonds:
            case CardId.QueenOfClubs:
            case CardId.QueenOfHearts:
            case CardId.QueenOfSpades:
                return CardType.Queen;
            default:
                return CardType.Regular;
        };
    }

    public static Card ToCardObject(this CardId cardId)
    {
        return cardId.ToCardType() switch
        {
            CardType.Regular => RegularCard.Create(cardId).Value,
            CardType.Ace => AceCard.Create(cardId).Value,
            CardType.Queen => QueenCard.Create(cardId).Value,
            CardType.Seven => SevenCard.Create(cardId).Value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static TurnType ToTurnType(this Turn turn)
    {
        return turn switch
        {
            AceTurn aceTurn => TurnType.AceTurn,
            QueenTurn queenTurn => TurnType.QueenTurn,
            RegularTurn regularTurn => TurnType.RegularTurn,
            SevenTurn sevenTurn => TurnType.SevenTurn,
            LickTurn lickTurn => TurnType.LickTurn,
            SkipTurn skipTurn => TurnType.SkipTurn,
            _ => throw new ArgumentOutOfRangeException(nameof(turn))
        };
    }
}
