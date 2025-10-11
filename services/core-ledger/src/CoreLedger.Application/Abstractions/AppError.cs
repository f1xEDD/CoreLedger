namespace CoreLedger.Application.Abstractions;

public abstract record AppError(string Code, string Message)
{
    public static AppError NotFound(string msg) => new NotFoundError(msg);
    public static AppError Conflict(string msg) => new ConflictError(msg);
    public static AppError Invalid(string msg) => new InvalidError(msg);
}

public sealed record NotFoundError(string Message) : AppError("not_found", Message);

public sealed record ConflictError(string Message) : AppError("conflict", Message);

public sealed record InvalidError(string Message) : AppError("invalid", Message);