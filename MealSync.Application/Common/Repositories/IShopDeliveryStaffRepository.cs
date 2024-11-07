using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IShopDeliveryStaffRepository : IBaseRepository<ShopDeliveryStaff>
{
    List<ShopDeliveryStaff> GetListAvailableShopDeliveryStaff(string? searchText, int orderMode, long shopId);

    Task<(int TotalCounts, List<ShopDeliveryStaff> ShopDeliveryStaffs)> GetAllStaffOfShop(
        long shopId, string? searchValue, ShopDeliveryStaffStatus? status, int pageIndex, int pageSize);

    Task<ShopDeliveryStaff?> GetByIdAndShopId(long id, long shopId);
}