using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reviews.Queries.Shop.GetReviewByFilter;

public class GetReviewByFilterHandler : IQueryHandler<GetReviewByFilterQuery, Result>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOrderDetailRepository _orderDetailRepository;

    public GetReviewByFilterHandler(
        IReviewRepository reviewRepository, ICurrentPrincipalService currentPrincipalService, IOrderDetailRepository orderDetailRepository)
    {
        _reviewRepository = reviewRepository;
        _currentPrincipalService = currentPrincipalService;
        _orderDetailRepository = orderDetailRepository;
    }

    public async Task<Result<Result>> Handle(GetReviewByFilterQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var data = await _reviewRepository.GetShopReview(shopId, request.SearchValue, request.Rating, request.Filter, request.PageIndex, request.PageSize).ConfigureAwait(false);
        foreach (var review in data.Reviews)
        {
            review.IsAllowShopReply = !review.Reviews.Exists(dto => dto.Reviewer == ReviewEntities.Shop);
            review.Description = await _orderDetailRepository.GetOrderDescriptionByOrderId(review.OrderId).ConfigureAwait(false);
        }

        var reviewOverview = await _reviewRepository.GetReviewOverviewByShopId(shopId).ConfigureAwait(false);

        var result = new PaginationResponse<ReviewOfShopOwnerDto>(data.Reviews, data.TotalCount, request.PageIndex, request.PageSize);

        return Result.Success(new ReviewOfShopOwnerResponse
        {
            ReviewOverview = reviewOverview,
            Reviews = result,
        });
    }
}