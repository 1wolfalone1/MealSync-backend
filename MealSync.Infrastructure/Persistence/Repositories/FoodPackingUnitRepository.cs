using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodPackingUnitRepository : BaseRepository<FoodPackingUnit>, IFoodPackingUnitRepository
{

    public FoodPackingUnitRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public (int TotalCount, List<FoodPackingUnit> FoodPackingUnits) GetShopFoodPackingUnitsPaging(long shopId, string? searchText, int pageIndex, int pageSize)
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

    public (int TotalCount, List<FoodPackingUnit> FoodPackingUnits) GetShopFoodPackingUnitAdminPaging(string? searchText, int typeMode, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize)
    {
        var query = DbSet.Include(fpu => fpu.Foods).AsQueryable();

        if (searchText != null)
        {
            query = query.Where(fpu => fpu.Id.ToString().Contains(searchText)
                                       || (fpu.ShopId.HasValue && fpu.ShopId.ToString().Contains(searchText))
                                       || fpu.Name.Contains(searchText)).AsQueryable();
        }

        if (typeMode != 0)
        {
            query = query.Where(fpu => (int)fpu.Type == typeMode);
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(fpu => fpu.CreatedDate.Date >= dateFrom.Value.Date && fpu.CreatedDate.Date <= dateTo.Value.Date);
        }

        var count = query.Count();
        var response = query.OrderBy(fpu => fpu.Type)
            .ThenByDescending(fpu => fpu.CreatedDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (count, response);
    }
}