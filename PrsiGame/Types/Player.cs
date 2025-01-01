namespace PrsiGame.Types;

public sealed record Player(uint PlayerOrder, PlayerState State, List<CardId> CardsOnHand);