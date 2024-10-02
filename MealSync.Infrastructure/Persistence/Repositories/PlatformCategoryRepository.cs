using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class PlatformCategoryRepository : BaseRepository<PlatformCategory>, IPlatformCategoryRepository
{
    public PlatformCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public bool CheckExistedById(long id)
    {
        return DbSet.Any(pc => pc.Id == id);
    }
}