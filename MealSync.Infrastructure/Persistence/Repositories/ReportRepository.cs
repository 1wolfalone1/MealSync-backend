using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ReportRepository : BaseRepository<Report>, IReportRepository
{
    public ReportRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<bool> CheckExistedCustomerReport(long orderId, long customerId)
    {
        return DbSet.AnyAsync(r => r.OrderId == orderId && r.CustomerId == customerId);
    }
}