using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reviews.Queries.GetOrderByOrderId;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reviews.Queries.GetShopReviewByOrderId;

public class GetShopReviewByOrderIdHandler : IQueryHandler<GetShopReviewByOrderIdQuery, Result>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetShopReviewByOrderIdHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetShopReviewByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var reviews = _reviewRepository.GetReviewByOrderId(request.OrderId);
        var now = DateTimeOffset.UtcNow;
        foreach (var review in reviews)
        {
            review.IsAllowShopReply = !review.Reviews.Exists(dto => dto.Reviewer == ReviewEntities.Shop)
                                      && review.Reviews.Count > 0
                                      && now >= review.Reviews[0].CreatedDate
                                      && now <= review.Reviews[0].CreatedDate.AddHours(24);
        }

        return Result.Success(reviews);
    }
}