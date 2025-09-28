namespace CoreLedger.Domain.Errors;

public abstract class DomainError(string message) : Exception(message);