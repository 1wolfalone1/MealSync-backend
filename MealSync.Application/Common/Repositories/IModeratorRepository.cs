using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IModeratorRepository : IBaseRepository<Moderator>
{
    (int TotalCount, List<Moderator> Moderators) GetAllModerator(string? searchValue, long? dormitoryId, int status, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize);
}