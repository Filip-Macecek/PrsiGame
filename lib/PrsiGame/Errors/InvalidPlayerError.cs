using FluentResults;

namespace PrsiGame.Errors
{
    public class InvalidPlayerError : Error
    {
        public InvalidPlayerError(string message) : base(message)
        {
        }
    }
}
