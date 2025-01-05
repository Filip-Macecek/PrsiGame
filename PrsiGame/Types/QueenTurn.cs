using System;
using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types
{
    public sealed class QueenTurn : CardTurn
    {
        private QueenTurn(Player player, QueenCard card, CardColor pickedColor) : base(card, player)
        {
            PickedColor = pickedColor;
        }

        public CardColor PickedColor { get; }

        public static QueenTurn Create(Player player, QueenCard card, CardColor pickedColor)
        {
            return new QueenTurn(player, card, pickedColor);
        }

        protected override Result ValidateInternal(Card card, CardColor? colorOverride, bool applies)
        {
            switch (card)
            {
                case AceCard aceCard:
                    return Validate(aceCard, applies);
                case QueenCard queenCard:
                case RegularCard regularCard:
                    return Result.Ok();
                case SevenCard sevenCard:
                    return Validate(sevenCard, applies);
                default:
                    throw new ArgumentOutOfRangeException(nameof(card));
            }
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
}
