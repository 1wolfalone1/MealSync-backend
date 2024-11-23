using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IModeratorDormitoryRepository : IBaseRepository<ModeratorDormitory>
{
    Task<List<ModeratorDormitory>> GetAllDormitoryByModeratorId(long moderatorId);

    Task<List<ModeratorDormitory>> GetAllIncludeDormitoryByModeratorId(long moderatorId);
}