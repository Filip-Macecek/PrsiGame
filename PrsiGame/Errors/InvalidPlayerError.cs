using FluentResults;

namespace PrsiGame.Errors;

public class InvalidPlayerError(string message) : Error(message);
