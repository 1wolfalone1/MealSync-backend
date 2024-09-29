using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<bool> CheckExistedByIds(List<long> ids);
}