using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IPlatformCategoryRepository : IBaseRepository<PlatformCategory>
{
    bool CheckExistedById(long id);

    Task<IEnumerable<PlatformCategory>> GetAll();
}