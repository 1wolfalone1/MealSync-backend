using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ProductOperatingHourRepository : BaseRepository<ProductOperatingHour>, IProductOperatingHourRepository
{
    public ProductOperatingHourRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}