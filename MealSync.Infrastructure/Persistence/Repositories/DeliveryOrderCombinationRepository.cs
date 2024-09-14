using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class DeliveryOrderCombinationRepository : BaseRepository<DeliveryOrderCombination>, IDeliveryOrderCombinationRepository
{
    public DeliveryOrderCombinationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}