using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OperatingSlotRepository : BaseRepository<OperatingSlot>, IOperatingSlotRepository
{
    public OperatingSlotRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public OperatingSlot? GetByIdAndShopId(long id, long shopId)
    {
        return DbSet
            .FirstOrDefault(o => o.Id == id && o.ShopId == shopId);
    }

    public Task<OperatingSlot?> GetAvailableForTimeRangeOrder(long shopId, long orderStartTimeReceiving, long orderEndTimeReceiving)
    {
        return DbSet.FirstOrDefaultAsync(slot =>
            slot.ShopId == shopId &&
            slot.IsActive &&
            slot.StartTime <= orderStartTimeReceiving &&
            slot.EndTime >= orderEndTimeReceiving);
    }

    public Task<OperatingSlot?> GetActiveByIdAndShopId(long operatingSlotId, long shopId)
    {
        return DbSet.FirstOrDefaultAsync(slot =>
            slot.Id == operatingSlotId &&
            slot.ShopId == shopId &&
            slot.IsActive);
    }

    public Task<List<OperatingSlot>> GetAllSlotReceivingOrderPaused()
    {
        return DbSet.Where(o => o.IsReceivingOrderPaused).ToListAsync();
    }
}