using FluentResults;

namespace PrsiGame.Errors;

public class InvalidGameStateError(string message) : Error(message);
