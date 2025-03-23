using System;
using System.Collections.Generic;
using PrsiGame.Types;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public sealed class GameDto
    {
        public GameDto(IEnumerable<TurnDto> turns, IEnumerable<CardId> lickPile, IEnumerable<CardId> discardPile)
        {
            Turns = turns;
            LickPile = lickPile;
            DiscardPile = discardPile;
        }

        public IEnumerable<TurnDto> Turns { get; }

        public IEnumerable<CardId> LickPile { get; }

        public IEnumerable<CardId> DiscardPile { get; }
    }
}
