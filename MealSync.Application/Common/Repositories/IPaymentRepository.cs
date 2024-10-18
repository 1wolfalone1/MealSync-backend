using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<Payment> GetOldestByOrderId(long orderId);

    Task<Payment?> GetPaymentVnPayByOrderId(long orderId);
}