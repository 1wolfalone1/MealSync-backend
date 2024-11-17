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

    public Task<Payment> GetOrderPaymentVnPayById(long id)
    {
        return DbSet.Include(p => p.Order)
            .ThenInclude(o => o.Shop)
            .OrderBy(p => p.CreatedDate)
            .FirstAsync(p =>
                p.Id == id
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

    public Task<List<Payment>> GetPendingPaymentOrder(DateTime intendedReceiveDate, int endTime)
    {
        return DbSet.Include(p => p.Order).Where(p =>
            p.PaymentMethods == PaymentMethods.VnPay
            && p.Type == PaymentTypes.Payment
            && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.PaidFail)
            && ((p.Order.IntendedReceiveDate == intendedReceiveDate && p.Order.EndTime <= endTime) || (p.Order.IntendedReceiveDate < intendedReceiveDate))
            && p.Order.Status == OrderStatus.PendingPayment
        ).ToListAsync();
    }

    public Task<Payment?> GetForRepaymentByOrderId(long orderId)
    {
        return DbSet.FirstOrDefaultAsync(p =>
            p.OrderId == orderId
            && p.Order.Status == OrderStatus.PendingPayment
            && p.Type == PaymentTypes.Payment
            && p.PaymentMethods == PaymentMethods.VnPay
            && (p.Status == PaymentStatus.PaidFail || p.Status == PaymentStatus.Pending));
    }

    public Task<Payment> GetPaymentByOrderId(long orderId)
    {
        return DbSet.FirstAsync(p =>
            p.OrderId == orderId
            && p.Type == PaymentTypes.Payment);
    }
}