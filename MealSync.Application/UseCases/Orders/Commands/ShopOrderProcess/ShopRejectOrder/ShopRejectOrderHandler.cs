using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopRejectOrder;

public class ShopRejectOrderHandler : ICommandHandler<ShopRejectOrderCommand, Result>
{
    private readonly INotifierSerivce _notifierSerivce;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ShopRejectOrderHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IShopRepository _shopRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public ShopRejectOrderHandler(INotifierSerivce notifierSerivce, IUnitOfWork unitOfWork, IOrderRepository orderRepository, ILogger<ShopRejectOrderHandler> logger, ICurrentPrincipalService currentPrincipalService, INotificationFactory notificationFactory, IShopRepository shopRepository, ISystemResourceRepository systemResourceRepository)
    {
        _notifierSerivce = notifierSerivce;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _notificationFactory = notificationFactory;
        _shopRepository = shopRepository;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(ShopRejectOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var order = _orderRepository.GetById(request.Id);
            order.Status = OrderStatus.Rejected;
            order.Reason = request.Reason;
            _orderRepository.Update(order);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Send notification to customer
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            var noti = _notificationFactory.CreateOrderRejectedNotification(order, shop);
            _notifierSerivce.NotifyAsync(noti);
            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_REJECT_SUCCESS.GetDescription(), order.Id),
                Code = MessageCode.I_ORDER_REJECT_SUCCESS.GetDescription(),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopRejectOrderCommand request)
    {
        var order = _orderRepository.Get(o => o.Id == request.Id && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Pending)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.Id });
    }
}