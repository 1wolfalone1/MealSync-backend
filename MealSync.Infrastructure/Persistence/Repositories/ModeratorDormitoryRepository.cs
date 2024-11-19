using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ModeratorDormitoryRepository : BaseRepository<ModeratorDormitory>, IModeratorDormitoryRepository
{
    public ModeratorDormitoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<List<ModeratorDormitory>> GetAllDormitoryByModeratorId(long moderatorId)
    {
        return DbSet.Where(md => md.ModeratorId == moderatorId).ToListAsync();
    }
}