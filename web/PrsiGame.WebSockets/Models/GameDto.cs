using System;
using System.Collections.Generic;
using PrsiGame.Types;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public sealed class GameDto
    {
        public IEnumerable<TurnDto> Turns { get; }

        public IEnumerable<CardId> LickPile { get; }

        public IEnumerable<CardId> DiscardPile { get; }

        public IEnumerable<PlayerDto> PlayerQueue { get; }
    }
}
