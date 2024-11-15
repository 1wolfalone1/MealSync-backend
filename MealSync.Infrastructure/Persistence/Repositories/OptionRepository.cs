using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OptionRepository : BaseRepository<Option>, IOptionRepository
{
    public OptionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<Option?> GetActiveByIdAndOptionGroupId(long id, long optionGroupId)
    {
        return DbSet.FirstOrDefaultAsync(o => o.Id == id && o.OptionGroupId == optionGroupId && o.Status == OptionStatus.Active);
    }

    public async Task<bool> CheckAllOptionAndOptionGroupActiveByIds(List<long> ids)
    {
        var totalActiveOption = await DbSet.CountAsync(
            o => ids.Contains(o.Id)
                 && o.Status == OptionStatus.Active
                 && o.OptionGroup.Status == OptionGroupStatus.Active).ConfigureAwait(false);
        return totalActiveOption == ids.Count;
    }

    public Task<Option> GetIncludeOptionGroupById(long id)
    {
        return DbSet.Include(o => o.OptionGroup).FirstAsync(o => o.Id == id);
    }
}