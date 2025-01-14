using System;
using System.Linq;
using PrsiGame.Common;
using PrsiGame.Types;

namespace PrsiGame
{
    public static class AI
    {
        public static Turn Turn(Game game)
        {
            var player = game.GetCurrentPlayer();
            var topCard = game.DiscardPile.Peek();
            var topCardApplies = game.TopCardAppliesToCurrentTurn();

            if (topCard.ToCardType() == CardType.Ace && topCardApplies)
            {
                var aces = player.CardsOnHand.Where(c => c.ToCardType() == CardType.Ace).ToList();
                if (aces.Any())
                {
                    return AceTurn.Create(player, AceCard.Create(aces.First()).Value);
                }
                else
                {
                    return SkipTurn.Create(player);
                }
            }

            if (topCard.ToCardType() == CardType.Seven && topCardApplies)
            {
                return LickTurn.Create(player, game.GetRequiredLicks().Value);
            }

            var currentValue = topCard.ToCardValue();
            var currentColor = game.GetCurrentColor();

            var possibleCards = player.CardsOnHand.Where(c => c.ToCardValue() == currentValue || c.ToColor() == currentColor).ToList();
            var random = new Random();

            if (possibleCards.Any())
            {
                var randomIndex = random.Next(possibleCards.Count);
                var cardToPlay = possibleCards[randomIndex];
                var cardToPlayType = cardToPlay.ToCardType();

                switch (cardToPlayType)
                {
                    case CardType.Regular:
                        return RegularTurn.Create(player, RegularCard.Create(cardToPlay).Value);
                    case CardType.Seven:
                        return SevenTurn.Create(player, SevenCard.Create(cardToPlay).Value);
                    case CardType.Queen:
                        var preferredColor = (CardColor)random.Next(4);
                        return QueenTurn.Create(player, QueenCard.Create(cardToPlay).Value, preferredColor);
                    case CardType.Ace:
                        return AceTurn.Create(player, AceCard.Create(cardToPlay).Value);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return LickTurn.Create(player, 1);
            }
        }
    }
}
