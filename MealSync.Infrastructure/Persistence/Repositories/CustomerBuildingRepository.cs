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
            .ThenInclude(b => b.Location)
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

    public async Task<List<CustomerBuilding>> GetByCustomerIdIncludeBuilding(long customerId)
    {
        // Find the default customer building for the given customer
        var defaultCustomerBuilding = await DbSet
            .Include(cb => cb.Building)
            .FirstOrDefaultAsync(cb => cb.CustomerId == customerId && cb.IsDefault)
            .ConfigureAwait(false);

        if (defaultCustomerBuilding == null)
        {
            return new List<CustomerBuilding>();
        }

        var dormitoryId = defaultCustomerBuilding.Building.DormitoryId;

        // Find all customer buildings in the same dormitory
        var customerBuildingsInSameDormitory = await DbSet
            .Include(cb => cb.Building)
            .Where(cb => cb.CustomerId == customerId && cb.Building.DormitoryId == dormitoryId)
            .OrderByDescending(cb => cb.IsDefault)
            .ToListAsync()
            .ConfigureAwait(false);

        return customerBuildingsInSameDormitory;
    }

    public Task<List<CustomerBuilding>> GetByCustomerIdIncludeBuildingAndDormitory(long customerId)
    {
        return DbSet
            .Include(cb => cb.Building)
            .ThenInclude(b => b.Dormitory)
            .Where(cb => cb.CustomerId == customerId)
            .OrderByDescending(cb => cb.IsDefault).ThenByDescending(cb => cb.CreatedDate)
            .ToListAsync();
    }
}