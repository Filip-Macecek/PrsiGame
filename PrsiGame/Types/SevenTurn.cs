using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record SevenTurn : Turn
{
    private SevenTurn(SevenCard card, Player player) : base(player)
    {
        Card = card;
    }

    public SevenCard Card { get; }

    public static SevenTurn Create(Player player, SevenCard card)
    {
        return new SevenTurn(card, player);
    }

    public Result Validate(Card lastCard, CardColor? colorOverride, bool specialCardApplies)
    {
        return lastCard switch
        {
            AceCard aceCard => Validate(aceCard, specialCardApplies),
            QueenCard queenCard => Validate(colorOverride!.Value),
            RegularCard regularCard => Validate(lastCard.Color),
            SevenCard sevenCard => Result.Ok(),
            _ => throw new ArgumentOutOfRangeException(nameof(lastCard))
        };
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
