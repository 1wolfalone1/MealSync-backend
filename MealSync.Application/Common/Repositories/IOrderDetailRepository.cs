using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IOrderDetailRepository : IBaseRepository<OrderDetail>
{
    Task<string> GetOrderDescriptionByOrderId(long orderId);

    List<long> GetListFoodIdInOrderDetailGroupBy(long orderId);
}