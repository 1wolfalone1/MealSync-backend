using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class PaymentHistoryRepository : BaseRepository<PaymentHistory>, IPaymentHistoryRepository
{
    public PaymentHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}