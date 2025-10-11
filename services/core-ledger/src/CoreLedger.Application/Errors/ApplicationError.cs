namespace CoreLedger.Application.Errors;

public abstract class ApplicationError(string message) : Exception(message);