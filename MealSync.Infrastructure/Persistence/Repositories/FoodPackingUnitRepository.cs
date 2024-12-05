using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodPackingUnitRepository : BaseRepository<FoodPackingUnit>, IFoodPackingUnitRepository
{

    public FoodPackingUnitRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public (int TotalCount, List<FoodPackingUnit> FoodPackingUnits) GetShopFoodPackingUnitsPaging(long shopId, string searchText, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(fpu => fpu.ShopId == shopId || fpu.Type == FoodPackingUnitType.System).AsQueryable();

        if (searchText != null)
        {
            query = query.Where(fpu => fpu.Id.ToString().Contains(searchText)
                                       || fpu.Name.Contains(searchText)).AsQueryable();
        }

        var count = query.Count();
        var response = query.OrderByDescending(fpu => fpu.ShopId)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (count, response);
    }
}