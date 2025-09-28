namespace CoreLedger.Domain.Errors;

public sealed class InvalidOperationError(string message) : DomainError(message);