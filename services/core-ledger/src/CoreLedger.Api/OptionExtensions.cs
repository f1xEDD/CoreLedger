using CoreLedger.Infrastructure.Options;

namespace CoreLedger.Api;

public static class OptionExtensions
{
    public static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(
            configuration.GetSection("Outbox"));

        return services;
    }
}