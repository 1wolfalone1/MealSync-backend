using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reviews.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Reviews.Commands.ReviewOrderOfCustomer;

public class ReviewOrderOfCustomerHandler : ICommandHandler<ReviewOrderOfCustomerCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReviewOrderOfCustomerHandler> _logger;
    private readonly IMapper _mapper;

    public ReviewOrderOfCustomerHandler(
        IOrderRepository orderRepository, IReviewRepository reviewRepository, IShopRepository shopRepository,
        ICurrentPrincipalService currentPrincipalService, IStorageService storageService,
        IUnitOfWork unitOfWork, ILogger<ReviewOrderOfCustomerHandler> logger, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _reviewRepository = reviewRepository;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ReviewOrderOfCustomerCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var order = await _orderRepository.GetByIdAndCustomerId(request.OrderId, customerId).ConfigureAwait(false);
        if (order == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId });
        }
        else if (await _reviewRepository.CheckExistedReviewOfCustomerByOrderId(request.OrderId).ConfigureAwait(false))
        {
            throw new InvalidBusinessException(MessageCode.E_REVIEW_CUSTOMER_ALREADY_REVIEW.GetDescription(), new object[] { request.OrderId });
        }
        else
        {
            if (
                order.Status == OrderStatus.Delivered || order.Status == OrderStatus.IssueReported ||
                order.Status == OrderStatus.UnderReview || order.Status == OrderStatus.Resolved || order.Status == OrderStatus.Completed)
            {
                var now = DateTimeOffset.UtcNow;
                DateTime receiveDate;
                if (order.EndTime == 2400)
                {
                    receiveDate = new DateTime(
                            order.IntendedReceiveDate.Year,
                            order.IntendedReceiveDate.Month,
                            order.IntendedReceiveDate.Day,
                            0,
                            0,
                            0)
                        .AddDays(1);
                }
                else
                {
                    receiveDate = new DateTime(
                        order.IntendedReceiveDate.Year,
                        order.IntendedReceiveDate.Month,
                        order.IntendedReceiveDate.Day,
                        order.EndTime / 100,
                        order.EndTime % 100,
                        0);
                }

                var endTime = new DateTimeOffset(receiveDate, TimeSpan.FromHours(7));

                // BR: Review within 24 hours after the 'endTime'
                if (now >= endTime && now <= endTime.AddHours(24))
                {
                    var imageUrls = new List<string>();
                    if (request.Images != default)
                    {
                        foreach (var file in request.Images)
                        {
                            var url = await _storageService.UploadFileAsync(file).ConfigureAwait(false);
                            imageUrls.Add(url);
                        }
                    }

                    Review review = new Review
                    {
                        CustomerId = customerId,
                        ShopId = order.ShopId,
                        OrderId = request.OrderId,
                        Rating = request.Rating,
                        Comment = request.Comment,
                        ImageUrl = string.Join(",", imageUrls),
                        Entity = ReviewEntities.Customer,
                    };

                    var shop = await _shopRepository.GetByAccountId(order.ShopId).ConfigureAwait(false);
                    shop.TotalReview += 1;
                    shop.TotalRating += (int)request.Rating;

                    try
                    {
                        // Begin transaction
                        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                        await _reviewRepository.AddAsync(review).ConfigureAwait(false);
                        _shopRepository.Update(shop);
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
                else
                {
                    throw new InvalidBusinessException(MessageCode.E_REVIEW_TIME_LIMIT.GetDescription());
                }
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_REVIEW_UNAVAILABLE.GetDescription());
            }
        }
    }
}