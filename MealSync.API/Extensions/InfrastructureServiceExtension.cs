using System.Text.Json.Serialization;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Infrastructure.Common.Data.ApplicationInitialData;
using MealSync.Infrastructure.Persistence.Repositories;
using MealSync.Infrastructure.Services;
using MealSync.Infrastructure.Services.Dapper;

namespace MealSync.API.Extensions;

public static class InfrastructureServiceExtension
{
    public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Initial data
        services.AddScoped<ApplicationDbInitializer>();
        using var scope = services.BuildServiceProvider().CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>();
        if (IsDevelopment())
        {
            initializer.SeedAsync().Wait();
        }
        return services;
    }
    
    private static bool IsDevelopment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment == Environments.Development;
    }
}