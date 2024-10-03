using System.Collections;
using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodRepository : BaseRepository<Food>, IFoodRepository
{
    public FoodRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Food GetByIdIncludeAllInfo(long id)
    {
        return DbSet.Include(f => f.PlatformCategory)
            .Include(f => f.ShopCategory)
            .Include(f => f.FoodOperatingSlots).ThenInclude(op => op.OperatingSlot)
            .Include(f => f.FoodOptionGroups).ThenInclude(fog => fog.OptionGroup).ThenInclude(og => og.Options)
            .First(f => f.Id == id);
    }

    public async Task<(int TotalCount, IEnumerable<Food> Foods)> GetTopFood(long dormitoryId, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(food => food.Status == FoodStatus.Active // The food item must be active
                                        && !food.IsSoldOut // The food item must not be sold out
                                        && food.Shop.ShopDormitories
                                            .Select(shopDormitory => shopDormitory.DormitoryId)
                                            .Contains(dormitoryId) // The shop must be in the specified dormitory
                                        && !food.Shop.IsReceivingOrderPaused // The shop must be accepting orders
                                        && food.Shop.Status == ShopStatus.Active // The shop must be active
        ).AsQueryable();
        var totalCount = await query.CountAsync().ConfigureAwait(false);
        List<Food> foods;

        if (totalCount == 0)
        {
            foods = new List<Food>();
        }
        else
        {
            foods = await query.OrderByDescending(food => food.TotalOrder) // Orders the result by the number of total orders in descending order
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        return (totalCount, foods);
    }
}