using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public sealed record RegularTurn : Turn
{
    private RegularTurn(RegularCard card, Player player) : base(player)
    {
        Card = card;
    }

    public RegularCard Card { get; }

    public static RegularTurn Create(Player player, RegularCard card)
    {
        return new RegularTurn(card, player);
    }

    public Result Validate(Card lastCard, CardColor? colorOverride, bool specialCardApplies)
    {
        return lastCard switch
        {
            AceCard aceCard => Validate(aceCard, specialCardApplies),
            QueenCard queenCard => Validate(queenCard.CardValue, colorOverride!.Value),
            RegularCard regularCard => Validate(regularCard.CardValue, regularCard.Color),
            SevenCard sevenCard => Validate(sevenCard, specialCardApplies),
            _ => throw new ArgumentOutOfRangeException(nameof(lastCard))
        };
    }

    private Result Validate(CardValue cardValue, CardColor color)
    {
        if (cardValue != Card.CardValue && color != Card.Color)
        {
            return new InvalidTurnError("Card value or color does not match.");
        }

        return Result.Ok();
    }

    private Result Validate(AceCard card, bool applies)
    {
        if (applies)
        {
            return new InvalidTurnError("The player is stunned.");
        }

        return Validate(card.CardValue, card.Color);
    }

    private Result Validate(SevenCard card, bool applies)
    {
        if (applies)
        {
            return new InvalidTurnError("The player is stunned.");
        }

        return Validate(card.CardValue, card.Color);
    }
}
