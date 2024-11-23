using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.WithdrawalRequests.Models;
using MealSync.Application.UseCases.WithdrawalRequests.Queries.GetAllWithdrawalRequestForMod;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class WithdrawalRequestRepository : BaseRepository<WithdrawalRequest>, IWithdrawalRequestRepository
{
    public WithdrawalRequestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<(int TotalCount, List<WithdrawalRequest> WithdrawalRequests)> GetByFilter(
        long walletId, List<WithdrawalRequestStatus>? status, string? searchValue, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize
    )
    {
        var query = DbSet.Where(wr => wr.WalletId == walletId).AsQueryable();

        if (status != default && status.Count > 0)
        {
            query = query.Where(p => status.Contains(p.Status));
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.CreatedDate >= new DateTimeOffset(startDate.Value, TimeSpan.Zero));
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.CreatedDate <= new DateTimeOffset(endDate.Value, TimeSpan.Zero));
        }

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = double.TryParse(searchValue, out var numericValue);
            string numericString = numericValue.ToString();

            query = query.Where(wr =>
                (isNumeric &&
                 (EF.Functions.Like(wr.Id.ToString(), $"%{numericString}%") ||
                  EF.Functions.Like(wr.Amount.ToString(), $"%{numericString}%"))) ||
                EF.Functions.Like(wr.BankCode, $"%{searchValue}%") ||
                EF.Functions.Like(wr.BankAccountNumber, $"%{searchValue}%") ||
                EF.Functions.Like(wr.BankShortName, $"%{searchValue}%") ||
                EF.Functions.Like(wr.Reason ?? string.Empty, $"%{searchValue}%")
            );
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);
        query = query.Select(wr => new WithdrawalRequest
        {
            Id = wr.Id,
            Amount = wr.Amount,
            Status = wr.Status,
            BankCode = wr.BankCode,
            BankShortName = wr.BankShortName,
            BankAccountNumber = wr.BankAccountNumber,
            Reason = wr.Reason,
            CreatedDate = wr.CreatedDate,
            WalletId = wr.WalletId,
            WalletTransaction = wr.WalletTransaction == default
                ? default
                : new WalletTransaction
                {
                    AvaiableAmountBefore = wr.WalletTransaction.AvaiableAmountBefore,
                    IncomingAmountBefore = wr.WalletTransaction.IncomingAmountBefore,
                    ReportingAmountBefore = wr.WalletTransaction.ReportingAmountBefore,
                    Amount = wr.WalletTransaction.Amount,
                },
        }).OrderByDescending(wr => wr.CreatedDate).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        var withdrawalRequests = await query.ToListAsync().ConfigureAwait(false);

        return (totalCount, withdrawalRequests);
    }

    public Task<WithdrawalRequest?> GetDetailByIdAndWalletId(long id, long walletId)
    {
        return DbSet.Include(wr => wr.WalletTransaction)
            .FirstOrDefaultAsync(wr => wr.WalletId == walletId && wr.Id == id);
    }

    public Task<WithdrawalRequest?> GetByIdAndWalletId(long id, long walletId)
    {
        return DbSet.FirstOrDefaultAsync(wr => wr.WalletId == walletId && wr.Id == id);
    }

    public Task<bool> CheckExistingPendingRequestByWalletId(long walletId)
    {
        return DbSet.AnyAsync(wr => wr.WalletId == walletId && wr.Status == WithdrawalRequestStatus.Pending);
    }

    public async Task<(List<WithdrawalRequestManageDto> WithdrawalRequests, int TotalCount)> GetAllWithdrawalRequestForManage(
        List<long> dormitoryIds, string? searchValue, DateTime? dateFrom, DateTime? dateTo,
        WithdrawalRequestStatus? status, long? dormitoryId, GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy? orderBy,
        GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection? direction, int pageIndex, int pageSize)
    {
        var query = DbSet
            .Where(w => w.Wallet.Shop != default
                        && w.Status != WithdrawalRequestStatus.Cancelled
                        && w.Wallet.Shop.Status != ShopStatus.Deleted
                        && w.Wallet.Shop.ShopDormitories.Any(sd => dormitoryIds.Contains(sd.DormitoryId))
            )
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = int.TryParse(searchValue, out var numericValue);

            query = query.Where(w =>
                EF.Functions.Like(w.Wallet.Shop!.Name, $"%{searchValue}%") ||
                EF.Functions.Like(w.BankCode, $"%{searchValue}%") ||
                EF.Functions.Like(w.BankShortName, $"%{searchValue}%") ||
                EF.Functions.Like(w.BankAccountNumber, $"%{searchValue}%") ||
                (isNumeric && EF.Functions.Like(w.Id.ToString(), $"%{numericValue.ToString()}%")) ||
                (isNumeric && EF.Functions.Like(w.Amount.ToString(), $"%{numericValue.ToString()}%")) ||
                (isNumeric && EF.Functions.Like(w.Wallet.AvailableAmount.ToString(), $"%{numericValue.ToString()}%"))
            );
        }

        if (status.HasValue && status.Value != WithdrawalRequestStatus.Cancelled)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        if (dormitoryId.HasValue && dormitoryId > 0)
        {
            query = query.Where(w => w.Wallet.Shop!.ShopDormitories.Any(sd => sd.DormitoryId == dormitoryId));
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(customer => customer.CreatedDate >= dateFrom.Value && customer.CreatedDate <= dateTo.Value);
        }
        else if (dateFrom.HasValue && !dateTo.HasValue)
        {
            query = query.Where(customer => customer.CreatedDate >= dateFrom.Value);
        }
        else if (!dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(customer => customer.CreatedDate <= dateTo.Value);
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        if (orderBy.HasValue && direction.HasValue)
        {
            if (orderBy.Value == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy.CreatedDate)
            {
                query = direction == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection.ASC
                    ? query.OrderBy(w => w.CreatedDate)
                    : query.OrderByDescending(w => w.CreatedDate);
            }
            else if (orderBy.Value == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy.ShopName)
            {
                query = direction == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection.ASC
                    ? query.OrderBy(w => w.Wallet.Shop!.Name)
                    : query.OrderByDescending(w => w.Wallet.Shop!.Name);
            }
            else if (orderBy.Value == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy.RequestAmount)
            {
                query = direction == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection.ASC
                    ? query.OrderBy(w => w.Amount)
                    : query.OrderByDescending(w => w.Amount);
            }
            else if (orderBy.Value == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy.AvailableAmount)
            {
                query = direction == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection.ASC
                    ? query.OrderBy(w => w.Wallet.AvailableAmount)
                    : query.OrderByDescending(w => w.Wallet.AvailableAmount);
            }
            else if (orderBy.Value == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy.BankCode)
            {
                query = direction == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection.ASC
                    ? query.OrderBy(w => w.BankCode)
                    : query.OrderByDescending(w => w.BankCode);
            }
            else if (orderBy.Value == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestOrderBy.BankAccountNumber)
            {
                query = direction == GetAllWithdrawalRequestForModQuery.FilterWithdrawalRequestDirection.ASC
                    ? query.OrderBy(w => w.BankAccountNumber)
                    : query.OrderByDescending(w => w.BankAccountNumber);
            }
        }
        else
        {
            query = query.OrderByDescending(w => w.CreatedDate);
        }

        var withdrawalRequests = query
            .Select(w => new WithdrawalRequestManageDto
            {
                Id = w.Id,
                ShopName = w.Wallet.Shop!.Name,
                RequestAmount = w.Amount,
                AvailableAmount = w.Wallet.AvailableAmount,
                BankCode = w.BankCode,
                BankShortName = w.BankShortName,
                BankAccountNumber = w.BankAccountNumber,
                Status = w.Status,
                CreatedDate = w.CreatedDate,
            })
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (withdrawalRequests, totalCount);
    }

    public Task<WithdrawalRequestDetailManageDto?> GetDetailForManage(List<long> dormitoryIds, long withdrawalRequestId)
    {
        return DbSet
            .Where(w => w.Id == withdrawalRequestId && w.Wallet.Shop != default
                                                    && w.Status != WithdrawalRequestStatus.Cancelled
                                                    && w.Wallet.Shop.Status != ShopStatus.Deleted
                                                    && w.Wallet.Shop.ShopDormitories.Any(sd => dormitoryIds.Contains(sd.DormitoryId))
            ).Select(w => new WithdrawalRequestDetailManageDto
            {
                Id = w.Id,
                ShopName = w.Wallet.Shop!.Name,
                ShopOwnerName = w.Wallet.Shop.Account.FullName ?? string.Empty,
                Email = w.Wallet.Shop.Account.Email,
                RequestAmount = w.Amount,
                AvailableAmount = w.Wallet.AvailableAmount,
                BankCode = w.BankCode,
                BankShortName = w.BankShortName,
                BankAccountNumber = w.BankAccountNumber,
                Reason = w.Reason,
                Status = w.Status,
                CreatedDate = w.CreatedDate,
                UpdatedDate = w.UpdatedDate,
            }).FirstOrDefaultAsync();
    }

    public Task<WithdrawalRequest?> GetForManageIncludeWalletAndShop(List<long> dormitoryIds, long withdrawalRequestId)
    {
        return DbSet
            .Include(wr => wr.Wallet)
            .ThenInclude(w => w.Shop)
            .Where(w => w.Id == withdrawalRequestId && w.Wallet.Shop != default
                                                    && w.Status != WithdrawalRequestStatus.Cancelled
                                                    && w.Wallet.Shop.Status != ShopStatus.Deleted
                                                    && w.Wallet.Shop.ShopDormitories.Any(sd => dormitoryIds.Contains(sd.DormitoryId)))
            .FirstOrDefaultAsync();
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}