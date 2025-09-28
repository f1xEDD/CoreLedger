using CoreLedger.Domain.Errors;

namespace CoreLedger.Domain.Abstractions;

public static class Guard
{
    public static void AgainstNull<T>(T? value, string name) where T : class
    {
        if (value is null)
        {
            throw new InvalidOperationError($"{name} must not be null.");
        }
    }

    public static void AgainstDefault(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new InvalidOperationError($"{name} must not be empty.");
        }
    }

    public static void That(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvariantViolation(message);
        }
    }

    public static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationError(message);
        }
    }
}