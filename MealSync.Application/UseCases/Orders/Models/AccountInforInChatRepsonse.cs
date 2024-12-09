namespace MealSync.Application.UseCases.Orders.Models;

public class AccountInforInChatRepsonse
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string AvatarUrl { get; set; }

    public string FullName { get; set; }

    public int RoleId { get; set; }
}

public static class AccountInformationChatConverter
{
    public static Dictionary<object, object?> ConvertListToDictionaryFormat(List<AccountInforInChatRepsonse> accounts, long orderId)
    {
        var result = new Dictionary<object, object?>
        {
            { "Id", orderId } // Add the OrderId as the first key-value pair
        };

        // Add account data to the dictionary
        foreach (var account in accounts)
        {
            result[account.Id] = new Dictionary<string, object?>
            {
                { "PhoneNumber", account.PhoneNumber },
                { "Email", account.Email },
                { "AvatarUrl", account.AvatarUrl },
                { "FullName", account.FullName },
                { "RoleId", account.RoleId }
            };
        }

        return result;
    }
}