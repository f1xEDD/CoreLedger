using CoreLedger.Api.Dto.Transfers;
using CoreLedger.Application.Services;
using CoreLedger.Infrastructure;
using CoreLedger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<LedgerDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("LedgerDb"),
        npg => npg.MigrationsHistoryTable("__ef_migrations_history", "public"));
});

builder.Services.AddScoped<ITransferService, TransferService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/accounts", async (CreateAccountRequest req, LedgerDbContext db) =>
{
    var account = new Account(Guid.NewGuid(), Guid.NewGuid(), req.Currency);
    
    db.Accounts.Add(account);
    
    await db.SaveChangesAsync();
    
    return Results.Created($"/accounts/{account.AccountId}", new CreateAccountResponse(account.AccountId));
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

        var booking = req.BookingDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var value = req.ValueDate ?? booking;

        var id = await svc.CreateAsync(
            key,
            req.FromAccountId,
            req.ToAccountId,
            req.Amount,
            req.Currency,
            booking,
            value,
            ct);
        
        return Results.Created($"/transfers/{id}", new CreateTransferResponse(id));
    });

app.Run();