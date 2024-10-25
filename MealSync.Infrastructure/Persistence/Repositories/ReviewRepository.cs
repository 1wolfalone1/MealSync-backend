using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<bool> CheckExistedReviewOfCustomerByOrderId(long orderId)
    {
        return DbSet.AnyAsync(r => r.OrderId == orderId);
    }
}