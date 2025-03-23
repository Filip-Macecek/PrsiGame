using System;
using System.Collections.Generic;
using PrsiGame.Types;

namespace PrsiGame.WebSockets.Models
{
    [Serializable]
    public class PlayerDto
    {
        public PlayerDto(Guid id, string name, List<CardId> cards)
        {
            Id = id;
            Name = name;
            Cards = cards;
        }

        public Guid Id { get; }

        public string Name { get; }

        public List<CardId> Cards { get; }
    }
}
