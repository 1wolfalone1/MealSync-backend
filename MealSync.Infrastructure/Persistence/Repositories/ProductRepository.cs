using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Product GetByIdIncludeAllInfo(long id)
    {
        return DbSet.Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductOperatingHours)
            .Include(p => p.ProductQuestions)
            .First(p => p.Id == id);
    }
}