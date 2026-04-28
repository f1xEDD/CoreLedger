using CoreLedger.Api;
using CoreLedger.Api.Dto.Accounts;
using CoreLedger.Api.Dto.Transfers;
using CoreLedger.Api.Middlewares;
using CoreLedger.Application.Outbox;
using CoreLedger.Application.Services;
using CoreLedger.Domain.Time;
using CoreLedger.Infrastructure;
using CoreLedger.Infrastructure.Outbox;
using CoreLedger.Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

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

builder.Services.RegisterOptions(builder.Configuration);

builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();
builder.Services.AddScoped<IEventPublisher, LoggingEventPublisher>();

builder.Services.AddHostedService<OutboxDispatcher>();

builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<ITransferQueryService, TransferQueryService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountQueryService, AccountQueryService>();

builder.Services.AddTransient<DomainErrorMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
    
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<DomainErrorMiddleware>();

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

app.MapGet("/accounts/{id:guid}/balance", async (Guid id, IAccountQueryService svc, CancellationToken ct) =>
{
    var result = await svc.GetBalanceAsync(id, ct);

    return ApiResults.From(result, Results.Ok);
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

app.MapGet("/transfers/{id:guid}", async (Guid id, ITransferQueryService svc, CancellationToken ct) =>
{
    var result = await svc.GetByIdAsync(id, ct);

    return ApiResults.From(result, Results.Ok);
});

app.Run();
