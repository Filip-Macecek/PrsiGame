using System;
using System.Collections.Generic;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class SessionDto
    {
        public SessionDto(Guid id, IEnumerable<PlayerDto> players, SessionStateDto state, GameDto game)
        {
            Id = id;
            Players = players;
            State = state;
            Game = game;
        }

        public Guid Id { get; }

        public IEnumerable<PlayerDto> Players { get; }

        public SessionStateDto State { get; }

        public GameDto Game { get; }
    }
}
