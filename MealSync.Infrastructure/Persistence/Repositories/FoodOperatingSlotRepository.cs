using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodOperatingSlotRepository : BaseRepository<FoodOperatingSlot>, IFoodOperatingSlotRepository
{

    public FoodOperatingSlotRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public List<FoodOperatingSlot> GetOperatingSlotsWithFoodByOpId(long operatingId)
    {
        return DbSet
            .Where(fos => fos.OperatingSlotId == operatingId)
            .Include(fos => fos.Food).ToList();
    }

    public Task<bool> ExistedByFoodIdAndOperatingSlotId(long foodId, long operatingSlotId, bool isOrderNextDay)
    {
        return DbSet.AnyAsync(fos => fos.FoodId == foodId && fos.OperatingSlotId == operatingSlotId && (isOrderNextDay || !fos.OperatingSlot.IsReceivingOrderPaused));
    }
}