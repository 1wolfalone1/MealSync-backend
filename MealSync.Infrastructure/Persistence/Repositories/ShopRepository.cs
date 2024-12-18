using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.ShopOwners.Models;
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
            .Include(shop => shop.Location)
            .Where(
                shop => shop.ShopDormitories.Select(shopDormitory => shopDormitory.DormitoryId).Contains(dormitoryId)
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
                shop.Status == ShopStatus.Active && shop.OperatingSlots.Any(os => os.IsActive)
            ).AsQueryable();

        if (platformCategoryId.HasValue && platformCategoryId.Value > 0)
        {
            query = query.Where(shop =>
                shop.Foods.Any(food => food.PlatformCategoryId == platformCategoryId.Value)
            );
        }

        if (startTime.HasValue && endTime.HasValue)
        {
            query = query.Where(shop =>
                shop.OperatingSlots.Any(slot => slot.IsActive && slot.StartTime < endTime && slot.EndTime > startTime) ||
                shop.Foods.Any(food =>
                    food.FoodOperatingSlots.Any(slot =>
                        slot.OperatingSlot.IsActive && slot.OperatingSlot.StartTime < endTime && slot.OperatingSlot.EndTime > startTime))
            );
        }
        else if (startTime.HasValue && !endTime.HasValue)
        {
            query = query.Where(shop =>
                shop.OperatingSlots.Any(slot => slot.IsActive && slot.EndTime > startTime) ||
                shop.Foods.Any(food =>
                    food.FoodOperatingSlots.Any(slot =>
                        slot.OperatingSlot.IsActive && slot.OperatingSlot.EndTime > startTime))
            );
        }
        else if (!startTime.HasValue && endTime.HasValue)
        {
            query = query.Where(shop =>
                shop.OperatingSlots.Any(slot => slot.IsActive && slot.StartTime < endTime) ||
                shop.Foods.Any(food =>
                    food.FoodOperatingSlots.Any(slot =>
                        slot.OperatingSlot.IsActive && slot.OperatingSlot.StartTime < endTime))
            );
        }

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
                TotalRating = shop.TotalRating,
                TotalReview = shop.TotalReview,
                TotalOrder = shop.TotalOrder,
                Foods = shop.Foods
                    .Where(food => food.Status == FoodStatus.Active &&
                                   (!platformCategoryId.HasValue || platformCategoryId.Value == 0 || food.PlatformCategoryId == platformCategoryId.Value) &&
                                   (string.IsNullOrEmpty(searchValue) || food.Name.Contains(searchValue) || (food.Description != default && food.Description.Contains(searchValue))) &&
                                   (
                                       (!startTime.HasValue && !endTime.HasValue) ||
                                       (startTime.HasValue && !endTime.HasValue &&
                                        food.FoodOperatingSlots.Any(slot => slot.OperatingSlot.IsActive && slot.OperatingSlot.EndTime > startTime)) ||
                                       (!startTime.HasValue && endTime.HasValue &&
                                        food.FoodOperatingSlots.Any(slot => slot.OperatingSlot.IsActive && slot.OperatingSlot.StartTime < endTime)) ||
                                       (startTime.HasValue && endTime.HasValue &&
                                        food.FoodOperatingSlots.Any(slot =>
                                            slot.OperatingSlot.IsActive && slot.OperatingSlot.StartTime < endTime && slot.OperatingSlot.EndTime > startTime))
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
                        TotalOrder = food.TotalOrder,
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
                                  (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription()
                                   || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription()) &&
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

    public async Task<List<ShopRevenueDto>> GetShopRevenueEachMonthById(long id)
    {
        var year = TimeFrameUtils.GetCurrentDateInUTC7().Year;
        var allMonths = Enumerable.Range(1, 12)
            .Select(month => new ShopRevenueDto
            {
                Month = month,
                Revenue = 0,
            }).ToList();

        var monthlyRevenue = await DbSet
            .Where(s => s.Id == id)
            .SelectMany(s => s.Orders
                .Where(o => (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Resolved) && o.IntendedReceiveDate.Year == year)
            )
            .GroupBy(o => new
            {
                Month = o.IntendedReceiveDate.Month,
            })
            .Select(g => new ShopRevenueDto
            {
                Month = g.Key.Month,
                Revenue = g.Sum(o =>
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
                                  (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription()
                                   || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription()) &&
                                  !o.IsRefund &&
                                  o.Payments.Any(p =>
                                      p.PaymentMethods == PaymentMethods.VnPay &&
                                      p.Type == PaymentTypes.Payment &&
                                      p.Status == PaymentStatus.PaidSuccess)
                                    ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                                    : 0),
            })
            .ToListAsync().ConfigureAwait(false);

        var finalResult = allMonths
            .GroupJoin(
                monthlyRevenue,
                defaultMonth => defaultMonth.Month,
                revenueMonth => revenueMonth.Month,
                (defaultMonth, revenueGroup) => new ShopRevenueDto
                {
                    Month = defaultMonth.Month,
                    Revenue = revenueGroup.Sum(r => r.Revenue),
                })
            .OrderBy(dto => dto.Month)
            .ToList();

        return finalResult;
    }

    public async Task<List<ShopOrderStatisticDto>> GetShopOrderStatistic(long id)
    {
        var year = TimeFrameUtils.GetCurrentDateInUTC7().Year;
        var allMonths = Enumerable.Range(1, 12)
            .Select(month => new ShopOrderStatisticDto
            {
                Month = month,
                OrderStatisticDetail = new ShopOrderStatisticDto.OrderStatisticDetailDto(),
            }).ToList();

        var monthlyOrderStatistic = await DbSet
            .Where(s => s.Id == id)
            .SelectMany(s => s.Orders)
            .Where(o => o.IntendedReceiveDate.Year == year)
            .GroupBy(o => new
            {
                Month = o.IntendedReceiveDate.Month,
            })
            .Select(g => new ShopOrderStatisticDto
            {
                Month = g.Key.Month,
                OrderStatisticDetail = new ShopOrderStatisticDto.OrderStatisticDetailDto
                {
                    Total = g.Count(),
                    TotalOrderInProcess = g.Count(o => o.Status != OrderStatus.Rejected && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Resolved),
                    TotalSuccess = g.Count(o =>
                        (o.Status == OrderStatus.Completed && o.ReasonIdentity == default)
                        || (o.Status == OrderStatus.Resolved && o.Reports.Count > 0 && o.Reports.Any(r => r.CustomerId != default && r.Status == ReportStatus.Rejected))
                    ),
                    TotalFailOrRefund = g.Count(o =>
                        (o.Status == OrderStatus.Completed && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
                        || (o.Status == OrderStatus.Completed && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription())
                        || (o.Status == OrderStatus.Resolved && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription() && o.Reports.Count > 0
                            && o.Reports.Any(r => r.CustomerId != default && r.Status == ReportStatus.Approved))
                        || (o.Status == OrderStatus.Resolved && (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription()
                                                                 || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription()))
                    ),
                    TotalCancelOrReject = g.Count(o =>
                        (o.Status == OrderStatus.Cancelled && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription())
                        || (o.Status == OrderStatus.Cancelled && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription())
                        || (o.Status == OrderStatus.Rejected)
                    ),
                },
            })
            .ToListAsync().ConfigureAwait(false);

        var finalResult = allMonths
            .GroupJoin(
                monthlyOrderStatistic,
                defaultMonth => defaultMonth.Month,
                statistic => statistic.Month,
                (defaultMonth, statistics) =>
                {
                    var detail = statistics.FirstOrDefault()?.OrderStatisticDetail;

                    return new ShopOrderStatisticDto
                    {
                        Month = defaultMonth.Month,
                        OrderStatisticDetail = new ShopOrderStatisticDto.OrderStatisticDetailDto
                        {
                            Total = detail?.Total ?? 0,
                            TotalOrderInProcess = detail?.TotalOrderInProcess ?? 0,
                            TotalSuccess = detail?.TotalSuccess ?? 0,
                            TotalFailOrRefund = detail?.TotalFailOrRefund ?? 0,
                            TotalCancelOrReject = detail?.TotalCancelOrReject ?? 0,
                        },
                    };
                })
            .OrderBy(dto => dto.Month)
            .ToList();

        return finalResult;
    }

    public Task<StatisticSummaryWeb> GetShopWebStatisticSummary(long id, DateTime? dateFrom, DateTime? dateTo)
    {
        return DbSet.Where(s => s.Id == id).Select(shop => new StatisticSummaryWeb
        {
            Revenue = shop.Orders
                .Where(o => (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Resolved) && (dateFrom == default || o.IntendedReceiveDate >= dateFrom.Value.Date)
                                                                                                    && (dateTo == default || o.IntendedReceiveDate <= dateTo.Value.Date))
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
                                  (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription()
                                   || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription()) &&
                                  !o.IsRefund &&
                                  o.Payments.Any(p =>
                                      p.PaymentMethods == PaymentMethods.VnPay &&
                                      p.Type == PaymentTypes.Payment &&
                                      p.Status == PaymentStatus.PaidSuccess)
                                    ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                                    : 0),
            TotalOrder = shop.Orders.Count(o => (dateFrom == default || o.IntendedReceiveDate >= dateFrom.Value.Date)
                                                && (dateTo == default || o.IntendedReceiveDate <= dateTo.Value.Date)),
            TotalPromotion = shop.Orders.Sum(o => (dateFrom == default || o.IntendedReceiveDate >= dateFrom.Value.Date)
                                                  && (dateTo == default || o.IntendedReceiveDate <= dateTo.Value.Date)
                ? o.TotalPromotion
                : 0),
            TotalCustomer = shop.Orders.Where(o => (dateFrom == default || o.IntendedReceiveDate >= dateFrom.Value.Date)
                                                   && (dateTo == default || o.IntendedReceiveDate <= dateTo.Value.Date)).Select(o => o.CustomerId).Distinct().Count(),
        }).SingleAsync();
    }

    public async Task<List<ShopFoodStatisticDto>> GetFoodOrderStatistic(long id, DateTime? dateFrom, DateTime? dateTo)
    {
        // Fetch the revenue data for all foods in the shop's orders
        var foodStatistics = await DbSet.Where(s => s.Id == id)
            .SelectMany(s => s.Orders
                .Where(o => (
                                (o.Status == OrderStatus.Completed && o.ReasonIdentity == null) ||
                                (o.Status == OrderStatus.Completed &&
                                 o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription() &&
                                 o.Payments.Any(p => p.Type == PaymentTypes.Payment && p.PaymentMethods == PaymentMethods.VnPay)) ||
                                (o.Status == OrderStatus.Resolved &&
                                 o.IsReport &&
                                 o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription()) ||
                                (o.Status == OrderStatus.Resolved && o.IsReport &&
                                 (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription()
                                  || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription()) && !o.IsRefund &&
                                 o.Payments.Any(p =>
                                     p.PaymentMethods == PaymentMethods.VnPay &&
                                     p.Type == PaymentTypes.Payment &&
                                     p.Status == PaymentStatus.PaidSuccess))
                            ) &&
                            (dateFrom == null || o.IntendedReceiveDate >= dateFrom.Value.Date) &&
                            (dateTo == null || o.IntendedReceiveDate <= dateTo.Value.Date))
                .SelectMany(o => o.OrderDetails)) // Flatten OrderDetails
            .GroupBy(od => new { od.FoodId, od.Food.Name }) // Group by FoodId and include Food.Name
            .Select(g => new
            {
                FoodId = g.Key.FoodId,
                FoodName = g.Key.Name, // Select the Food Name
                TotalRevenue = g.Sum(od => od.TotalPrice), // Calculate total revenue per food
            })
            .ToListAsync()
            .ConfigureAwait(false);

        // Calculate the total revenue of all foods
        double totalRevenue = foodStatistics.Sum(f => f.TotalRevenue);

        // Step 1: Calculate raw percentages
        var rawPercentages = foodStatistics
            .Select(f => new
            {
                f.FoodName,
                Percent = totalRevenue > 0 ? (f.TotalRevenue / totalRevenue) * 100 : 0,
            })
            .ToList();

        // Step 2: Round percentages to 2 decimal places
        var roundedPercentages = rawPercentages
            .Select(f => new
            {
                f.FoodName,
                RoundedPercent = Math.Round(f.Percent, 2) // Round to 2 decimal places
            })
            .ToList();

        // Step 3: Adjust the largest percentage to ensure the sum equals 100
        double totalRounded = roundedPercentages.Sum(f => f.RoundedPercent);
        double difference = 100 - totalRounded;

        if (roundedPercentages.Any() && Math.Abs(difference) > 0.0001)
        {
            // Find the item with the largest percentage
            var largestContributor = roundedPercentages
                .OrderByDescending(f => f.RoundedPercent)
                .First();

            roundedPercentages = roundedPercentages
                .Select(f => new
                {
                    f.FoodName,
                    RoundedPercent = f.FoodName == largestContributor.FoodName
                        ? Math.Round(f.RoundedPercent + difference, 2) // Adjust only the largest
                        : f.RoundedPercent,
                })
                .ToList();
        }

        // Step 4: Map the results to the DTO
        var result = roundedPercentages
            .Select(f => new ShopFoodStatisticDto
            {
                FoodName = f.FoodName,
                Percent = f.RoundedPercent,
            })
            .OrderByDescending(f => f.Percent)
            .ToList();

        return result;
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
                              (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription()
                               || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP_REPORTED_BY_CUSTOMER.GetDescription()) &&
                              !o.IsRefund &&
                              o.Payments.Any(p =>
                                  p.PaymentMethods == PaymentMethods.VnPay &&
                                  p.Type == PaymentTypes.Payment &&
                                  p.Status == PaymentStatus.PaidSuccess)
                                ? o.TotalPrice - o.TotalPromotion - o.ChargeFee
                                : 0);
    }

    public Task<List<Shop>> GetShopByIds(List<long> ids)
    {
        return DbSet.Include(shop => shop.Location).Where(s => ids.Contains(s.Id) && s.Status == ShopStatus.Active).ToListAsync();
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}