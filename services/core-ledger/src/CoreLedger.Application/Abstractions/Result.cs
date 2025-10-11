namespace CoreLedger.Application.Abstractions;

public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public AppError? Error { get; }

    private Result(bool ok, T? v, AppError? e)
    {
        IsSuccess = ok;
        Value = v;
        Error = e;
    }

    public static Result<T> Ok(T v) => new(true, v, null);
    public static Result<T> Fail(AppError e) => new(false, default, e);
}