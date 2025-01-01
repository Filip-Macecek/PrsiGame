using System.Drawing;
using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record AceTurn : Turn
{
    public AceCard Card { get; }

    private AceTurn(AceCard card, Player player) : base(player)
    {
        Card = card;
    }

    public static AceTurn Create(Player player, AceCard card)
    {
        return new AceTurn(card, player);
    }

    public Result Validate(Turn previousTurn, Card discardPileTop)
    {
        return previousTurn switch
        {
            AceTurn aceTurn => Result.Ok(),
            SkipTurn _ => Validate(discardPileTop),
            _ => throw new ArgumentOutOfRangeException(nameof(previousTurn))
        };
    }

    private Result Validate(Card discardPileTop)
    {
        if (discardPileTop.Color != Card.Color)
        {
            return new InvalidTurnError("Color mismatch.");
        }

        return Result.Ok();
    }
}
