using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reviews.Commands.ShopReplyReviewOfCustomers;
using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Reviews.Commands.ShopReplyOrderOfCustomer;

public class ShopReplyReviewOfCustomerHandler : ICommandHandler<ShopReplyReviewOfCustomerCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<ShopReplyReviewOfCustomerHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public ShopReplyReviewOfCustomerHandler(IUnitOfWork unitOfWork, IReviewRepository reviewRepository, ILogger<ShopReplyReviewOfCustomerHandler> logger, ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _reviewRepository = reviewRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopReplyReviewOfCustomerCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);

        Review review = new Review
        {
            ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
            OrderId = request.OrderId,
            Rating = RatingRanges.FiveStar,
            Comment = request.Comment,
            ImageUrl = string.Join(",", request.ImageUrls),
            Entity = ReviewEntities.Shop,
        };

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _reviewRepository.AddAsync(review).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Create(_mapper.Map<ReviewDetailResponse>(review));
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }

    private async Task ValidateAsync(ShopReplyReviewOfCustomerCommand request)
    {
        var order = _orderRepository.Get(x => x.Id == request.OrderId && x.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault();
        if (order == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId });
        }
        else if (!await _reviewRepository.CheckExistedReviewOfCustomerByOrderId(request.OrderId).ConfigureAwait(false))
        {
            throw new InvalidBusinessException(MessageCode.E_REVIEW_CUSTOMER_NOT_REVIEW_YET.GetDescription(), new object[] { request.OrderId });
        }
        else if (_reviewRepository.Get(rv => rv.OrderId == request.OrderId && rv.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault() != default)
        {
            throw new InvalidBusinessException(MessageCode.E_REVIEW_SHOP_ALREADY_REVIEW.GetDescription(), new object[] { request.OrderId });
        }

        // Shop only have 24h after customer review
        var customerReview = _reviewRepository.Get(rv => rv.OrderId == request.OrderId && rv.Entity == ReviewEntities.Customer).FirstOrDefault();
        TimeSpan difference = TimeFrameUtils.GetCurrentDate().Date - customerReview.CreatedDate.Date;
        if (customerReview != null && difference.TotalHours > 24)
            throw new InvalidBusinessException(MessageCode.E_REVIEW_SHOP_OVER_TIME_REPLY.GetDescription(), new object[]{ request.OrderId });

    }
}