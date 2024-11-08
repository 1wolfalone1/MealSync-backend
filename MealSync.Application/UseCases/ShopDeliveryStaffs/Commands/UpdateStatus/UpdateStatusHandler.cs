using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateStatus;

public class UpdateStatusHandler : ICommandHandler<UpdateStatusCommand, Result>
{
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateStatusHandler> _logger;

    public UpdateStatusHandler(
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, ISystemResourceRepository systemResourceRepository,
        IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IMapper mapper, ILogger<UpdateStatusHandler> logger)
    {
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _systemResourceRepository = systemResourceRepository;
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopDeliveryStaff = await _shopDeliveryStaffRepository.GetByIdAndShopId(request.Id, shopId).ConfigureAwait(false);

        if (shopDeliveryStaff == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            if (!request.IsConfirm && request.Status == ShopDeliveryStaffStatus.Online)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_SHOP_DELIVERY_STAFF_STATUS_TO_ONLINE.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_SHOP_DELIVERY_STAFF_STATUS_TO_ONLINE.GetDescription(), new object[] { shopDeliveryStaff.Account.FullName! }),
                });
            }
            else if (!request.IsConfirm && request.Status == ShopDeliveryStaffStatus.Offline)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_SHOP_DELIVERY_STAFF_STATUS_TO_OFFLINE.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_SHOP_DELIVERY_STAFF_STATUS_TO_OFFLINE.GetDescription(), new object[] { shopDeliveryStaff.Account.FullName! }),
                });
            }
            else if (!request.IsConfirm && request.Status == ShopDeliveryStaffStatus.InActive)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_SHOP_DELIVERY_STAFF_STATUS_TO_INACTIVE.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_SHOP_DELIVERY_STAFF_STATUS_TO_INACTIVE.GetDescription(), new object[] { shopDeliveryStaff.Account.FullName! }),
                });
            }
            else
            {
                try
                {
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    shopDeliveryStaff.Status = request.Status;
                    _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }
                return Result.Success(_mapper.Map<ShopDeliveryStaffInfoResponse>(shopDeliveryStaff));
            }
        }
    }
}