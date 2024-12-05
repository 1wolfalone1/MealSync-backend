using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IFoodPackingUnitRepository : IBaseRepository<FoodPackingUnit>
{
    (int TotalCount, List<FoodPackingUnit> FoodPackingUnits) GetShopFoodPackingUnitsPaging(long shopId, string searchText, int pageIndex, int pageSize);
}