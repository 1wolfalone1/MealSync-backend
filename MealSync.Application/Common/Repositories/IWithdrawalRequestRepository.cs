using MealSync.Application.UseCases.WithdrawalRequests.Models;
using MealSync.Application.UseCases.WithdrawalRequests.Queries.GetAllWithdrawalRequestForMod;
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

    Task<(List<WithdrawalRequestManageDto> WithdrawalRequests, int TotalCount)> GetAllWithdrawalRequestForManage(
        List<long> dormitoryIds, string? searchValue,
        DateTime? dateFrom, DateTime? dateTo, WithdrawalRequestStatus? status, long? dormitoryId,
        GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy? orderBy,
        GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection? direction,
        int pageIndex, int pageSize);

    Task<WithdrawalRequestDetailManageDto?> GetDetailForManage(List<long> dormitoryIds, long withdrawalRequestId);

    Task<WithdrawalRequest?> GetForManageIncludeWalletAndShop(List<long> dormitoryIds, long withdrawalRequestId);

    Task<WithdrawalRequestDetailManageDto?> GetDetailForAdmin(long withdrawalRequestId);
}