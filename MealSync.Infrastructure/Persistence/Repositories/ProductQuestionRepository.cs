using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ProductQuestionRepository : BaseRepository<ProductQuestion>, IProductQuestionRepository
{
    public ProductQuestionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}