using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IOptionGroupRepository : IBaseRepository<OptionGroup>
{
    bool CheckExistedByIdAndShopId(long id, long shopId);

    OptionGroup GetByIdIncludeOption(long id);

    (int TotalCount, List<OptionGroup> OptionGroups) GetAllShopOptonGroup(long? currentPrincipalId, int requestPageIndex, int requestPageSize);

    bool CheckExistTitleOptionGroup(string title, long shopId, long? id = null);

    Task<OptionGroup?> GetByIdAndOptionIds(long id, long[] optionIds);

    List<OptionGroup> GetOptionGroupsWithFoodLinkStatus(long shopId, long foodId, int filterMode);
}