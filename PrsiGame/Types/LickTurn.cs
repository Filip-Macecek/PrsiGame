using FluentResults;

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

    public Result Validate(Turn previousTurn)
    {
        throw new NotImplementedException();
    }
}