using System;
using System.Collections.Generic;
using System.Linq;
using PrsiGame.Types;

namespace PrsiGame
{
    public static class GameFactory
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

            var cards = (int[])Enum.GetValues(typeof(CardId));
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
                discardPile: new Stack<CardId>(new []{ firstDiscardCard }),
                players: players,
                playerQueue: new Queue<Player>(players)
            );
        }
    }
}
