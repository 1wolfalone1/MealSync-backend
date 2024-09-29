using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderDetailProductVariantRepository : BaseRepository<OrderDetailProductVariant>, IOrderDetailProductVariantRepository
{
    public OrderDetailProductVariantRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}