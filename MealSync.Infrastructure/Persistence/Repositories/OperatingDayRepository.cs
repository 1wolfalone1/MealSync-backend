using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OperatingDayRepository : BaseRepository<OperatingDay>, IOperatingDayRepository
{
    public OperatingDayRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public OperatingDay? GetByIdAndShopId(long id, long shopId)
    {
        return DbSet.Include(o => o.OperatingFrames)
            .FirstOrDefault(o => o.Id == id && o.ShopOwnerId == shopId);
    }
}