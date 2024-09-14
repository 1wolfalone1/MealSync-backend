using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using MealSync.Domain.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MealSync.Infrastructure.Settings;

namespace MealSync.API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration config)
    {
        //config authen
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = config["TOKEN_ISSUER"],
                ValidAudience = config["TOKEN_AUDIENCE"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TOKEN_KEY"])),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };
            
            // Customize the response for unauthorized requests
            x.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    // Skip the default logic
                    context.HandleResponse();

                    // Set custom response
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(
                        Result.Failure(new Error("401", "Authentication failed: JWT token không hợp lệ", true)),
                        new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    return context.Response.WriteAsync(result);
                },
                OnForbidden = context =>
                {
                    // Set custom response
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(
                        Result.Failure(new Error("403", "Authorization failed: Bạn không có quyền truy cập", true)),
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    return context.Response.WriteAsync(result);
                }
            };
        });
        
        // Add authorization
        services.AddAuthorization();
        
        //Set Jwt Setting
        var jwtSetting = new JwtSetting
        {
            Key = config["TOKEN_KEY"],
            Audience = config["TOKEN_AUDIENCE"],
            Issuer = config["TOKEN_ISSUER"],
            TokenExpire = int.Parse(config["TOKEN_TIME_EXPIRED_IN_HOURS"])
        };

        // Validate the JwtSettings instance using DataAnnotations
        var validationContext = new ValidationContext(jwtSetting);
        Validator.ValidateObject(jwtSetting, validationContext, validateAllProperties: true);

        // Register the JwtSettings instance as a singleton
        services.AddSingleton<JwtSetting>(jwtSetting);
        return services;
    }
}