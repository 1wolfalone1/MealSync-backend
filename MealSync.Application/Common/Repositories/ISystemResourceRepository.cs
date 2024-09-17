using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface ISystemResourceRepository : IBaseRepository<SystemResource>
{
    string? GetByResourceCode(string code);
    string GetByResourceCode(string code, params object[] args);
}