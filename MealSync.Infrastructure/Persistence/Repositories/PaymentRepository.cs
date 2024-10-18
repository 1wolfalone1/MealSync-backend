using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<Payment> GetOldestByOrderId(long orderId)
    {
        return DbSet.OrderBy(p => p.CreatedDate).FirstAsync(p =>
            p.OrderId == orderId
            && p.Type == PaymentTypes.Payment
            && p.PaymentMethods == PaymentMethods.VnPay);
    }

    public Task<Payment?> GetPaymentVnPayByOrderId(long orderId)
    {
        return DbSet.FirstOrDefaultAsync(p =>
            p.OrderId == orderId
            && p.Type == PaymentTypes.Payment
            && p.PaymentMethods == PaymentMethods.VnPay);
    }
}