namespace MealSync.Application.Common.Utils;

public class BCrypUnitls
{
    public static string Hash(string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input);
    }

    public static bool Verify(string input, string hashedValue)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(input, hashedValue);
        }
        catch
        {
            // ignore
            return false;
        }
    }
}