namespace MealSync.Application.UseCases.Wallets.Models;

public class WalletSummaryResponse
{
    public double AvailableAmount { get; set; }

    public double IncomingAmount { get; set; }

    public double ReportingAmount { get; set; }

    public bool IsAllowedRequestWithdrawal { get; set; }
}