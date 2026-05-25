using InvenTrack.Application.Interfaces;
using InvenTrack.Infrastructure.BackgroundJobs;
using InvenTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvenTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<ImportBackgroundService>();
        services.AddHostedService<DailySummaryService>();

        services.AddMemoryCache();

        return services;
    }
}
