using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.Moderators.Queries.GetAllModerators;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ModeratorRepository : BaseRepository<Moderator>, IModeratorRepository
{
    public ModeratorRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public (int TotalCount, List<Moderator> Moderators) GetAllModerator(string? searchValue, long? dormitoryId, int status, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize)
    {
        var query = DbSet.Include(m => m.ModeratorDormitories)
            .ThenInclude(m => m.Dormitory)
            .Include(m => m.Account).AsQueryable();

        if (searchValue != null)
        {
            query = query.Where(m => m.Id.ToString().Contains(searchValue)
                                     || m.Account.FullName.Contains(searchValue));
        }

        if (dormitoryId.HasValue)
        {
            query = query.Where(m => m.ModeratorDormitories.Any(md => md.DormitoryId == dormitoryId));
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(m => m.CreatedDate >= dateFrom && m.CreatedDate <= dateTo);
        }

        if (status != 0)
        {
            query = query.Where(m => (int)m.Account.Status == status);
        }

        var totalCount = query.Count();
        var response = query.OrderByDescending(m => m.CreatedDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (totalCount, response);
    }
}