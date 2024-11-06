using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

internal class WithdrawalRequestRepository : BaseRepository<WithdrawalRequest>, IWithdrawalRequestRepository
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
                },
        }).Skip((pageIndex - 1) * pageSize).Take(pageSize);
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

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}