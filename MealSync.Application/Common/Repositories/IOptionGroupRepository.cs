using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IOptionGroupRepository : IBaseRepository<OptionGroup>
{
    bool CheckExistedByIdAndShopId(long id, long shopId);

    OptionGroup GetByIdIncludeOption(long id);

    (int TotalCount, List<OptionGroup> OptionGroups) GetAllShopOptonGroup(long? currentPrincipalId, string? title, int optionGroupStatus, int requestPageIndex, int requestPageSize);

    bool CheckExistTitleOptionGroup(string title, long shopId, long? id = null);

    Task<OptionGroup?> GetByIdAndOptionIds(long id, long[] optionIds);
}