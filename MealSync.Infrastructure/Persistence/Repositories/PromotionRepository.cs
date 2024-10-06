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
        // Get UTC+7 timezone
        var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        // Convert current time to UTC+7
        var currentTimeInUtcPlus7 = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, utcPlus7);

        return await DbSet.Where(
            p => p.ShopId == id
                 && p.Status == PromotionStatus.Active
                 && p.NumberOfUsed < p.UsageLimit
                 && p.EndDate >= currentTimeInUtcPlus7)
            .ToListAsync();
    }
}