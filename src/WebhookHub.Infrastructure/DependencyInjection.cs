using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebhookHub.Application.Abstractions;
using WebhookHub.Infrastructure.Persistence;

namespace WebhookHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        var connString = connectionString ?? "Host=localhost;Username=app;Password=app;Database=hub";
        services.AddDbContext<AppDb>(o => o.UseNpgsql(connString));

        services.AddScoped<IEventWriter, EventWriter>();

        return services;
    }
}
