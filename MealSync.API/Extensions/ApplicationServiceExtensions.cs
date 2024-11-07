using FluentValidation.AspNetCore;
using FluentValidation;
using MealSync.Application.Mappings;
using System.Data;
using MediatR;
using MealSync.Application.Behaviors;
using System.Text.Json.Serialization;
using MealSync.API.Converters;
using MealSync.Domain.Enums;
using MySqlConnector;

namespace MealSync.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration config)
    {
        // Register
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();

        //Add memory cache
        services.AddMemoryCache();

        //Dapper
        services.AddScoped<IDbConnection>((sp) => new MySqlConnection(config["DATABASE_URL"]));

        //Allow origin
        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", poli =>
            {
                poli.WithOrigins(config["ALLOW_ORIGIN"].Split(",")).AllowAnyMethod().AllowAnyHeader();

            });
        });

        // MediaR
        var applicationAssembly = typeof(Application.AssemblyReference).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Auto mapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentAPI validation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Fix disable 400 request filter auto
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

        // Config Json Convert
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new DateTimeOffsetConverter());
            options.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonNumericEnumConverter());
        });

        services.AddHttpContextAccessor();

        return services;
    }

    private static bool IsDevelopment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment == Environments.Development;
    }
}