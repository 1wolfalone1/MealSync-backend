using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Application.UseCases.Shops.Queries.ModeratorManage.GetListShop;
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

    public async Task<Shop?> GetShopInfoByIdForCustomer(long id)
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
                Description = shop.Description,
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

    public Task<Shop> GetShopInfoByIdForShop(long id)
    {
        return DbSet
            .Include(s => s.OperatingSlots)
            .Include(s => s.Location)
            .Include(s => s.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .FirstAsync(s => s.Id == id);
    }

    public Task<Shop> GetShopInfoForReOrderById(long id)
    {
        return DbSet
            .Include(s => s.OperatingSlots.Where(os => os.IsActive))
            .Include(s => s.ShopDormitories).ThenInclude(sd => sd.Dormitory)
            .Include(s => s.Location)
            .FirstAsync(s => s.Id == id);
    }

    public Task<List<Shop>> GetAllShopReceivingOrderPaused()
    {
        return DbSet.Where(s => s.IsReceivingOrderPaused).ToListAsync();
    }

    public async Task<(List<ShopManageDto> Shops, int TotalCount)> GetAllShopByDormitoryIds(
        List<long> dormitoryIds, string? searchValue, DateTime? dateFrom, DateTime? dateTo,
        ShopStatus? status, long? dormitoryId, GetListShopQuery.FilterShopOrderBy? orderBy,
        GetListShopQuery.FilterShopDirection? direction, int pageIndex, int pageSize)
    {
        var query = DbSet
            .Where(s => s.Account.Status != AccountStatus.UnVerify
                        && s.Account.Status != AccountStatus.Deleted
                        && s.Status != ShopStatus.Deleted && s.ShopDormitories.Any(sd => dormitoryIds.Contains(sd.DormitoryId)))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = int.TryParse(searchValue, out var numericValue);

            query = query.Where(shop =>
                EF.Functions.Like(shop.Name, $"%{searchValue}%") ||
                EF.Functions.Like(shop.Description ?? string.Empty, $"%{searchValue}%") ||
                EF.Functions.Like(shop.Account.FullName ?? string.Empty, $"%{searchValue}%") ||
                (isNumeric && EF.Functions.Like(shop.Id.ToString(), $"%{numericValue.ToString()}%"))
            );
        }

        if (status.HasValue && status.Value != ShopStatus.Deleted)
        {
            query = query.Where(shop => shop.Status == status.Value);
        }

        if (dormitoryId.HasValue && dormitoryId > 0)
        {
            query = query.Where(shop => shop.ShopDormitories.Any(cb => cb.DormitoryId == dormitoryId));
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(shop => shop.CreatedDate >= dateFrom.Value && shop.CreatedDate <= dateTo.Value);
        }
        else if (dateFrom.HasValue && !dateTo.HasValue)
        {
            query = query.Where(shop => shop.CreatedDate >= dateFrom.Value);
        }
        else if (!dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(shop => shop.CreatedDate <= dateTo.Value);
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        var revenueQuery = query.Select(shop => new ShopManageDto
        {
            Id = shop.Id,
            ShopName = shop.Name,
            ShopOwnerName = shop.Account.FullName ?? string.Empty,
            LogoUrl = shop.LogoUrl,
            Status = shop.Status,
            TotalFood = shop.TotalFood,
            TotalOrder = shop.TotalOrder,
            TotalOrderInProcess = shop.Orders.Count(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Delivering),
            CreatedDate = shop.CreatedDate,
            TotalRevenue = shop.Orders
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Resolved)
                .Sum(o =>
                    o.Status == OrderStatus.Completed && o.ReasonIdentity == null
                        ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                        : o.Status == OrderStatus.Completed &&
                          o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription() &&
                          o.Payments.Any(p => p.Type == PaymentTypes.Payment && p.PaymentMethods == PaymentMethods.VnPay)
                            ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                            : o.Status == OrderStatus.Resolved &&
                              o.IsReport &&
                              o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription()
                                ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                                : o.Status == OrderStatus.Resolved &&
                                  o.IsReport &&
                                  o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER.GetDescription() &&
                                  !o.IsRefund &&
                                  o.Payments.Any(p =>
                                      p.PaymentMethods == PaymentMethods.VnPay &&
                                      p.Type == PaymentTypes.Payment &&
                                      p.Status == PaymentStatus.PaidSuccess)
                                    ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                                    : 0),
        });

        if (orderBy.HasValue && direction.HasValue)
        {
            if (orderBy.Value == GetListShopQuery.FilterShopOrderBy.CreatedDate)
            {
                revenueQuery = direction == GetListShopQuery.FilterShopDirection.ASC
                    ? revenueQuery.OrderBy(shop => shop.CreatedDate)
                    : revenueQuery.OrderByDescending(shop => shop.CreatedDate);
            }
            else if (orderBy.Value == GetListShopQuery.FilterShopOrderBy.ShopName)
            {
                revenueQuery = direction == GetListShopQuery.FilterShopDirection.ASC
                    ? revenueQuery.OrderBy(shop => shop.ShopName)
                    : revenueQuery.OrderByDescending(shop => shop.ShopName);
            }
            else if (orderBy.Value == GetListShopQuery.FilterShopOrderBy.ShopOwnerName)
            {
                revenueQuery = direction == GetListShopQuery.FilterShopDirection.ASC
                    ? revenueQuery.OrderBy(shop => shop.ShopOwnerName)
                    : revenueQuery.OrderByDescending(shop => shop.ShopOwnerName);
            }
            else if (orderBy.Value == GetListShopQuery.FilterShopOrderBy.Revenue)
            {
                revenueQuery = direction == GetListShopQuery.FilterShopDirection.ASC
                    ? revenueQuery.OrderBy(r => r.TotalRevenue)
                    : revenueQuery.OrderByDescending(r => r.TotalRevenue);
            }
            else if (orderBy.Value == GetListShopQuery.FilterShopOrderBy.TotalOrder)
            {
                revenueQuery = direction == GetListShopQuery.FilterShopDirection.ASC
                    ? revenueQuery.OrderBy(shop => shop.TotalOrder)
                    : revenueQuery.OrderByDescending(shop => shop.TotalOrder);
            }
            else if (orderBy.Value == GetListShopQuery.FilterShopOrderBy.TotalFood)
            {
                revenueQuery = direction == GetListShopQuery.FilterShopDirection.ASC
                    ? revenueQuery.OrderBy(shop => shop.TotalFood)
                    : revenueQuery.OrderByDescending(shop => shop.TotalFood);
            }
        }
        else
        {
            revenueQuery = revenueQuery.OrderByDescending(shop => shop.CreatedDate);
        }

        var shops = revenueQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (shops, totalCount);
    }

    public Task<Shop?> GetShopManageDetail(long shopId, List<long> dormitoriesIdMod)
    {
        return DbSet
            .Include(s => s.Account)
            .Include(s => s.Location)
            .Include(s => s.ShopDormitories)
            .ThenInclude(sd => sd.Dormitory)
            .Include(s => s.OperatingSlots)
            .Where(s => s.Id == shopId
                        && s.Account.Status != AccountStatus.UnVerify
                        && s.Account.Status != AccountStatus.Deleted
                        && s.Status != ShopStatus.Deleted
                        && s.ShopDormitories.Any(sd => dormitoriesIdMod.Contains(sd.DormitoryId)))
            .FirstOrDefaultAsync();
    }

    public Task<Shop?> GetShopManage(long shopId, List<long> dormitoriesIdMod)
    {
        return DbSet
            .Include(s => s.Account)
            .Where(s => s.Id == shopId
                        && s.Account.Status != AccountStatus.UnVerify
                        && s.Account.Status != AccountStatus.Deleted
                        && s.Status != ShopStatus.Deleted
                        && s.ShopDormitories.Any(sd => dormitoriesIdMod.Contains(sd.DormitoryId)))
            .FirstOrDefaultAsync();
    }

    public Task<double> GetShopRevenue(long shopId)
    {
        return DbSet
            .Where(s => s.Id == shopId)
            .SelectMany(s => s.Orders)
            .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Resolved)
            .SumAsync(o =>
                o.Status == OrderStatus.Completed && o.ReasonIdentity == null
                    ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                    : o.Status == OrderStatus.Completed &&
                      o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription() &&
                      o.Payments.Any(p => p.Type == PaymentTypes.Payment && p.PaymentMethods == PaymentMethods.VnPay)
                        ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                        : o.Status == OrderStatus.Resolved &&
                          o.IsReport &&
                          o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription()
                            ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                            : o.Status == OrderStatus.Resolved &&
                              o.IsReport &&
                              o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER.GetDescription() &&
                              !o.IsRefund &&
                              o.Payments.Any(p =>
                                  p.PaymentMethods == PaymentMethods.VnPay &&
                                  p.Type == PaymentTypes.Payment &&
                                  p.Status == PaymentStatus.PaidSuccess)
                                ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                                : 0);
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}