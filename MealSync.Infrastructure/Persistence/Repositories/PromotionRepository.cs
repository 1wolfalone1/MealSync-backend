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
        var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));

        return await DbSet.Where(
            p => p.ShopId == id
                 && p.Status == PromotionStatus.Active
                 && p.NumberOfUsed < p.UsageLimit
                 && p.EndDate >= now)
            .ToListAsync().ConfigureAwait(false);
    }

    public Task<Promotion?> GetByIdAndShopId(long id, long shopId)
    {
        return DbSet.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shopId && p.Type == PromotionTypes.ShopPromotion);
    }
}