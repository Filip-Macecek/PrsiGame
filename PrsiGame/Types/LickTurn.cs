using System;
using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types
{
    public sealed class LickTurn : Turn
    {
        private LickTurn(Player player, int lickCount) : base(player)
        {
            LickCount = lickCount;
        }

        public int LickCount { get; }

        public static LickTurn Create(Player player, int lickCount)
        {
            return new LickTurn(player, lickCount);
        }

        public Result Validate(TurnType lastTurn, int? requiredLickCount)
        {
            switch (lastTurn)
            {
                case TurnType.AceTurn:
                    return new InvalidTurnError("The player is currently stunned.");
                case TurnType.LickTurn:
                case TurnType.QueenTurn:
                case TurnType.RegularTurn:
                    return ValidateSingleCardLick();
                case TurnType.SevenTurn:
                    return Result.FailIf(requiredLickCount.Value != LickCount,
                        () => new InvalidTurnError($"Player must lick {requiredLickCount.Value} cards."));
                case TurnType.SkipTurn:
                    return ValidateSingleCardLick();
                default:
                    throw new ArgumentOutOfRangeException(nameof(lastTurn), lastTurn, null);
            }
        }

        private Result ValidateSingleCardLick()
        {
            return LickCount == 1 ? Result.Ok() : new InvalidTurnError("The player can lick only a single card.");
        }
    }
}
