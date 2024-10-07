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
        return await DbSet.Where(
            p => p.ShopId == id
                 && p.Status == PromotionStatus.Active
                 && p.NumberOfUsed < p.UsageLimit
                 && p.EndDate >= DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7)))
            .ToListAsync().ConfigureAwait(false);
    }
}