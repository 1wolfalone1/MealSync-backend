using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Queries.OrderHistory;

public class GetOrderHistoryHandler : IQueryHandler<GetOrderHistoryQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IDapperService _dapperService;
    private readonly IMapper _mapper;

    public GetOrderHistoryHandler(IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper, IDapperService dapperService)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;

        // var data = await _orderRepository.GetByCustomerIdAndStatus(customerId, request.StatusList, request.ReviewMode, request.PageIndex, request.PageSize)
        // .ConfigureAwait(false);

        var orders = await _dapperService.SelectAsync<OrderSummaryDto>(QueryName.GetOrderHistoryOfCustomer, new
        {
            CustomerId = customerId,
            DeliveredReportedByCustomer = OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription(),
            FilterStatusList = request.StatusList != default && request.StatusList.Length > 0
                ? request.StatusList
                : new OrderStatus[]
                {
                    OrderStatus.Pending,
                    OrderStatus.Rejected,
                    OrderStatus.Confirmed,
                    OrderStatus.Cancelled,
                    OrderStatus.Preparing,
                    OrderStatus.Delivering,
                    OrderStatus.Delivered,
                    OrderStatus.FailDelivery,
                    OrderStatus.Completed,
                    OrderStatus.IssueReported,
                    OrderStatus.UnderReview,
                    OrderStatus.Resolved,
                },
            ReviewMode = request.ReviewMode,
            Now = now,
            PageSize = request.PageSize,
            Offset = (request.PageIndex - 1) * request.PageSize,
        }).ConfigureAwait(false);

        var result = new PaginationResponse<OrderSummaryResponse>(
            _mapper.Map<List<OrderSummaryResponse>>(orders), orders.Any() ? orders.First().TotalCount : 0, request.PageIndex, request.PageSize);

        return Result.Success(result);
    }
}