namespace CoreLedger.Domain.Errors;

public sealed class InvariantViolation(string message) : DomainError(message);