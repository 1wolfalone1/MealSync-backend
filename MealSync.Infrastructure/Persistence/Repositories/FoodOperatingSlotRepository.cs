using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodOperatingSlotRepository : BaseRepository<FoodOperatingSlot>, IFoodOperatingSlotRepository
{

    public FoodOperatingSlotRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}