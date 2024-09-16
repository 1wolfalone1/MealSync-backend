using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ProductQuestionOptionRepository : BaseRepository<ProductQuestionOption>, IProductQuestionOptionRepository
{
    public ProductQuestionOptionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}