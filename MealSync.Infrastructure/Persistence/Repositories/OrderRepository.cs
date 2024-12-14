using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<Order?> GetByIdAndCustomerIdForDetail(long id, long customerId)
    {
        return DbSet.Include(o => o.OrderDetails)
            .ThenInclude(od => od.Food)
            .Include(o => o.Promotion)
            .Include(o => o.CustomerLocation)
            .Include(o => o.Payments)
            .Include(o => o.Shop)
            .ThenInclude(s => s.Location)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);

        // return DbSet.Where(o => o.Id == id && o.CustomerId == customerId)
        //     .Select(o => new Order
        //     {
        //         Id = o.Id,
        //         FullName = o.FullName,
        //         PhoneNumber = o.PhoneNumber,
        //         BuildingName = o.BuildingName,
        //         Status = o.Status,
        //         Note = o.Note,
        //         ShippingFee = o.ShippingFee,
        //         TotalPrice = o.TotalPrice,
        //         TotalPromotion = o.TotalPromotion,
        //         OrderDate = o.OrderDate,
        //         IntendedReceiveDate = o.IntendedReceiveDate,
        //         StartTime = o.StartTime,
        //         EndTime = o.EndTime,
        //         ReceiveAt = o.ReceiveAt,
        //         CompletedAt = o.CompletedAt,
        //         OrderDetails = o.OrderDetails.Select(od => new OrderDetail
        //         {
        //             Id = od.Id,
        //             Quantity = od.Quantity,
        //             BasicPrice = od.BasicPrice,
        //             TotalPrice = od.TotalPrice,
        //             Description = od.Description,
        //             Note = od.Note,
        //             Food = new Food
        //             {
        //                 Name = od.Food.Name,
        //                 ImageUrl = od.Food.ImageUrl,
        //             },
        //         }).ToList(),
        //         Payments = o.Payments.Select(p => new Payment
        //         {
        //             Id = p.Id,
        //             Amount = p.Amount,
        //             Status = p.Status,
        //             Type = p.Type,
        //             PaymentMethods = p.PaymentMethods,
        //         }).ToList(),
        //         Shop = new Shop
        //         {
        //             Id = o.Shop.Id,
        //             Name = o.Shop.Name,
        //             LogoUrl = o.Shop.LogoUrl,
        //         },
        //         ShopLocation = new Location
        //         {
        //             Address = o.ShopLocation.Address,
        //             Latitude = o.ShopLocation.Latitude,
        //             Longitude = o.ShopLocation.Longitude,
        //         },
        //         CustomerLocation = new Location
        //         {
        //             Latitude = o.ShopLocation.Latitude,
        //             Longitude = o.ShopLocation.Longitude,
        //         },
        //         Promotion = o.Promotion == default
        //             ? null
        //             : new Promotion
        //             {
        //                 Id = o.Promotion.Id,
        //                 Title = o.Promotion.Title,
        //                 BannerUrl = o.Promotion.BannerUrl,
        //                 Description = o.Promotion.Description,
        //                 Type = o.Promotion.Type,
        //                 AmountRate = o.Promotion.AmountRate,
        //                 MaximumApplyValue = o.Promotion.MaximumApplyValue,
        //                 AmountValue = o.Promotion.AmountValue,
        //                 MinOrdervalue = o.Promotion.MinOrdervalue,
        //                 StartDate = o.Promotion.StartDate,
        //                 EndDate = o.Promotion.EndDate,
        //                 ApplyType = o.Promotion.ApplyType,
        //             },
        //         Reviews = o.Reviews.Select(od => new Review
        //         {
        //             Id = od.Id,
        //         }).ToList(),
        //         Reports = o.Reports.Select(od => new Report
        //         {
        //             Id = od.Id,
        //         }).ToList(),
        //     }).FirstOrDefaultAsync();
    }

    public async Task<(int TotalCount, IEnumerable<Order> Orders)> GetByCustomerIdAndStatus(
        long customerId, OrderStatus[]? statusList, bool reviewMode, int pageIndex, int pageSize)
    {
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var query = DbSet.Where(o => o.CustomerId == customerId);

        if (statusList != default && statusList.Length > 0)
        {
            query = query.Where(o => statusList.Contains(o.Status));
        }
        var projectedQuery = query.Select(o => new Order
            {
                Id = o.Id,
                Status = o.Status,
                ShippingFee = o.ShippingFee,
                TotalPrice = o.TotalPrice,
                TotalPromotion = o.TotalPromotion,
                OrderDate = o.OrderDate,
                IntendedReceiveDate = o.IntendedReceiveDate,
                StartTime = o.StartTime,
                EndTime = o.EndTime,
                Shop = new Shop
                {
                    Name = o.Shop.Name,
                    LogoUrl = o.Shop.LogoUrl,
                },
                OrderDetails = o.OrderDetails.Select(od => new OrderDetail
                {
                    Id = od.Id,
                }).ToList(),
                Reviews = o.Reviews.Select(od => new Review
                {
                    Id = od.Id,
                }).ToList(),
            })
            .ToList();

        var result = projectedQuery.AsEnumerable(); // This brings the data into memory

        if (reviewMode)
        {
            result = result.Where(o =>
                (o.Status == OrderStatus.Delivered ||
                 o.Status == OrderStatus.IssueReported ||
                 o.Status == OrderStatus.UnderReview ||
                 o.Status == OrderStatus.Resolved) &&
                o.Reviews.Count == 0 &&
                now >= new DateTimeOffset(
                    o.IntendedReceiveDate.Year,
                    o.IntendedReceiveDate.Month,
                    o.IntendedReceiveDate.Day,
                    o.EndTime / 100,
                    o.EndTime % 100,
                    0,
                    TimeSpan.FromHours(7)) &&
                now <= new DateTimeOffset(
                    o.IntendedReceiveDate.Year,
                    o.IntendedReceiveDate.Month,
                    o.IntendedReceiveDate.Day,
                    o.EndTime / 100,
                    o.EndTime % 100,
                    0,
                    TimeSpan.FromHours(7)).AddHours(24));
        }

        var totalCount = result.Count();
        var orders = result
            .OrderByDescending(o => o.CreatedDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (totalCount, orders);
    }

    public Task<Order?> GetByIdAndCustomerIdIncludePayment(long id, long customerId)
    {
        return DbSet.Include(o => o.Payments).FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);
    }

    public Task<bool> CheckExistedByIdAndCustomerId(long id, long customerId)
    {
        return DbSet.AnyAsync(o => o.Id == id && o.CustomerId == customerId);
    }

    public Task<Order?> GetByIdAndCustomerId(long id, long customerId)
    {
        return DbSet.FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);
    }

    public List<(int StartTime, int EndTime, int NumberOfOrder, bool IsCreated)> GetListTimeFrameUnAssignByReceiveDate(DateTime intendedReceiveDate, long shopId)
    {
        var result = DbSet.Where(o => o.IntendedReceiveDate.Date == intendedReceiveDate.Date &&
                                      TimeFrameUtils.GetCurrentHoursInUTC7() <= o.EndTime
                                      && OrderConstant.LIST_ORDER_STATUS_IN_SHOP_ASSIGN_PROCESS.Contains(o.Status)
                                      && o.ShopId == shopId)
            .GroupBy(o => new { o.StartTime, o.EndTime })
            .Select(g => new
            {
                g.Key.StartTime,
                g.Key.EndTime,
                numberOfOrder = g.Count(), // Count the number of orders in the group
                IsCreated = g.Any(o => o.DeliveryPackageId != null) // Check if any order has a DeliveryPackageId
            })
            .ToList();

        return result.Select(x => (x.StartTime, x.EndTime, x.numberOfOrder, x.IsCreated)).OrderBy(x => x.StartTime).ToList();
    }

    public List<Order> GetByIds(List<long> ids)
    {
        return DbSet.Where(o => ids.Contains(o.Id)).ToList();
    }

    public List<Order> GetByIdsWithBuilding(List<long> ids)
    {
        return DbSet.Where(o => ids.Contains(o.Id))
            .Include(o => o.Building).ToList();
    }

    public async Task<ShopStatisticDto> GetShopOrderStatistic(long shopId, DateTime startDate, DateTime endDate)
    {
        var totalOrdersQuery = DbSet
            .Where(o => o.ShopId == shopId && o.IntendedReceiveDate >= startDate && o.IntendedReceiveDate <= endDate);

        var totalOrders = await totalOrdersQuery.CountAsync().ConfigureAwait(false);
        if (totalOrders == 0)
        {
            return new ShopStatisticDto();
        }

        int totalCancelByCustomer = 0; // Đơn hủy, từ chối Status: Cancelled, ReasonIdentity: CustomerCancel
        int totalCancelByShop = 0; // Đơn hủy, từ chối Status: Cancelled, ReasonIdentity: ShopCancel
        int totalReject = 0; // Đơn hủy, từ chối Status: Rejected
        int totalDeliveredCompleted = 0; // Đơn hàng thành công Status: Completed, ReasonIdentity: default => Có Revenue
        int totalFailDeliveredByCustomerCompleted = 0; // Đơn hàng thất bại / hoàn tiền Status: Completed, ReasonIdentity: DeliveryFailByCustomer
        int totalFailDeliveredByShopCompleted = 0; // Đơn hàng thất bại / hoàn tiền Status: Completed, ReasonIdentity: DeliveryFailByShop
        int totalReportResolvedHaveRefund = 0; // Đơn hàng thất bại / hoàn tiền Status: Resolved, ReasonIdentity: DeliveredReportedByCustomer || DeliveryFailReportedByCustomer, IsReport: true, IsRefund: True
        int totalReportResolved = 0; // Status: Resolved, ReasonIdentity: DeliveredReportedByCustomer || DeliveryFailReportedByCustomer, IsReport: true

        totalCancelByCustomer = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Cancelled
                             && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription())
            .ConfigureAwait(false);

        totalCancelByShop = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Cancelled
                             && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription())
            .ConfigureAwait(false);

        totalReject = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Rejected).ConfigureAwait(false);

        totalDeliveredCompleted = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Completed
                             && string.IsNullOrEmpty(o.ReasonIdentity))
            .ConfigureAwait(false);

        totalFailDeliveredByCustomerCompleted = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Completed
                             && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription()
            ).ConfigureAwait(false);

        totalFailDeliveredByShopCompleted = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Completed
                             && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription()
            ).ConfigureAwait(false);

        totalReportResolvedHaveRefund = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Resolved && o.IsReport &&
                             (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription()
                              || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription())
                             && o.IsRefund).ConfigureAwait(false);

        totalReportResolved = await totalOrdersQuery
            .CountAsync(o => o.Status == OrderStatus.Resolved && o.IsReport &&
                             (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription()
                              || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER_REPORTED_BY_CUSTOMER.GetDescription()))
            .ConfigureAwait(false);

        // Status: Completed, ReasonIdentity: default => Có Revenue
        // Status: Completed, ReasonIdentity: DeliveryFailByCustomer, PaymentType = Payment, PaymentMethod != COD => Có Revenue
        // Status: Resolved, ReasonIdentity: DeliveredReportedByCustomer, IsReport: true => Có Revenue
        // Status: Resolved, ReasonIdentity: DeliveryFailByCustomerReportedByCustomer || DeliveryFailByShopReportedByCustomer, IsReport: true, IsRefund: false, PaymentMethods == PaymentMethods.VnPay, PaymentType == PaymentTypes.Payment, PaymentStatus == PaymentStatus.PaidSuccess => Có Revenue
        var revenue = await totalOrdersQuery
            .Where(o => (o.Status == OrderStatus.Completed && o.ReasonIdentity == default)
                        || (o.Status == OrderStatus.Completed && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription()
                                                              && o.Payments.Any(p => p.Type == PaymentTypes.Payment && p.PaymentMethods == PaymentMethods.VnPay))
                        || (o.Status == OrderStatus.Completed && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription() && o.IsReport)
            ).SumAsync(o => o.TotalPrice - o.TotalPromotion - o.ChargeFee).ConfigureAwait(false);

        var topFoodItems = await totalOrdersQuery
            .Where(o => o.Status == OrderStatus.Completed && o.ReasonIdentity == default)
            .SelectMany(o => o.OrderDetails
                    .GroupBy(od => new { od.OrderId, od.FoodId }) // Group by OrderId and FoodId to get unique food per order
                    .Select(g => g.First()) // Select only one record per unique food item in the order
            )
            .GroupBy(od => new { od.FoodId, od.Food.Name }) // Group by Food Name to get total orders
            .Select(g => new ShopStatisticDto.TopFoodItemDto
            {
                Id = g.Key.FoodId,
                Name = g.Key.Name,
                TotalOrders = g.Count(),
            })
            .OrderByDescending(f => f.TotalOrders)
            .Take(5)
            .ToListAsync().ConfigureAwait(false);

        var successfulOrderPercentage = Math.Round((double)totalDeliveredCompleted / totalOrders * 100, 2);

        return new ShopStatisticDto
        {
            SuccessfulOrderPercentage = successfulOrderPercentage,
            Revenue = revenue,
            TotalCancelByCustomer = totalCancelByCustomer,
            TotalCancelByShop = totalCancelByShop,
            TotalReject = totalReject,
            TotalDeliveredCompleted = totalDeliveredCompleted,
            TotalFailDeliveredByCustomerCompleted = totalFailDeliveredByCustomerCompleted,
            TotalFailDeliveredByShopCompleted = totalFailDeliveredByShopCompleted,
            TotalReportResolvedHaveRefund = totalReportResolvedHaveRefund,
            TotalReportResolved = totalReportResolved,
            TopFoodItems = topFoodItems,
        };
    }

    public List<Order> GetListOrderOnStatusDeliveringButOverTimeFrame(int hoursToMarkDeliveryFail, DateTime currentDateTime)
    {
        var result = DbSet.Where(o => o.Status == OrderStatus.FailDelivery && (o.ReasonIdentity == null || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription()) &&
                                      (
                                      o.EndTime == 2400
                                      ? o.IntendedReceiveDate.AddDays(1)
                                      : o.IntendedReceiveDate.AddHours(o.EndTime / 100).AddMinutes(o.EndTime % 100)
                                      ).AddHours(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON) <= currentDateTime
                                      && (
                                          o.EndTime == 2400
                                          ? o.IntendedReceiveDate.AddDays(1)
                                          : o.IntendedReceiveDate.AddHours(o.EndTime / 100).AddMinutes(o.EndTime % 100)
                                      ) >= currentDateTime.AddHours(-OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON))
            .ToList();
        return result;
    }

    public List<Order> GetListOrderOnStatusFailDeliveredWithoutPayIncomingShop(int hoursToMarkDeliveryFail, DateTime currentDateTime)
    {
        var result = DbSet.Where(o => o.Status == OrderStatus.FailDelivery &&
                                      o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription()
                                      && !o.IsRefund && !o.IsReport && !o.IsPaidToShop
                                      && o.Payments.Any(p => p.Type == PaymentTypes.Payment && p.Status == PaymentStatus.PaidSuccess && p.PaymentMethods == PaymentMethods.VnPay)
                                      &&
                                      (o.EndTime == 2400
                                      ? o.IntendedReceiveDate.AddDays(1)
                                      : o.IntendedReceiveDate.AddHours(o.EndTime / 100).AddMinutes(o.EndTime % 100)
                                      ).AddHours(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON) <= currentDateTime)
            .ToList();
        return result;
    }

    public Task<Order?> GetByIdAndCustomerIdForReorder(long id, long customerId)
    {
        return DbSet.Where(o => o.Id == id && o.CustomerId == customerId)
            .Select(o => new Order
            {
                Id = o.Id,
                FullName = o.FullName,
                PhoneNumber = o.PhoneNumber,
                Note = o.Note,
                BuildingId = o.BuildingId,
                ShopId = o.ShopId,
                OrderDetails = o.OrderDetails.Select(od => new OrderDetail
                {
                    Id = od.Id,
                    Quantity = od.Quantity,
                    Note = od.Note,
                    FoodId = od.FoodId,
                    Food = new Food
                    {
                        Id = od.Food.Id,
                        Name = od.Food.Name,
                        Description = od.Food.Description,
                        Price = od.Food.Price,
                        ImageUrl = od.Food.ImageUrl,
                    },
                    OrderDetailOptions = od.OrderDetailOptions.Select(odo => new OrderDetailOption
                    {
                        OptionId = odo.OptionId,
                    }).ToList(),
                }).ToList(),
                Shop = new Shop
                {
                    Id = o.Shop.Id,
                    Name = o.Shop.Name,
                    LogoUrl = o.Shop.LogoUrl,
                },
            }).FirstOrDefaultAsync();
    }

    public Task<List<Order>> GetFailDeliveryAndDelivered(DateTime intendedReceiveDate, int endTime)
    {
        return DbSet.Where(o => ((
                                    o.Status == OrderStatus.FailDelivery &&
                                    (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription()
                                     || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription())
                                ) || o.Status == OrderStatus.Delivered)
                                && ((o.IntendedReceiveDate == intendedReceiveDate && o.EndTime <= endTime) || (o.IntendedReceiveDate < intendedReceiveDate))).ToListAsync();
    }

    public Task<Order> GetIncludeDeliveryPackageById(long id)
    {
        return DbSet.Include(o => o.DeliveryPackage).FirstAsync(o => o.Id == id);
    }

    public Task<int> CountTotalOrderInProcessByShopId(long shopId)
    {
        return DbSet.CountAsync(o => o.ShopId == shopId && (o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Delivering));
    }

    public Task<List<Order>> GetForSystemCancelByShopId(long shopId)
    {
        return DbSet.Include(o => o.Payments)
            .Where(o =>
                o.ShopId == shopId
                && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PendingPayment || o.Status == OrderStatus.Confirmed))
            .ToListAsync();
    }

    public Order GetOrderInforNotification(long orderId)
    {
        return DbSet.Where(o => o.Id == orderId)
            .Include(o => o.DeliveryPackage)
            .ThenInclude(dp => dp.ShopDeliveryStaff)
            .ThenInclude(sds => sds.Account)
            .Include(o => o.Customer)
            .ThenInclude(cus => cus.Account)
            .Include(o => o.Shop)
            .ThenInclude(sh => sh.Account).SingleOrDefault();
    }

    public List<Order> GetOrderListInforNotification(long[] ids)
    {
        return DbSet.Where(o => ids.Contains(o.Id))
            .Include(o => o.DeliveryPackage)
            .ThenInclude(dp => dp.ShopDeliveryStaff)
            .ThenInclude(sds => sds.Account)
            .Include(o => o.Customer)
            .ThenInclude(cus => cus.Account)
            .Include(o => o.Shop)
            .ThenInclude(sh => sh.Account).ToList();
    }

    public (int TotalCount, List<Order> Orders) GetOrderForModerator(string? searchValue, DateTime? dateFrom, DateTime? dateTo, List<OrderStatus> status, List<long> dormitoryIds, int pageIndex, int pageSize)
    {
        var query = DbSet.AsQueryable();

        if (dormitoryIds != null && dormitoryIds.Count > 0)
        {
            query = query.Where(o => dormitoryIds.Contains(o.Building.DormitoryId));
        }

        if (status != null && status.Count > 0)
        {
            query = query.Where(o => status.Contains(o.Status));
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(o => o.IntendedReceiveDate.Date >= dateFrom.Value.Date && o.IntendedReceiveDate.Date <= dateTo.Value.Date);
        }

        if (searchValue != null)
        {
            query = query.Where(o => o.Id.ToString().Contains(searchValue) ||
                                     o.Shop.Name.Contains(searchValue) ||
                                     o.FullName.Contains(searchValue) ||
                                     o.PhoneNumber.Contains(searchValue));
        }

        var totalCount = query.Count();
        var resultList = query
            .OrderByDescending(o => o.IntendedReceiveDate)
            .Select(o => new Order
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                FullName = o.FullName,
                PhoneNumber = o.PhoneNumber,
                TotalPrice = o.TotalPrice,
                TotalPromotion = o.TotalPromotion,
                IntendedReceiveDate = o.IntendedReceiveDate,
                ChargeFee = o.ChargeFee,
                OrderDate = o.OrderDate,
                ReceiveAt = o.ReceiveAt,
                CancelAt = o.CancelAt,
                CompletedAt = o.CompletedAt,
                ResolveAt = o.ResolveAt,
                Status = o.Status,
                StartTime = o.StartTime,
                EndTime = o.EndTime,
                Building = new Building
                {
                    Id = o.BuildingId,
                    Name = o.Building.Name,
                    Dormitory = new Dormitory()
                    {
                        Id = o.Building.Dormitory.Id,
                        Name = o.Building.Dormitory.Name,
                    },
                },
                Shop = new Shop
                {
                    Id = o.ShopId,
                    Name = o.Shop.Name,
                    LogoUrl = o.Shop.LogoUrl,
                    BannerUrl = o.Shop.BannerUrl,
                    Account = new Account()
                    {
                        Id = o.Shop.Account.Id,
                        FullName = o.Shop.Account.FullName,
                    },
                },
            })
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (totalCount, resultList);
    }

    public Task<int> CountTotalOrderInProcessByCustomerId(long customerId)
    {
        return DbSet.CountAsync(o => o.CustomerId == customerId && (o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Delivering));
    }

    public Task<List<Order>> GetForSystemCancelByCustomerId(long customerId)
    {
        return DbSet.Include(o => o.Payments)
            .Where(o =>
                o.CustomerId == customerId
                && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PendingPayment || o.Status == OrderStatus.Confirmed))
            .ToListAsync();
    }

    public Order GetOrderWithDormitoryById(long orderId)
    {
        return DbSet.Where(o => o.Id == orderId)
            .Include(o => o.Building)
            .ThenInclude(b => b.Dormitory).SingleOrDefault();
    }

    public Task<Order> GetOrderIncludePaymentById(long id)
    {
        return DbSet.Include(o => o.Payments).FirstAsync(o => o.Id == id);
    }

    public List<Order> CheckOrderOfShopInDeliveringAndPeparing(long shopId)
    {
        return DbSet.Where(o => o.ShopId == shopId && (o.Status == OrderStatus.Delivering || o.Status == OrderStatus.Preparing)).ToList();
    }

    public async Task<List<Order>> GetOrderOverEndFrameAsync(DateTime currentDateTime)
    {
        return await DbSet.Where(o => OrderConstant.LIST_ORDER_STATUS_IN_PROCESSING.Contains(o.Status) &&
                                (o.EndTime == 2400
                                        ? o.IntendedReceiveDate.AddDays(1)
                                        : o.IntendedReceiveDate.AddHours(o.EndTime / 100).AddMinutes(o.EndTime % 100)) <= currentDateTime).ToListAsync();
    }

    public List<long> GetListAccountIdRelatedToOrder(long orderId)
    {
        var accountIds = new List<long>();
        var order = DbSet.Where(o => o.Id == orderId)
            .Include(o => o.DeliveryPackage).Single();
        accountIds.Add(order.ShopId);
        accountIds.Add(order.CustomerId);
        if (order.DeliveryPackageId.HasValue && order.DeliveryPackage.ShopDeliveryStaffId.HasValue)
        {
            accountIds.Add(order.DeliveryPackage.ShopDeliveryStaffId.Value);
        }

        if (order.HistoryAssignJson != null)
        {
            var histories = JsonConvert.DeserializeObject<List<HistoryAssign>>(order.HistoryAssignJson);
            accountIds.AddRange(histories.Select(h => h.Id).ToList());
        }

        return accountIds;
    }

    public Dictionary<long, List<long>> GetDictionaryAccountIdRelated(List<long> orderIds)
    {
        var result = new Dictionary<long, List<long>>();
        foreach (var id in orderIds)
        {
            result.Add(id, GetListAccountIdRelatedToOrder(id));
        }

        return result;
    }

    public List<Order> GetListOrderOverTwoHour( DateTime currentDateTime)
    {
        var result = DbSet.Where(o => o.Status != OrderStatus.Rejected && o.Status != OrderStatus.PendingPayment &&
                                      (
                                          o.EndTime == 2400
                                              ? o.IntendedReceiveDate.AddDays(1)
                                              : o.IntendedReceiveDate.AddHours(o.EndTime / 100).AddMinutes(o.EndTime % 100)
                                      ).AddHours(OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON) <= currentDateTime
                                      && (
                                          o.EndTime == 2400
                                              ? o.IntendedReceiveDate.AddDays(1)
                                              : o.IntendedReceiveDate.AddHours(o.EndTime / 100).AddMinutes(o.EndTime % 100)
                                      ) >= currentDateTime.AddHours(-OrderConstant.HOUR_ACCEPT_SHOP_FILL_REASON))
            .ToList();
        return result;
    }
}