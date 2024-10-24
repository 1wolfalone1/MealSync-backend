namespace MealSync.Application.Common.Utils;

public static class MoneyUtils
{
    public static int RoundToNearestInt(double amount)
    {
        return (int)Math.Round(amount, MidpointRounding.AwayFromZero);
    }
}