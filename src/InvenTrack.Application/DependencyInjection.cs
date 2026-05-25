using FluentValidation;
using FluentValidation.AspNetCore;
using InvenTrack.Application.Interfaces;
using InvenTrack.Application.Mapping;
using InvenTrack.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvenTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddFluentValidationAutoValidation();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddSingleton<IEmailService, EmailService>();

        return services;
    }
}
