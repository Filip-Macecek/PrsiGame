using System;
using System.Collections.Generic;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class SessionDto
    {
        public SessionDto(Guid id, IEnumerable<PlayerDto> players, SessionStateDto state)
        {
            Id = id;
            Players = players;
            State = state;
        }

        public Guid Id { get; }

        public IEnumerable<PlayerDto> Players { get; }

        public SessionStateDto State { get; }
    }
}
