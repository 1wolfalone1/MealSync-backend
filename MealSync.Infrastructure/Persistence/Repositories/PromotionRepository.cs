using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class PromotionRepository : BaseRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<Promotion>> GetShopAvailablePromotionsByShopId(long id)
    {
        var now = DateTimeOffset.UtcNow;

        return await DbSet.Where(
                p => p.ShopId == id
                     && p.Status == PromotionStatus.Active
                     && p.Type == PromotionTypes.ShopPromotion
                     && p.NumberOfUsed < p.UsageLimit
                     && p.EndDate >= now
                     && p.StartDate <= now)
            .ToListAsync().ConfigureAwait(false);
    }

    public Task<Promotion?> GetByIdAndShopId(long id, long shopId)
    {
        return DbSet.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shopId && p.Type == PromotionTypes.ShopPromotion);
    }

    public async Task<(IEnumerable<Promotion> EligibleList, IEnumerable<Promotion> IneligibleList)>
        GetShopAvailablePromotionsByShopIdAndTotalPrice(long shopId, double totalPrice)
    {
        var now = DateTimeOffset.UtcNow;

        var promotions = await DbSet.Where(p =>
            p.ShopId == shopId &&
            p.Status == PromotionStatus.Active &&
            p.Type == PromotionTypes.ShopPromotion &&
            p.EndDate >= now &&
            p.StartDate <= now
        ).ToListAsync().ConfigureAwait(false);

        // Split promotions into eligible and ineligible
        var eligibleList = promotions.Where(p =>
            p.NumberOfUsed < p.UsageLimit &&
            p.MinOrdervalue <= totalPrice
        ).ToList();

        var ineligibleList = promotions.Except(eligibleList).ToList();

        return (eligibleList, ineligibleList);
    }
}