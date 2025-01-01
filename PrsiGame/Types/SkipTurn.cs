using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record SkipTurn : Turn
{
    private SkipTurn(Player player) : base(player)
    {
    }

    public static SkipTurn Create(Player player)
    {
        return new SkipTurn(player);
    }

    public Result Validate(Turn previousTurn)
    {
        if (previousTurn is not AceTurn)
        {
            return new InvalidTurnError("Turn can only be skipped if the previous player played an ace.");
        }

        return Result.Ok();
    }
}
