using FluentResults;

namespace PrsiGame.Errors
{
    public class InvalidGameStateError : Error
    {
        public InvalidGameStateError(string message) : base(message)
        {
        }
    }
}
