using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services;

public interface ICurrentAccountService
{
    public Account GetCurrentAccount();
}