using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

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
    public static Dictionary<object, object?> ConvertListToDictionaryFormat(List<AccountInforInChatRepsonse> accounts, long orderId, IShopRepository shopRepository)
    {
        var result = new Dictionary<object, object?>
        {
            { "id", orderId } // Add the OrderId as the first key-value pair
        };

        var shopAccount = accounts.Where(a => a.RoleId == (int)Domain.Enums.Roles.ShopOwner).FirstOrDefault();
        var shop = shopRepository.GetById(shopAccount.Id);
        // Add account data to the dictionary
        foreach (var account in accounts)
        {
            if (account.RoleId == (int)Domain.Enums.Roles.ShopOwner)
            {
                account.FullName = shop.Name;
                account.AvatarUrl = shop.LogoUrl;
            }
            else if (account.RoleId == (int)Domain.Enums.Roles.ShopDelivery)
            {
                account.FullName = "Shipper - " + account.FullName;
                account.AvatarUrl = shop.LogoUrl;
            }

            result[account.Id] = new Dictionary<string, object?>
            {
                { "id", account.Id },
                { "phoneNumber", account.PhoneNumber },
                { "email", account.Email },
                { "avatarUrl", account.AvatarUrl },
                { "fullName", account.FullName },
                { "roleId", account.RoleId }
            };
        }

        return result;
    }
}