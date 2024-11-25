using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.WithdrawalRequests.Models;

public class WithdrawalRequestDetailManageDto
{
    public long Id { get; set; }

    public string ShopName { get; set; }

    public string ShopOwnerName { get; set; }

    public string Email { get; set; }

    public double RequestAmount { get; set; }

    public double AvailableAmount { get; set; }

    public string BankCode { get; set; }

    public string BankShortName { get; set; }

    public string BankAccountNumber { get; set; }

    public string BankAccountName { get; set; }

    public WithdrawalRequestStatus Status { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
}