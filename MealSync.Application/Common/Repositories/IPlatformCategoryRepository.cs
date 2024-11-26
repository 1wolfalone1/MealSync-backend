using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IPlatformCategoryRepository : IBaseRepository<PlatformCategory>
{
    bool CheckExistedById(long id);

    Task<IEnumerable<PlatformCategory>> GetAll();

    bool CheckExsitName(string name);

    int GetMaxDisplayOrder();

    bool CheckExsitUpdateName(string requestName, long requestId);

    List<PlatformCategory> GetByIds(long[] ids);
}