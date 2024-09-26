using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Product GetByIdIncludeAllInfo(long id);
}