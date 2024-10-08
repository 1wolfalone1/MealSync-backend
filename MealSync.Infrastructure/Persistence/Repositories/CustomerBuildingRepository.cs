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

    public CustomerBuilding? GetByBuildingIdAndCustomerId(long buildingId, long customerId)
    {
        return DbSet.FirstOrDefault(cb => cb.BuildingId == buildingId && cb.CustomerId == customerId);
    }

    public CustomerBuilding GetByBuildingIdAndCustomerIdIncludeBuilding(long buildingId, long customerId)
    {
        return DbSet.Include(cb => cb.Building)
            .First(cb => cb.BuildingId == buildingId && cb.CustomerId == customerId);
    }
}