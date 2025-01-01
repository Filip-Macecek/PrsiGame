using FluentResults;

namespace PrsiGame.Errors;

public class InvalidCardTypeError(string message) : Error(message);
