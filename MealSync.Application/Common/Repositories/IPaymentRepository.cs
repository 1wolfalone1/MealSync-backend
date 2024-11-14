using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<Payment> GetOrderPaymentVnPayById(long id);

    Task<Payment?> GetPaymentVnPayByOrderId(long orderId);

    Task<List<Payment>> GetPendingPaymentOrder(DateTime intendedReceiveDate, int startTime, int endTime);

    Task<Payment?> GetForRepaymentByOrderId(long orderId);

    Task<Payment> GetPaymentByOrderId(long orderId);
}