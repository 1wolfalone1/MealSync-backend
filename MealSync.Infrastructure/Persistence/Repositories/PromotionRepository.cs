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

    public async Task<(int TotalCount, IEnumerable<Promotion> Promotions)> GetShopPromotionByFilter(
        long shopId, string? searchValue, PromotionStatus? status, PromotionApplyTypes? applyTypes,
        DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize
    )
    {
        var query = DbSet.Where(p => p.ShopId == shopId && p.Type == PromotionTypes.ShopPromotion && p.Status != PromotionStatus.Delete).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (applyTypes.HasValue)
        {
            query = query.Where(p => p.ApplyType == applyTypes.Value);
        }

        if (startTime.HasValue)
        {
            query = query.Where(p => p.StartDate >= new DateTimeOffset(startTime.Value, TimeSpan.Zero));
        }

        if (endTime.HasValue)
        {
            query = query.Where(p => p.EndDate <= new DateTimeOffset(endTime.Value, TimeSpan.Zero));
        }

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = double.TryParse(searchValue, out var numericValue);
            string numericString = numericValue.ToString();

            query = query.Where(p =>
                EF.Functions.Like(p.Title, $"%{searchValue}%") ||
                EF.Functions.Like(p.Description ?? string.Empty, $"%{searchValue}%") ||
                (isNumeric && applyTypes == PromotionApplyTypes.Percent &&
                 (EF.Functions.Like(p.Id.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.AmountRate.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.MaximumApplyValue.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.MinOrdervalue.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.UsageLimit.ToString(), $"%{numericString}%"))) ||
                (isNumeric && applyTypes == PromotionApplyTypes.Absolute &&
                 (EF.Functions.Like(p.Id.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.AmountValue.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.MinOrdervalue.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.UsageLimit.ToString(), $"%{numericString}%"))) ||
                (isNumeric && applyTypes == null &&
                 (EF.Functions.Like(p.Id.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.AmountRate.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.MaximumApplyValue.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.AmountValue.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.MinOrdervalue.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(p.UsageLimit.ToString(), $"%{numericString}%")))
            );
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);
        query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        var promotions = await query.ToListAsync().ConfigureAwait(false);

        return (totalCount, promotions);
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}