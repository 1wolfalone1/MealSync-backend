using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderTransactionRepository : BaseRepository<OrderTransaction>, IOrderTransactionRepository
{
    public OrderTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}