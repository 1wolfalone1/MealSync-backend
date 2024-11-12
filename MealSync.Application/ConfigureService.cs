using FluentValidation;
using FluentValidation.AspNetCore;
using MealSync.Application.Behaviors;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Mappings;
using MealSync.Application.Shared;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealSync.Application;

public static class ConfigureService
{
    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        // Add Error Config
        var resourceRepository = services.BuildServiceProvider().GetService<ISystemResourceRepository>();
        Error.Configure(resourceRepository);

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

        return services;
    }
}