using FluentResults;
using PrsiGame.Common;
using PrsiGame.Errors;
using PrsiGame.Types;

namespace PrsiGame;

public static class PrsiEngine
{
    private const ushort CardCount = 32;

    public static Stack<CardId> ShuffleCards()
    {
        var random = new Random();
        var pickedCardIndexes = new HashSet<int>();
        for (var i = 0; i < CardCount; i++)
        {
            var randomIndex = random.Next(0, 32);
            while (pickedCardIndexes.Contains(randomIndex))
            {
                randomIndex = random.Next(0, 32);
            }
            pickedCardIndexes.Add(randomIndex);
        }

        var cards = (int[])Enum.GetValuesAsUnderlyingType(typeof(CardId));
        var shuffledCards = pickedCardIndexes.Select(i => (CardId)cards[i]);
        return new Stack<CardId>(shuffledCards);
    }

    public static Game NewGame(Stack<CardId> deck, GameSetup setup)
    {
        var players = Enumerable.Range(0, setup.PlayerCount).Select(i =>
        {
            var cards = Enumerable.Range(0, setup.PlayerCardCount).Select(_ => deck.Pop());
            return new Player(cards.ToList());
        }).ToList();

        var firstDiscardCard = deck.Pop();

        return new Game(
            state: GameState.Started,
            setup: setup,
            turns: new Stack<Turn>(),
            lickPile: deck,
            discardPile: new Stack<CardId>([firstDiscardCard]),
            players: players,
            playerQueue: new Queue<Player>(players)
        );
    }

    // public static Result PlayRegularCard(Game game, Player player, RegularCard card)
    // {
    //     if (!(player.State == PlayerState.OnTurn || player.CardsOnHand.Contains(card.Id)))
    //     {
    //         return new InvalidTurnError("It's not the player's turn or the card is not in player's hand.");
    //     }
    //
    //     if (game.State != GameState.Ended)
    //     {
    //         return new InvalidGameStateError("Game is not in progress.");
    //     }
    //
    //     if (game.CurrentColor != card.Color)
    //     {
    //         return new InvalidTurnError($"The card to be discarded is wrong color. Current color is {card.Color.ToString()}");
    //     }
    //
    //     var discardPileTop = game.DiscardPile.Peek().ToCardObject();
    //     return discardPileTop switch
    //     {
    //         RegularCard regularCard => ,
    //         AceCard aceCard => throw new NotImplementedException(),
    //         QueenCard queenCard => throw new NotImplementedException(),
    //         SevenCard sevenCard => throw new NotImplementedException(),
    //         _ => throw new ArgumentOutOfRangeException(nameof(discardPileTop))
    //     };
    // }
}
