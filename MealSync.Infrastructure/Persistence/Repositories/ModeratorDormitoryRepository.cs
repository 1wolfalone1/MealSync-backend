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

    public Task<List<ModeratorDormitory>> GetAllIncludeDormitoryByModeratorId(long moderatorId)
    {
        return DbSet.Include(md => md.Dormitory).Where(md => md.ModeratorId == moderatorId).ToListAsync();
    }
}