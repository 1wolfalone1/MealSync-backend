using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Wallets.Models;

public class WalletTransactionResponse
{
    public long Id { get; set; }

    public long? WalletFromId { get; set; }

    public string NameOfWalletOwnerFrom { get; set; }

    public long? WalletToId { get; set; }

    public string NameOfWalletOwnerTo { get; set; }

    public long? WithdrawalRequestId { get; set; }

    public long? PaymentId { get; set; }

    public double AvaiableAmountBefore { get; set; }

    public double IncomingAmountBefore { get; set; }

    public double ReportingAmountBefore { get; set; }

    public double Amount { get; set; }

    public double TotalAmountAfter { get; set; }

    public WalletTransactionType Type { get; set; }

    public string Description { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}