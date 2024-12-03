using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reviews.Models;

namespace MealSync.Application.UseCases.Reviews.Queries.GetAllReviewShopWeb;

public class GetAllReviewShopWebHandler : IQueryHandler<GetAllReviewShopWebQuery, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IDapperService _dapperService;

    public GetAllReviewShopWebHandler(IUnitOfWork unitOfWork, IReviewRepository reviewRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService, IDapperService dapperService)
    {
        _unitOfWork = unitOfWork;
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(GetAllReviewShopWebQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _dapperService.SelectAsync<ReviewForShopWebResponse, ReviewForShopWebResponse.CustomerInReview, ReviewForShopWebResponse>(QueryName.GetListReviewForShopWeb,
            (parent, child1) =>
            {
                parent.Customer = child1;
                return parent;
            }
            ,new
            {
                SearchValue = request.SearchValue,
                StatusMode = request.StatusMode,
                DateFrom = request.DateFrom.HasValue ? request.DateFrom.Value.ToString("yyyy-MM-dd") : null,
                DateTo = request.DateTo.HasValue ? request.DateTo.Value.ToString("yyyy-MM-dd") : null,
                ShopId = _currentPrincipalService.CurrentPrincipalId,
                CurrentDate = TimeFrameUtils.GetCurrentDate().ToString("yyyy-MM-dd"),
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
            },
            "CustomerSection").ConfigureAwait(false);

        return Result.Success(new PaginationResponse<ReviewForShopWebResponse>(reviews.ToList(), reviews.FirstOrDefault() != default ? reviews.First().TotalCount : 0, request.PageIndex, request.PageSize));
    }
}