using System.Security.Claims;

namespace MealSync.Application.Common.Services;

public interface ICurrentPrincipalService
{
    public string? CurrentPrincipal { get; }

    public long? CurrentPrincipalId { get; }

    public ClaimsPrincipal GetCurrentPrincipalFromToken(string token);
}