using FluentResults;

namespace PrsiGame.Errors;

public class InvalidTurnError(string message) : Error(message);
