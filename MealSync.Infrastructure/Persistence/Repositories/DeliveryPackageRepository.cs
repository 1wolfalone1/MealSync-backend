using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class DeliveryPackageRepository : BaseRepository<DeliveryPackage>, IDeliveryPackageRepository
{
    public DeliveryPackageRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}