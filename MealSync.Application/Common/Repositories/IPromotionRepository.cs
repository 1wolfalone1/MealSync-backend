using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IPromotionRepository : IBaseRepository<Promotion>
{
    Task<IEnumerable<Promotion>> GetShopAvailablePromotionsByShopId(long id);

    Task<Promotion?> GetByIdAndShopId(long id, long shopId);

    Task<(IEnumerable<Promotion> EligibleList, IEnumerable<Promotion> IneligibleList)> GetShopAvailablePromotionsByShopIdAndTotalPrice(long shopId, double totalPrice);

    Task<(int TotalCount, IEnumerable<Promotion> Promotions)> GetShopPromotionByFilter(
        long shopId, string? searchValue, PromotionStatus? status, PromotionApplyTypes? applyTypes,
        DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize);
}