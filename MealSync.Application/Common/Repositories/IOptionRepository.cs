using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IOptionRepository : IBaseRepository<Option>
{
    Task<Option?> GetActiveByIdAndOptionGroupId(long id, long optionGroupId);

    Task<bool> CheckAllOptionAndOptionGroupActiveByIds(List<long> ids);

    Task<Option> GetIncludeOptionGroupById(long id);
}