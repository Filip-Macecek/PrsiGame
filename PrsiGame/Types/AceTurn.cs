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

    public Result Validate(Card card, CardColor? currentColorOverride, bool specialCardApplies)
    {
        return card switch
        {
            AceCard aceCard => Result.Ok(),
            QueenCard queenCard => Validate(currentColorOverride!.Value),
            RegularCard regularCard => Validate(regularCard.Color),
            SevenCard sevenCard => Validate(sevenCard, specialCardApplies),
            _ => throw new ArgumentOutOfRangeException(nameof(card))
        };
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
