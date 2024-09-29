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
}