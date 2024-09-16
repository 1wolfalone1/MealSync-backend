using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderDetailOptionRepository : BaseRepository<OrderDetailOption>, IOrderDetailOptionRepository
{
    public OrderDetailOptionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}