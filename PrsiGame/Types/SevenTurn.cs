using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record SevenTurn : CardTurn
{
    private SevenTurn(SevenCard card, Player player) : base(card, player)
    {
        Card = card;
    }

    public static SevenTurn Create(Player player, SevenCard card)
    {
        return new SevenTurn(card, player);
    }

    protected override Result ValidateInternal(Card lastCard, CardColor? colorOverride, bool specialCardApplies)
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
