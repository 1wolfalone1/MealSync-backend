using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Application.UseCases.Reviews.Queries.GetReviewOfShop;
using MealSync.Application.UseCases.Reviews.Queries.Shop.GetReviewByFilter;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IReviewRepository : IBaseRepository<Review>
{
    Task<bool> CheckExistedReviewOfCustomerByOrderId(long orderId);

    Task<(int TotalCount, List<ReviewShopDto> Reviews)> GetByShopId(long shopId, GetReviewOfShopQuery.ReviewFilter filter, int pageIndex, int pageSize);

    Task<ReviewOverviewDto> GetReviewOverviewByShopId(long shopId);

    Task<(int TotalCount, List<ReviewOfShopOwnerDto> Reviews)> GetShopReview(long shopId, string? searchValue, RatingRanges? rating, GetReviewByFilterQuery.FilterQuery filter, int pageIndex, int pageSize);

    List<ReviewOfShopOwnerDto> GetReviewByOrderId(long orderId);
}