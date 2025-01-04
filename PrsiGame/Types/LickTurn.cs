using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record LickTurn : Turn
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

    public Result Validate(Turn turn, int? requiredLickCount)
    {
        return turn switch
        {
            AceTurn aceTurn => new InvalidTurnError("The player is currently stunned."),
            LickTurn lickTurn => ValidateSingleCardLick(),
            QueenTurn queenTurn => new InvalidTurnError("Cannot lick when stunned."),
            RegularTurn regularTurn => ValidateSingleCardLick(),
            SevenTurn sevenTurn => Validate(sevenTurn, requiredLickCount ?? int.MaxValue),
            SkipTurn skipTurn => ValidateSingleCardLick(),
            _ => throw new ArgumentOutOfRangeException(nameof(turn))
        };
    }

    private Result Validate(SevenTurn turn, int requiredCountLick)
    {
        if (requiredCountLick != LickCount)
        {
            return new InvalidTurnError($"Player must lick {requiredCountLick} cards.");
        }

        return Result.Ok();
    }

    private Result ValidateSingleCardLick()
    {
        return LickCount == 1 ? Result.Ok() : new InvalidTurnError("The player can lick only a single card.");
    }
}
