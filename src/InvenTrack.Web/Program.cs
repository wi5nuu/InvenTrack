using InvenTrack.Application;
using InvenTrack.Infrastructure;
using InvenTrack.Infrastructure.Data;
using InvenTrack.Infrastructure.Data.Seed;
using InvenTrack.Web.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

/// <summary>
/// InvenTrack Enterprise Web UI — entry point.
/// Configures Cookie auth, MVC pipeline, Serilog logging,
/// and auto-seeds the database on first run.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/inventrack-web-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllersWithViews();

// Cookie auth for Web UI
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(context);
}

app.Run();
