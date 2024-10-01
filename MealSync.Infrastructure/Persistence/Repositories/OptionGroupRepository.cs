using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OptionGroupRepository : BaseRepository<OptionGroup>, IOptionGroupRepository
{
    public OptionGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public bool CheckExistedByIdAndShopId(long id, long shopId)
    {
        return DbSet.Any(og => og.Id == id && og.ShopId == shopId);
    }
}