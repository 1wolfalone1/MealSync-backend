using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Wallets.Models;

public class WithdrawalRequestHistoryResponse
{
    public long Id { get; set; }

    public double Amount { get; set; }

    public WithdrawalRequestStatus Status { get; set; }

    public string BankCode { get; set; }

    public string BankShortName { get; set; }

    public string BankAccountNumber { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public WalletHistoryResponse WalletHistory { get; set; }

    public class WalletHistoryResponse
    {
        public long WalletId { get; set; }

        public double AvaiableAmountBefore { get; set; }

        public double IncomingAmountBefore { get; set; }

        public double ReportingAmountBefore { get; set; }
    }
}