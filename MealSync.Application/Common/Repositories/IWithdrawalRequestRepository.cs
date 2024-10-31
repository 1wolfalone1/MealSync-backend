using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IWithdrawalRequestRepository : IBaseRepository<WithdrawalRequest>
{
    Task<(int TotalCount, List<WithdrawalRequest> WithdrawalRequests)> GetByFilter(
        long walletId, WithdrawalRequestStatus? status, string? searchValue, DateTime? createdDate, int pageIndex, int pageSize);
}