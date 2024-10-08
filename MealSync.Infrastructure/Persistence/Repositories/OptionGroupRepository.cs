using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OptionGroupRepository : BaseRepository<OptionGroup>, IOptionGroupRepository
{
    public OptionGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public bool CheckExistedByIdAndShopId(long id, long shopId)
    {
        return DbSet.Any(og => og.Id == id && og.ShopId == shopId && og.Status != OptionGroupStatus.Delete);
    }

    public OptionGroup GetByIdIncludeOption(long id)
    {
        return DbSet.Include(og => og.Options.Where(o => o.Status != OptionStatus.Delete))
            .First(og => og.Id == id && og.Status != OptionGroupStatus.Delete);
    }
}