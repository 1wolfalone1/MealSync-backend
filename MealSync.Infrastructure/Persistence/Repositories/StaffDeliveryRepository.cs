using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class StaffDeliveryRepository : BaseRepository<StaffDelivery>, IStaffDeliveryRepository
{
    public StaffDeliveryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}