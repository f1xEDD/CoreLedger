using CoreLedger.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<LedgerDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("LedgerDb"),
        npg => npg.MigrationsHistoryTable("__ef_migrations_history", "public"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "CoreLedger API is running");

app.Run();