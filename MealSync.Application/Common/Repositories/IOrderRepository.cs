using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<Order?> GetByIdAndCustomerIdForDetail(long id, long customerId);

    Task<(int TotalCount, IEnumerable<Order> Orders)> GetByCustomerIdAndStatus(long customerId, List<OrderStatus>? statusList, int pageIndex, int pageSize);

    Task<Order?> GetByIdAndCustomerIdIncludePayment(long id, long customerId);

    Task<bool> CheckExistedByIdAndCustomerId(long id, long customerId);

    Task<Order?> GetByIdAndCustomerId(long id, long customerId);
}