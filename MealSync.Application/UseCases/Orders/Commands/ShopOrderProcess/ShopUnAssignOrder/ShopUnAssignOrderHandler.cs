using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopUnAssignOrder;

public class ShopUnAssignOrderHandler : ICommandHandler<ShopUnAssignOrderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<ShopUnAssignOrderHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public ShopUnAssignOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, ILogger<ShopUnAssignOrderHandler> logger, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(ShopUnAssignOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var order = _orderRepository.GetById(request.OrderId);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            order.DeliveryPackageId = null;
            _orderRepository.Update(order);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success(new
        {
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_UN_ASSIGN_SUCCESS.GetDescription(), request.OrderId),
            Code = MessageCode.I_ORDER_UN_ASSIGN_SUCCESS.GetDescription(),
        });
    }

    private void Validate(ShopUnAssignOrderCommand request)
    {
        var order = _orderRepository
            .Get(o => o.Id == request.OrderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .Include(o => o.DeliveryPackage).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Preparing && order.Status != OrderStatus.Delivering)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.OrderId });

        if (order.Status == OrderStatus.Delivering)
            throw new InvalidBusinessException(MessageCode.E_ORDER_DELIVERING_NOT_UN_ASSIGN.GetDescription(), new object[] { request.OrderId });

        var startEndDateTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
        if (startEndDateTime.EndTime <= TimeFrameUtils.GetCurrentDateInUTC7().DateTime)
            throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME_TO_ACTION.GetDescription(), new object[] { request.OrderId });
    }
}