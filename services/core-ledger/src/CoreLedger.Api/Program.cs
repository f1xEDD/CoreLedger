using CoreLedger.Api;
using CoreLedger.Api.Dto.Accounts;
using CoreLedger.Api.Dto.Transfers;
using CoreLedger.Application.Services;
using CoreLedger.Infrastructure;
using CoreLedger.Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<LedgerDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("LedgerDb"),
        npg => npg.MigrationsHistoryTable("__ef_migrations_history", "public"));
});

builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<LedgerDbContext>("db");

builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = r => r.Name == "self",
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapPost("/accounts", async (CreateAccountRequest req, IAccountService svc, CancellationToken ct) =>
{
    var result = await svc.CreateAsync(req.Currency, ct);
    
    return ApiResults.From(result, id => Results.Created($"/accounts/{id}", new CreateAccountResponse(id)));
});

app.MapPost("/accounts/{id:guid}/close", async (Guid id, IAccountService svc, CancellationToken ct) =>
{
    var result = await svc.CloseAsync(id, ct);
    
    return ApiResults.From(result, _ => Results.Ok(new { closed = true }));
});

app.MapPost("/transfers",
    async (HttpRequest http, CreateTransferRequest req, ITransferService svc, CancellationToken ct) =>
    {
        var key = http.Headers["Idempotency-Key"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(key))
        {
            return Results.BadRequest("Missing Idempotency-Key header.");
        }

        try
        {
            var booking = req.BookingDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var value = req.ValueDate ?? booking;

            var result = await svc.CreateAsync(
                key,
                req.FromAccountId,
                req.ToAccountId,
                req.Amount,
                req.Currency,
                booking,
                value,
                ct);

            return ApiResults.From(result, id => Results.Created($"/transfers/{id}", new CreateTransferResponse(id)));
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message);
        }
    });

app.Run();