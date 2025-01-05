using FluentResults;

namespace PrsiGame.Errors
{
    public class InvalidCardTypeError : Error
    {
        public InvalidCardTypeError(string message) : base(message)
        {
        }
    }
}
