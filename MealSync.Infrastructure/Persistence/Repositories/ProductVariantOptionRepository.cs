using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ProductVariantOptionRepository : BaseRepository<ProductVariantOption>, IProductVariantOptionRepository
{
    public ProductVariantOptionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}