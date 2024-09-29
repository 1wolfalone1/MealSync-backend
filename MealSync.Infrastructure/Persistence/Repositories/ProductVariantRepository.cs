using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ProductVariantRepository : BaseRepository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}