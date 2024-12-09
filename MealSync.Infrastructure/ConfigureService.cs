using System.ComponentModel.DataAnnotations;
using System.Data;
using Amazon;
using Amazon.Rekognition;
using Amazon.Runtime;
using Amazon.S3;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Infrastructure.Common.Data.ApplicationInitialData;
using MealSync.Infrastructure.Persistence.Contexts;
using MealSync.Infrastructure.Persistence.Interceptors;
using MealSync.Infrastructure.Persistence.Repositories;
using MealSync.Infrastructure.Services;
using MealSync.Infrastructure.Services.Dapper;
using MealSync.Infrastructure.Services.Notifications;
using MealSync.Infrastructure.Services.Notifications.Kafka;
using MealSync.Infrastructure.Services.Notifications.Kafka.Consumers;
using MealSync.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using StackExchange.Redis;

namespace MealSync.Infrastructure;

public static class ConfigureService
{
    public static IServiceCollection ConfigureInfrastuctureServices(this IServiceCollection services,
        IConfiguration config)
    {
        //Set Jwt Setting
        var jwtSetting = new JwtSetting
        {
            Key = config["TOKEN_KEY"],
            Audience = config["TOKEN_AUDIENCE"],
            Issuer = config["TOKEN_ISSUER"],
            TokenExpire = int.Parse(config["TOKEN_TIME_EXPIRED_IN_HOURS"]),
            RefreshTokenExpire = int.Parse(config["REFRESH_TOKEN_TIME_EXPIRED_IN_HOURS"])
        };

        // Set Firebase Setting
        FirebaseConfigLoader.LoadFirebaseCredentials(config["GOOGLE_APPLICATION_CREDENTIALS"]);

        // Validate the JwtSettings instance using DataAnnotations
        var validationContext = new ValidationContext(jwtSetting);
        Validator.ValidateObject(jwtSetting, validationContext, validateAllProperties: true);

        // Register the JwtSettings instance as a singleton
        services.AddSingleton<JwtSetting>(jwtSetting);

        // Set Redis Setting
        var redidSetting = new RedisSetting()
        {
            ConnectionString = config["REDIS_URL"]
        };

        // Validate the RedisSettings instance using DataAnnotations
        var validationRedisContext = new ValidationContext(redidSetting);
        Validator.ValidateObject(redidSetting, validationRedisContext, validateAllProperties: true);

        // Register the RedisSettings instance as a singleton
        services.AddSingleton<RedisSetting>(redidSetting);

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redidSetting.ConnectionString));
        services.AddStackExchangeRedisCache(option => option.Configuration = redidSetting.ConnectionString);

        // Set VnPay Setting
        var vnPaySetting = new VnPaySetting
        {
            PaymentUrl = config["VN_PAY_PAYMENT_URL"] ?? string.Empty,
            ReturnUrl = config["VN_PAY_RETURN_URL"] ?? string.Empty,
            RefundUrl = config["VN_PAY_REFUND_URL"] ?? string.Empty,
            TmpCode = config["VN_PAY_TMP_CODE"] ?? string.Empty,
            HashSecret = config["VN_PAY_HASH_SECRET"] ?? string.Empty,
        };

        // Validate the VnPaySettings instance using DataAnnotations
        var validationVnPayContext = new ValidationContext(vnPaySetting);
        Validator.ValidateObject(vnPaySetting, validationVnPayContext, validateAllProperties: true);

        // Register the RedisSettings instance as a singleton
        services.AddSingleton<VnPaySetting>(vnPaySetting);

        services.AddSingleton<IAmazonRekognition>(sp =>
        {
            return new AmazonRekognitionClient(
                config["AWS_ACCESS_KEY"],
                config["AWS_SECRET_KEY"],
                RegionEndpoint.GetBySystemName(config["AWS_REGION"])
            );
        });

        // S3
        services.AddSingleton<IAmazonS3>(sp =>
        {
            return new AmazonS3Client(
                new BasicAWSCredentials(
                    config["AWS_ACCESS_KEY"] ?? string.Empty,
                    config["AWS_SECRET_KEY"] ?? string.Empty
                ),
                new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(config["AWS_REGION"] ?? string.Empty),
                }
            );
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
        services.AddScoped<CustomSaveChangesInterceptor>();
        services.AddDbContext<MealSyncContext>(options =>
        {
            options.UseMySql(config["DATABASE_URL"], ServerVersion.Parse("8.0.33-mysql"))
                .UseSnakeCaseNamingConvention();
        });

        // Add notification service
        services.AddSingleton<IMobileNotificationService, FirebaseNotificationService>();
        services.AddSingleton<IWebNotificationService, KafkaPushNotificationService>();
        services.AddSingleton<INotificationProvider, NotificationProvider>();
        services.AddSingleton<INotifierService, NotifierService>();
        services.AddSingleton<INotificationFactory, NotificationFactory>();

        //Add memory cache
        services.AddMemoryCache();

        //Dapper
        services.AddScoped<IDbConnection>((sp) => new MySqlConnection(config["DATABASE_URL"]));

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