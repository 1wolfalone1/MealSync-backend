using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderDetailRepository : BaseRepository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public List<long> GetListFoodIdInOrderDetailGroupBy(long orderId)
    {
        var result = DbSet
            .Where(od => od.OrderId == orderId)
            .GroupBy(od => od.FoodId)
            .Select(g => g.Key) // Select only the FoodId (the key of the group)
            .ToList();

        return result;
    }

    public async Task<string> GetOrderDescriptionByOrderId(long orderId)
    {
        var foodDescriptions = await DbSet
            .Where(od => od.OrderId == orderId)
            .GroupBy(od => new { od.Food.Id, od.Food.Name })
            .Select(g => new
            {
                Name = g.Key.Name,
                Quantity = g.Sum(od => od.Quantity),
            })
            .ToListAsync()
            .ConfigureAwait(false);

        return string.Join(", ", foodDescriptions
            .Select(fd => fd.Quantity > 1 ? $"{fd.Name} x{fd.Quantity}" : fd.Name));
    }
}