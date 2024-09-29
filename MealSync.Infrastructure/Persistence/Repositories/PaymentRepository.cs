using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}