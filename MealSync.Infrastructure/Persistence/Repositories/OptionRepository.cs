using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OptionRepository : BaseRepository<Option>, IOptionRepository
{
    public OptionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}