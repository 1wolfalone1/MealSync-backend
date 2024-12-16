using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmOrder;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmListOrder;

public class ShopConfirmListOrderHandler : ICommandHandler<ShopConfirmListOrderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ShopConfirmOrderHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopRepository _shopRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public ShopConfirmListOrderHandler(IUnitOfWork unitOfWork, ILogger<ShopConfirmOrderHandler> logger, IOrderRepository orderRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService, IShopRepository shopRepository, INotificationFactory notificationFactory, INotifierService notifierService, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _orderRepository = orderRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _shopRepository = shopRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(ShopConfirmListOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var orders = _orderRepository.GetByIds(request.Ids.ToList());
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            foreach (var order in orders)
            {
                order.Status = OrderStatus.Confirmed;
            }
            _orderRepository.UpdateRange(orders);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        // Send notification to customer
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
        foreach (var order in orders)
        {
            var noti = _notificationFactory.CreateOrderConfirmNotification(order, shop);
            _notifierService.NotifyAsync(noti);
        }

        return Result.Success(new
        {
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_CONFIRM_SUCCESS.GetDescription(), string.Join(", ", orders.Select(o => o.Id))),
            Code = MessageCode.I_ORDER_CONFIRM_SUCCESS.GetDescription(),
        });
    }

    private void Validate(ShopConfirmListOrderCommand request)
    {
        foreach (var id in request.Ids)
        {
            var order = _orderRepository.Get(o => o.Id == id && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault();
            if (order == default)
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { id }, HttpStatusCode.NotFound);

            if (order.Status != OrderStatus.Pending)
                throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { id});

            var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
            var startEndTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
            if (currentDateTime.DateTime > startEndTime.EndTime)
                throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME.GetDescription(), new object[] { id });
        }
    }
}