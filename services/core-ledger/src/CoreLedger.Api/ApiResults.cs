using CoreLedger.Application.Abstractions;

namespace CoreLedger.Api;

public static class ApiResults
{
    public static IResult From<T>(Result<T> r, Func<T, IResult> onOk)
    {
        if (r.IsSuccess)
        {
            return onOk(r.Value!);
        }

        return r.Error switch
        {
            NotFoundError nf => Results.NotFound(new { error = nf.Code, message = nf.Message }),
            ConflictError cf => Results.Conflict(new { error = cf.Code, message = cf.Message }),
            InvalidError iv => Results.BadRequest(new { error = iv.Code, message = iv.Message }),
            _ => Results.Problem(title: "Unexpected error", detail: r.Error?.Message)
        };
    }
}