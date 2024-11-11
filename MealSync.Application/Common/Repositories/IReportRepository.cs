using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IReportRepository : IBaseRepository<Report>
{
    Task<bool> CheckExistedCustomerReport(long orderId, long customerId);
}