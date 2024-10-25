using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Application.UseCases.Reviews.Queries.GetReviewOfShop;
using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Repositories;

public interface IReviewRepository : IBaseRepository<Review>
{
    Task<bool> CheckExistedReviewOfCustomerByOrderId(long orderId);

    Task<(int TotalCount, List<ReviewShopDto> Reviews)> GetByShopId(long shopId, GetReviewOfShopQuery.ReviewFilter filter, int pageIndex, int pageSize);
}