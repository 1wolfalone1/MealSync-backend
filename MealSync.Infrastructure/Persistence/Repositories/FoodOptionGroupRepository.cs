using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class FoodOptionGroupRepository : BaseRepository<FoodOptionGroup>, IFoodOptionGroupRepository
{

    public FoodOptionGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}