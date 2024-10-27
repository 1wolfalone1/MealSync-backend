using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderDetailRepository : BaseRepository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<string> GetOrderDescriptionByOrderId(long orderId)
    {
        var foodNames = await DbSet.Where(od => od.OrderId == orderId)
            .Select(od => od.Food.Name)
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(false);
        return string.Join(", ", foodNames);
    }
}