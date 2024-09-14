using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class CustomerBuildingRepository : BaseRepository<CustomerBuilding>, ICustomerBuildingRepository
{
    public CustomerBuildingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}