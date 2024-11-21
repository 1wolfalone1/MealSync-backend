using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.WithdrawalRequests.Models;

public class WithdrawalRequestManageDto
{
    public long Id { get; set; }

    public string ShopName { get; set; }

    public double RequestAmount { get; set; }

    public double AvailableAmount { get; set; }

    public string BankCode { get; set; }

    public string BankShortName { get; set; }

    public string BankAccountNumber { get; set; }

    public WithdrawalRequestStatus Status { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}