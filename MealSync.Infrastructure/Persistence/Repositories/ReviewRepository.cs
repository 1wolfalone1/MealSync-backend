using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Application.UseCases.Reviews.Queries.GetReviewOfShop;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<bool> CheckExistedReviewOfCustomerByOrderId(long orderId)
    {
        return DbSet.AnyAsync(r => r.OrderId == orderId && r.Entity == ReviewEntities.Customer);
    }

    public async Task<(int TotalCount, List<ReviewShopDto> Reviews)> GetByShopId(
        long shopId, GetReviewOfShopQuery.ReviewFilter filter, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(r => r.ShopId == shopId);

        switch (filter)
        {
            case GetReviewOfShopQuery.ReviewFilter.ContainComment:
                query = query.Where(r => !string.IsNullOrEmpty(r.Comment));
                break;

            case GetReviewOfShopQuery.ReviewFilter.ContainImageAndComment:
                query = query.Where(r =>
                    !string.IsNullOrEmpty(r.Comment) &&
                    !string.IsNullOrEmpty(r.ImageUrl));
                break;

            case GetReviewOfShopQuery.ReviewFilter.All:
            default:
                break;
        }

        var groupedQuery = query
            .GroupBy(r => r.OrderId)
            .Select(g => new ReviewShopDto
            {
                OrderId = g.Key,
                Reviews = g.Select(r => new ReviewShopDto.ReviewDto
                {
                    Id = r.Id,
                    Name = r.Entity == ReviewEntities.Customer ? r.Customer!.Account.FullName : r.Shop!.Name,
                    Avatar = r.Entity == ReviewEntities.Customer ? r.Customer!.Account.AvatarUrl : r.Shop!.LogoUrl,
                    Reviewer = r.Entity,
                    Rating = r.Rating,
                    Comment = r.Comment ?? string.Empty,
                    ImageUrls = string.IsNullOrEmpty(r.ImageUrl)
                        ? new List<string>()
                        : r.ImageUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                }).ToList(),
            });

        var reviews = await groupedQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync().ConfigureAwait(false);

        var totalCount = await groupedQuery.CountAsync().ConfigureAwait(false);

        return (totalCount, reviews);
    }
}