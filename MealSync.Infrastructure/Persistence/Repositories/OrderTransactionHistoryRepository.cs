using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderTransactionHistoryRepository : BaseRepository<OrderTransactionHistory>, IOrderTransactionHistoryRepository
{
    public OrderTransactionHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}