using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<Order?> GetByIdAndCustomerId(long id, long customerId);

    Task<(int TotalCount, IEnumerable<Order> Orders)> GetByCustomerIdAndStatus(long customerId, OrderStatus? status, int pageIndex, int pageSize);

    Task<Order?> GetByIdAndCustomerIdIncludePayment(long id, long customerId);
}