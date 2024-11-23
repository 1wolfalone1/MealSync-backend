using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Orders.Queries.ModeratorManage.GetOrderForModerator;

public class GetOrderForModeratorHandler : IQueryHandler<GetOrderForModeratorQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetOrderForModeratorHandler(IOrderRepository orderRepository, IMapper mapper, IModeratorDormitoryRepository moderatorDormitoryRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetOrderForModeratorQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var statusList = new List<OrderStatus>();
        switch (request.StatusMode)
        {
            case 1:
                statusList = new List<OrderStatus>()
                {
                    OrderStatus.Delivered, // 7
                    OrderStatus.FailDelivery, // 8
                    OrderStatus.Completed, // 9
                    OrderStatus.Resolved, // 12
                };
                break;
            case 2:
                statusList = new List<OrderStatus>()
                {
                    OrderStatus.Pending, // 1
                    OrderStatus.Confirmed, // 3
                    OrderStatus.Preparing, // 5
                    OrderStatus.Delivering, // 6
                };
                break;
            case 3:
                statusList = new List<OrderStatus>()
                {
                    OrderStatus.IssueReported, // 10
                    OrderStatus.UnderReview, // 11
                };
                break;
            case 4:
                statusList = new List<OrderStatus>()
                {
                    OrderStatus.Rejected, // 2
                    OrderStatus.Cancelled, // 4
                };
                break;
        }

        var dormitoryList = new List<long>();
        if (request.DormitoryMode == 0)
        {
            var modDormitory = _moderatorDormitoryRepository.Get(mod => mod.ModeratorId == _currentPrincipalService.CurrentPrincipalId).ToList();
            dormitoryList = modDormitory.Select(o => o.DormitoryId).ToList();
        }
        else
        {
            dormitoryList.Add(request.DormitoryMode);
        }

        var orderPaging = _orderRepository.GetOrderForModerator(request.SearchValue, request.DateFrom, request.DateTo, statusList, dormitoryList, request.PageIndex, request.PageSize);
        var order = _mapper.Map<List<OrderListForModeratorResponse>>(orderPaging.Orders);
        return Result.Success(new PaginationResponse<OrderListForModeratorResponse>(order, orderPaging.TotalCount, request.PageIndex, request.PageSize));
    }

    private void Validate(GetOrderForModeratorQuery request)
    {
        if (request.DormitoryMode != 0)
        {
            var modDormitory = _moderatorDormitoryRepository.Get(mod => mod.ModeratorId == _currentPrincipalService.CurrentPrincipalId).ToList();
            if (!modDormitory.Any(mod => mod.DormitoryId == request.DormitoryMode))
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_DORMITORY_NOT_IN_DORMITORY_ACCESS.GetDescription(), new object[] { request.DormitoryMode });
        }
    }
}