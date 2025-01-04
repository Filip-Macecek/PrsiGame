using FluentResults;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public abstract record CardTurn(Card Card, Player Player) : Turn(Player)
{
    public Result Validate(Card lastCard, CardColor? colorOverride, bool specialCardApplies)
    {
        return Result
            .FailIf(!Player.CardsOnHand.Contains(Card.Id), () => new InvalidTurnError("Card is not in the player's hand."))
            .Bind(() => ValidateInternal(lastCard, colorOverride, specialCardApplies));
    }

    protected abstract Result ValidateInternal(Card lastCard, CardColor? colorOverride, bool specialCardApplies);
}
