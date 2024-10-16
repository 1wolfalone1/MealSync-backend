using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<Order?> GetByIdAndCustomerId(long id, long customerId);

    Task<(int TotalCount, IEnumerable<Order> Orders)> GetByCustomerId(long customerId, int pageIndex, int pageSize);
}