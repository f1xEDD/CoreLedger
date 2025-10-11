namespace CoreLedger.Application.Errors;

public sealed class NotFoundError(string message) : ApplicationError(message);