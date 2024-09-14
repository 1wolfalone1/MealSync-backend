using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ModeratorRepository : BaseRepository<Moderator>, IModeratorRepository
{
    public ModeratorRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}