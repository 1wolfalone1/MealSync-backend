using MealSync.Application.UseCases.ShopOwners.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<Order?> GetByIdAndCustomerIdForDetail(long id, long customerId);

    Task<(int TotalCount, IEnumerable<Order> Orders)> GetByCustomerIdAndStatus(long customerId, OrderStatus[]? statusList, bool reviewMode, int pageIndex, int pageSize);

    Task<Order?> GetByIdAndCustomerIdIncludePayment(long id, long customerId);

    Task<bool> CheckExistedByIdAndCustomerId(long id, long customerId);

    Task<Order?> GetByIdAndCustomerId(long id, long customerId);

    List<(int StartTime, int EndTime, int NumberOfOrder, bool IsCreated)> GetListTimeFrameUnAssignByReceiveDate(DateTime intendedReceiveDate, long shopId);

    List<Order> GetByIds(List<long> ids);

    Task<ShopStatisticDto> GetShopOrderStatistic(long shopId, DateTime startDate, DateTime endDate);

    List<Order> GetListOrderOnStatusDeliveringButOverTimeFrame(int hoursToMarkDeliveryFail, DateTime currentDate);

    Task<Order?> GetByIdAndCustomerIdForReorder(long id, long customerId);

    List<Order> GetListOrderOnStatusFailDeliveredWithoutPayIncomingShop(int hoursToMarkDeliveryFail, DateTime currentDateTime);

    Task<List<Order>> GetFailDeliveryAndDelivered(DateTime intendedReceiveDate, int endTime);

    Task<Order> GetIncludeDeliveryPackageById(long orderId);

    Task<int> CountTotalOrderInProcessByShopId(long shopId);

    Task<List<Order>> GetForSystemCancelByShopId(long shopId);

    Order GetOrderInforNotification(long orderId);

    (int TotalCount, List<Order> Orders) GetOrderForModerator(string? searchValue, DateTime? dateFrom, DateTime? dateTo, List<OrderStatus> status, List<long> dormitoryIds, int pageIndex, int pageSize);

    List<Order> GetOrderListInforNotification(long[] ids);

    Task<int> CountTotalOrderInProcessByCustomerId(long customerId);

    Task<List<Order>> GetForSystemCancelByCustomerId(long customerId);

    Order GetOrderWithDormitoryById(long orderId);

    Task<Order> GetOrderIncludePaymentById(long id);

    List<Order> CheckOrderOfShopInDeliveringAndPeparing(long shopId);

    Task<List<Order>> GetOrderOverEndFrameAsync(DateTime currentDateTime);
}