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
}