using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Services.Notifications.Models;

public class WithdrawalRequestNotification
{
    public long Id { get; set; }

    public long WalletId { get; set; }

    public double Amount { get; set; }

    public WithdrawalRequestStatus Status { get; set; }

    public string BankCode { get; set; }

    public string BankShortName { get; set; }

    public string BankAccountNumber { get; set; }

    public string? Reason { get; set; }
}