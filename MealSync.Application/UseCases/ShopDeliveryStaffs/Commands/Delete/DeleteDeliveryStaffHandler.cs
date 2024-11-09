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

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Delete;

public class DeleteDeliveryStaffHandler : ICommandHandler<DeleteDeliveryStaffCommand, Result>
{
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<DeleteDeliveryStaffHandler> _logger;

    public DeleteDeliveryStaffHandler(
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, ISystemResourceRepository systemResourceRepository,
        IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, ILogger<DeleteDeliveryStaffHandler> logger,
        IDeliveryPackageRepository deliveryPackageRepository)
    {
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _systemResourceRepository = systemResourceRepository;
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _deliveryPackageRepository = deliveryPackageRepository;
    }

    public async Task<Result<Result>> Handle(DeleteDeliveryStaffCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopDeliveryStaff = await _shopDeliveryStaffRepository.GetByIdAndShopId(request.Id, shopId).ConfigureAwait(false);

        if (shopDeliveryStaff == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            if (!request.IsConfirm)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_SHOP_DELIVERY_STAFF_ACCOUNT_TO_DELETED.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_SHOP_DELIVERY_STAFF_ACCOUNT_TO_DELETED.GetDescription(), new object[] { shopDeliveryStaff.Account.FullName! }),
                });
            }
            else
            {
                if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.Online)
                {
                    var haveNotDone = await _deliveryPackageRepository.CheckHaveInDeliveryPackageNotDone(shopDeliveryStaff.Id).ConfigureAwait(false);
                    if (haveNotDone)
                    {
                        throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_IN_DELIVERY_PACKAGE.GetDescription());
                    }
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    shopDeliveryStaff.Account.Status = AccountStatus.Deleted;
                    shopDeliveryStaff.Status = ShopDeliveryStaffStatus.InActive;
                    _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }

                return Result.Success(new
                {
                    Code = MessageCode.I_SHOP_DELIVERY_STAFF_DELETE_SUCCESS.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_DELIVERY_STAFF_DELETE_SUCCESS.GetDescription(), new object[] { shopDeliveryStaff.Account.FullName! }),
                });
            }
        }
    }
}