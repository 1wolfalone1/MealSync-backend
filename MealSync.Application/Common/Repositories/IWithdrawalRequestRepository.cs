using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IWithdrawalRequestRepository : IBaseRepository<WithdrawalRequest>
{
    Task<(int TotalCount, List<WithdrawalRequest> WithdrawalRequests)> GetByFilter(
        long walletId, List<WithdrawalRequestStatus>? status, string? searchValue, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize);

    Task<WithdrawalRequest?> GetDetailByIdAndWalletId(long id, long walletId);

    Task<WithdrawalRequest?> GetByIdAndWalletId(long id, long walletId);

    Task<bool> CheckExistingPendingRequestByWalletId(long walletId);
}