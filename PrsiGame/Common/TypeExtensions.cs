using System;
using PrsiGame.Types;

namespace PrsiGame.Common
{
    public static class TypeExtensions
    {
        private const int CardValueIndexOffset = 6;

        private const int CardColorOffset = 10;

        public static CardColor ToColor(this CardId cardId)
        {
            switch ((int)cardId / CardColorOffset)
            {
                case 0:
                    return CardColor.Hearts;
                case 1:
                    return CardColor.Diamonds;
                case 2:
                    return CardColor.Clubs;
                case 3:
                    return CardColor.Spades;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            switch (cardId.ToCardType())
            {
                case CardType.Regular:
                    return RegularCard.Create(cardId).Value;
                case CardType.Ace:
                    return AceCard.Create(cardId).Value;
                case CardType.Queen:
                    return QueenCard.Create(cardId).Value;
                case CardType.Seven:
                    return SevenCard.Create(cardId).Value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TurnType ToTurnType(this Turn turn)
        {
            switch (turn)
            {
                case AceTurn aceTurn:
                    return TurnType.AceTurn;
                case QueenTurn queenTurn:
                    return TurnType.QueenTurn;
                case RegularTurn regularTurn:
                    return TurnType.RegularTurn;
                case SevenTurn sevenTurn:
                    return TurnType.SevenTurn;
                case LickTurn lickTurn:
                    return TurnType.LickTurn;
                case SkipTurn skipTurn:
                    return TurnType.SkipTurn;
                default:
                    throw new ArgumentOutOfRangeException(nameof(turn));
            }
        }
    }
}
