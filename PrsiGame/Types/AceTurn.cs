using System;
using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types
{
    public sealed class AceTurn : CardTurn
    {
        private AceTurn(AceCard card, Player player) : base(card, player)
        {
        }

        public static AceTurn Create(Player player, AceCard card)
        {
            return new AceTurn(card, player);
        }

        protected override Result ValidateInternal(Card card, CardColor? currentColorOverride, bool specialCardApplies)
        {
            switch (card)
            {
                case AceCard aceCard:
                    return Result.Ok();
                case QueenCard queenCard:
                    return Validate(currentColorOverride.Value);
                case RegularCard regularCard:
                    return Validate(regularCard.Color);
                case SevenCard sevenCard:
                    return Validate(sevenCard, specialCardApplies);
                default:
                    throw new ArgumentOutOfRangeException(nameof(card));
            }
        }

        private Result Validate(SevenCard sevenCard, bool applies)
        {
            if (applies)
            {
                return new InvalidTurnError("Player must lick the pile.");
            }

            if (sevenCard.Color != Card.Color)
            {
                return new InvalidTurnError("Card colors do not match.");
            }

            return Result.Ok();
        }

        private Result Validate(CardColor color)
        {
            if (Card.Color != color)
            {
                return new InvalidTurnError("Colors do not match.");
            }

            return Result.Ok();
        }
    }
}
