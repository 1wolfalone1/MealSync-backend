using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IOptionGroupRepository : IBaseRepository<OptionGroup>
{
    bool CheckExistedByIdAndShopId(long id, long shopId);
}