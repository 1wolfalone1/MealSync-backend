using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Services.Notifications.Models;

public class WalletNotification
{
    public long Id { get; set; }

    public double AvailableAmount { get; set; }

    public double IncomingAmount { get; set; }

    public double ReportingAmount { get; set; }

    public DateTimeOffset NextTransferDate { get; set; }

    public WalletTypes Type { get; set; }
}