namespace MealSync.Application.Common.Utils;

public static class MoneyUtils
{
    public static int AVAILABLE_AMOUNT_LIMIT = -200000;

    public static int RoundToNearestInt(double amount)
    {
        return (int)Math.Round(amount, MidpointRounding.AwayFromZero);
    }

    public static string FormatMoneyWithDots(double amount)
    {
        return string.Format("{0:N0}", amount).Replace(",", ".");
    }
}