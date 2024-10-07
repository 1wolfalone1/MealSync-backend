using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class CustomerBuildingRepository : BaseRepository<CustomerBuilding>, ICustomerBuildingRepository
{
    public CustomerBuildingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public CustomerBuilding? GetDefaultByCustomerId(long id)
    {
        return DbSet.Include(cb => cb.Building)
            .SingleOrDefault(cb => cb.CustomerId == id && cb.IsDefault);
    }
}