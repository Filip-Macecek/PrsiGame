using System;

namespace PrsiGame.WebSockets.Models
{
    public class PlayerDto
    {
        public PlayerDto(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public Guid Id { get; }

        public string Name { get; }
    }
}
