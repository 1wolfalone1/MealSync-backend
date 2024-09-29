using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Exceptions;
using MealSync.Application.Common.Services;
using MealSync.Infrastructure.Settings;

namespace MealSync.Infrastructure.Services;

public class CurrentPrincipalService : ICurrentPrincipalService, IBaseService
{
    private readonly IHttpContextAccessor _accessor;
    private readonly JwtSetting _jwtSetting;

    public CurrentPrincipalService(IHttpContextAccessor accessor, JwtSetting jwtSetting)
    {
        _accessor = accessor;
        _jwtSetting = jwtSetting;
    }

    // Get current login acc email
    public string? CurrentPrincipal
    {
        get
        {
            var identity = _accessor?.HttpContext?.User.Identity as ClaimsIdentity;

            if (identity == null || !identity.IsAuthenticated) return null;

            var claims = identity.Claims;

            var email = claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value ?? null;

            return email;
        }
    }

    public long? CurrentPrincipalId
    {
        get
        {
            var identity = _accessor?.HttpContext?.User.Identity as ClaimsIdentity;

            if (identity == null || !identity.IsAuthenticated) return null;

            var claims = identity.Claims;

            var id = claims.FirstOrDefault(o => o.Type == ClaimTypes.Sid)?.Value ?? null;

            return long.Parse(id);
        }
    }

    public ClaimsPrincipal GetCurrentPrincipalFromToken(string token)
    {
        var tokenValidationParams = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSetting.Issuer,
            ValidAudience = _jwtSetting.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out var securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ApiException(ResponseCode.AuthErrorInvalidRefreshToken);
        }

        return principal;
    }
}