using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OptionGroupRepository : BaseRepository<OptionGroup>, IOptionGroupRepository
{
    public OptionGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}