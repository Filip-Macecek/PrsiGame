namespace PrsiGame.Types;

public record Game(
    GameState State,
    GameSetup Setup,
    Stack<Turn> Turns,
    Stack<CardId> LickPile,
    Stack<CardId> DiscardPile,
    IReadOnlyList<Player> Players,
    CardColor? CurrentColor
);
