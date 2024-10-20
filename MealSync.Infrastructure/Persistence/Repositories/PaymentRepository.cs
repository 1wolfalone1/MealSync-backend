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

    public Task<Payment> GetOrderPaymentVnPayByOrderId(long orderId)
    {
        return DbSet.Include(p => p.Order)
            .ThenInclude(o => o.Shop)
            .OrderBy(p => p.CreatedDate)
            .FirstAsync(p =>
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