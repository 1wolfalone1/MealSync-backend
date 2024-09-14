using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services;

public interface IJwtTokenService
{
    string GenerateJwtToken(Account account);
}