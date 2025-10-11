using CoreLedger.Domain.Errors;

namespace CoreLedger.Api.Middlewares;

public sealed class DomainErrorMiddleware(ILogger<DomainErrorMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (DomainError de)
        {
            logger.LogWarning(de, "Domain invariant violation: {Message}", de.Message);
            
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            await ctx.Response.WriteAsJsonAsync(new { error = "domain_invariant", message = de.Message });
        }
    }
}