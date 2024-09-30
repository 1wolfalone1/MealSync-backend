using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IPlatformCategoryRepository : IBaseRepository<PlatformCategory>
{
    Task<bool> CheckExistedByIds(List<long> ids);
}