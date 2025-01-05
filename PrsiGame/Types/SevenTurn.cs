using System;
using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types
{
    public sealed class SevenTurn : CardTurn
    {
        private SevenTurn(SevenCard card, Player player) : base(card, player)
        {
        }

        public static SevenTurn Create(Player player, SevenCard card)
        {
            return new SevenTurn(card, player);
        }

        protected override Result ValidateInternal(Card lastCard, CardColor? colorOverride, bool specialCardApplies)
        {
            switch (lastCard)
            {
                case AceCard aceCard:
                    return Validate(aceCard, specialCardApplies);
                case QueenCard queenCard:
                    return Validate(colorOverride.Value);
                case RegularCard regularCard:
                    return Validate(lastCard.Color);
                case SevenCard sevenCard:
                    return Result.Ok();
                default:
                    throw new ArgumentOutOfRangeException(nameof(lastCard));
            }
        }

        private Result Validate(CardColor currentColor)
        {
            if (currentColor != Card.Color)
            {
                return new InvalidTurnError("Color mismatch.");
            }

            return Result.Ok();
        }

        private Result Validate(AceCard aceCard, bool applies)
        {
            if (applies)
            {
                return new InvalidTurnError("The player is stunned.");
            }

            return Validate(aceCard.Color);
        }
    }
}
