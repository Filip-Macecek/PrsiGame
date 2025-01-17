using FluentResults;

namespace PrsiGame.Errors
{
    public class InvalidTurnError : Error
    {
        public InvalidTurnError(string message) : base(message)
        {
        }
    }
}
