using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class SystemResourceRepository : BaseRepository<SystemResource>, ISystemResourceRepository
{
    public SystemResourceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public string? GetByResourceCode(string code)
    {
        return DbSet.FirstOrDefault(sr => sr.ResourceCode == code)?.ResourceContent;
    }

    public string? GetByResourceCode(string code, params object[] args)
    {
        var systemResource = DbSet.FirstOrDefault(sr => sr.ResourceCode == code);
        return systemResource == default ? null : string.Format(systemResource.ResourceContent, args);
    }
}