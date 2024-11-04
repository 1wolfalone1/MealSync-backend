using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.Shops.Queries.SearchShop;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ShopRepository : BaseRepository<Shop>, IShopRepository
{
    public ShopRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Shop GetShopConfiguration(long id)
    {
        return this.DbSet.Include(so => so.Location)
            .Include(so => so.OperatingSlots)
            .Include(so => so.Location)
            .Include(so => so.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .Include(so => so.Account)
            .SingleOrDefault(so => so.Id == id);
    }

    public async Task<Shop> GetByAccountId(long id)
    {
        return await DbSet.SingleAsync(shop => shop.Id == id).ConfigureAwait(false);
    }

    public async Task<(int TotalCount, IEnumerable<Shop> Shops)> GetTopShop(long dormitoryId, int pageIndex, int pageSize)
    {
        // Query to get a paginated list of shops associated with the given dormitory ID
        // The shop must be active and not paused for receiving orders
        var query = DbSet
            .Include(shop => shop.ShopDormitories)
            .Where(
                shop => shop.ShopDormitories.Select(shopDormitory => shopDormitory.DormitoryId).Contains(dormitoryId)
                        && !shop.IsReceivingOrderPaused
                        && shop.Status == ShopStatus.Active
                        && shop.OperatingSlots.Any(os => os.IsActive)
            );
        var totalCount = await query.CountAsync().ConfigureAwait(false);
        var shops = await query
            .OrderByDescending(shop => (double)shop.TotalRating / shop.TotalReview) // High total rating / total review
            .ThenBy(shop => shop.NumOfWarning) // Low number of warnings
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync()
            .ConfigureAwait(false);

        return (totalCount, shops);
    }

    public Task<Shop?> GetByIdIncludeLocation(long id)
    {
        return DbSet.Include(s => s.Location).FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Shop?> GetShopInfoById(long id)
    {
        return await DbSet
            .Where(s => s.OperatingSlots.Any(os => os.IsActive))
            .Include(s => s.OperatingSlots.Where(os => os.IsActive))
            .Include(s => s.Location)
            .Include(s => s.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id && s.Status != ShopStatus.Deleted && s.Status != ShopStatus.UnApprove).ConfigureAwait(false);
    }

    public async Task<(int TotalCounts, List<Shop> Shops)> SearchShops(
        long dormitoryId, string? searchValue, int? platformCategoryId,
        int? startTime, int? endTime, int foodSize,
        SearchShopQuery.OrderBy? orderBy, SearchShopQuery.Direction direction,
        int pageIndex, int pageSize)
    {
        var query = DbSet
            .Where(shop =>
                shop.ShopDormitories.Any(sd => sd.DormitoryId == dormitoryId) &&
                shop.Status == ShopStatus.Active &&
                !shop.IsReceivingOrderPaused && shop.OperatingSlots.Any(os => os.IsActive)
            ).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = int.TryParse(searchValue, out var numericValue);

            query = query.Where(shop =>
                EF.Functions.Like(shop.Name, $"%{searchValue}%") ||
                EF.Functions.Like(shop.Description ?? string.Empty, $"%{searchValue}%") ||
                shop.Foods.Any(food =>
                    EF.Functions.Like(food.Name, $"%{searchValue}%") ||
                    EF.Functions.Like(food.Description ?? string.Empty, $"%{searchValue}%") ||
                    (isNumeric && EF.Functions.Like(food.Price.ToString(), $"%{numericValue.ToString()}%"))
                )
            );

            if (!orderBy.HasValue)
            {
                query = query.OrderByDescending(shop =>
                    (EF.Functions.Like(shop.Name, $"%{searchValue}%") ? 1 : 0) +
                    (EF.Functions.Like(shop.Description ?? string.Empty, $"%{searchValue}%") ? 1 : 0) +
                    shop.Foods.Count(food =>
                        EF.Functions.Like(food.Name, $"%{searchValue}%") ||
                        EF.Functions.Like(food.Description ?? string.Empty, $"%{searchValue}%")
                    )
                );
            }
        }

        if (platformCategoryId.HasValue && platformCategoryId.Value > 0)
        {
            query = query.Where(shop =>
                shop.Foods.Any(food => food.PlatformCategoryId == platformCategoryId.Value)
            );
        }

        if (startTime.HasValue && endTime.HasValue)
        {
            query = query.Where(shop =>
                shop.OperatingSlots.Any(slot => slot.StartTime <= startTime && slot.EndTime >= endTime) ||
                shop.Foods.Any(food => food.FoodOperatingSlots.Any(slot =>
                    slot.OperatingSlot.StartTime <= startTime && slot.OperatingSlot.EndTime >= endTime))
            );
        }
        else if (startTime.HasValue && !endTime.HasValue)
        {
            query = query.Where(shop =>
                shop.OperatingSlots.Any(slot => slot.StartTime <= startTime && slot.EndTime > startTime) ||
                shop.Foods.Any(food => food.FoodOperatingSlots.Any(slot =>
                    slot.OperatingSlot.StartTime <= startTime && slot.OperatingSlot.EndTime > startTime))
            );
        }
        else if (!startTime.HasValue && endTime.HasValue)
        {
            query = query.Where(shop =>
                shop.OperatingSlots.Any(slot => slot.EndTime >= endTime && slot.StartTime < endTime) ||
                shop.Foods.Any(food => food.FoodOperatingSlots.Any(slot =>
                    slot.OperatingSlot.EndTime >= endTime && slot.OperatingSlot.StartTime < endTime))
            );
        }

        if (orderBy.HasValue && orderBy == SearchShopQuery.OrderBy.Price)
        {
            query = direction == SearchShopQuery.Direction.ASC
                ? query.OrderBy(shop => shop.Foods.Min(food => food.Price))
                : query.OrderByDescending(shop => shop.Foods.Max(food => food.Price));
        }
        else if (orderBy.HasValue && orderBy == SearchShopQuery.OrderBy.Rating)
        {
            query = direction == SearchShopQuery.Direction.ASC
                ? query.OrderBy(shop => (double)shop.TotalRating / shop.TotalReview)
                : query.OrderByDescending(shop => (double)shop.TotalRating / shop.TotalReview);
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        var shops = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(shop => new Shop
            {
                Id = shop.Id,
                Name = shop.Name,
                LogoUrl = shop.LogoUrl,
                BannerUrl = shop.BannerUrl,
                PhoneNumber = shop.PhoneNumber,
                IsAcceptingOrderNextDay = shop.IsAcceptingOrderNextDay,
                Foods = shop.Foods
                    .Where(food => food.Status == FoodStatus.Active &&
                                   (!platformCategoryId.HasValue || platformCategoryId.Value == 0 || food.PlatformCategoryId == platformCategoryId.Value) &&
                                   (
                                       (!startTime.HasValue && !endTime.HasValue) ||
                                       (startTime.HasValue && !endTime.HasValue &&
                                        food.FoodOperatingSlots.Any(slot => slot.OperatingSlot.StartTime <= startTime && slot.OperatingSlot.EndTime > startTime)) ||
                                       (!startTime.HasValue && endTime.HasValue &&
                                        food.FoodOperatingSlots.Any(slot => slot.OperatingSlot.EndTime >= endTime && slot.OperatingSlot.StartTime < endTime)) ||
                                       (startTime.HasValue && endTime.HasValue &&
                                        food.FoodOperatingSlots.Any(slot =>
                                            slot.OperatingSlot.StartTime < endTime && slot.OperatingSlot.EndTime > startTime))
                                   ))
                    .OrderBy(food =>
                            orderBy.HasValue && orderBy == SearchShopQuery.OrderBy.Price
                                ? direction == SearchShopQuery.Direction.ASC ? food.Price : -food.Price
                                : EF.Functions.Like(food.Name, $"%{searchValue}%")
                                    ? 0 // Primary sort by match
                                    : EF.Functions.Like(food.Description ?? string.Empty, $"%{searchValue}%")
                                        ? 1 // Secondary sort by match
                                        : 2 // Default sorting value
                    )
                    .ThenBy(food => food.Name) // Sort by name as a fallback
                    .Take(foodSize)
                    .Select(food => new Food
                    {
                        Id = food.Id,
                        ShopId = food.ShopId,
                        Name = food.Name,
                        Description = food.Description,
                        Price = food.Price,
                        ImageUrl = food.ImageUrl,
                        IsSoldOut = food.IsSoldOut,
                    })
                    .ToList(),
            })
            .ToListAsync()
            .ConfigureAwait(false);

        return (totalCount, shops);
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}