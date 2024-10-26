using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IShopDeliveryStaffRepository : IBaseRepository<ShopDeliveryStaff>
{
    List<ShopDeliveryStaff> GetListAvailableShopDeliveryStaff(string? searchText, int orderMode, long shopId);
}