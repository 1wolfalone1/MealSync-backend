using FluentValidation.AspNetCore;
using FluentValidation;
using MealSync.Application.Mappings;
using System.Data;
using MySql.Data.MySqlClient;
using MediatR;
using MealSync.Application.Behaviors;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Infrastructure.Persistence.Repositories;
using MealSync.Infrastructure.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Infrastructure.Services.Dapper;
using System.Text.Json.Serialization;
using MealSync.Infrastructure.Common.Data.ApplicationInitialData;

namespace MealSync.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        //Add memory cache
        services.AddMemoryCache();

        //Dapper
        services.AddScoped<IDbConnection>((sp) => new MySqlConnection(config["DATABASE_URL"]));

        //Allow origin
        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", poli =>
            {
                poli.AllowAnyMethod().AllowAnyHeader().WithOrigins(config["ALLOW_ORIGIN"]);

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
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        //Config service
        var assembly = typeof(BaseService).Assembly;
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(BaseService))
            .AddClasses(classes => classes.AssignableTo(typeof(IBaseService)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
            .FromAssembliesOf(typeof(AccountRepository)) // Infrastructure Layer
            .AddClasses(classes => classes.AssignableTo(typeof(IBaseRepository<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        //Add unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDapperService, DapperService>();

        return services;
    }
    
    private static bool IsDevelopment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment == Environments.Development;
    }
}
