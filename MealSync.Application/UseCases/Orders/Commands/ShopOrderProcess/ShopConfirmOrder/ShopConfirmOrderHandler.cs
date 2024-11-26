using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopConfirmOrder;

public class ShopConfirmOrderHandler : ICommandHandler<ShopConfirmOrderCommand, Result>
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

    public ShopConfirmOrderHandler(IUnitOfWork unitOfWork, ILogger<ShopConfirmOrderHandler> logger, IOrderRepository orderRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService, IShopRepository shopRepository, INotificationFactory notificationFactory, INotifierService notifierService, ISystemResourceRepository systemResourceRepository)
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

    public async Task<Result<Result>> Handle(ShopConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var order = _orderRepository.GetById(request.Id);
            order.Status = OrderStatus.Confirmed;
            _orderRepository.Update(order);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Send notification to customer
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            var noti = _notificationFactory.CreateOrderConfirmNotification(order, shop);
            _notifierService.NotifyAsync(noti);
            return Result.Success(new
            {
                OrderId = order.Id,
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_CONFIRM_SUCCESS.GetDescription(), order.Id),
                Code = MessageCode.I_ORDER_CONFIRM_SUCCESS.GetDescription(),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopConfirmOrderCommand request)
    {
        var order = _orderRepository.Get(o => o.Id == request.Id && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Pending)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.Id });

        var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
        var startEndTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
        if (currentDateTime.DateTime > startEndTime.EndTime)
            throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME.GetDescription(), new object[] { request.Id });
    }
}