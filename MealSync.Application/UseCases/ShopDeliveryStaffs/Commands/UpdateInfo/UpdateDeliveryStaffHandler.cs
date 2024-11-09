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

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateInfo;

public class UpdateDeliveryStaffHandler : ICommandHandler<UpdateDeliveryStaffCommand, Result>
{
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateDeliveryStaffHandler> _logger;

    public UpdateDeliveryStaffHandler(
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, IAccountRepository accountRepository,
        IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService,
        IMapper mapper, ILogger<UpdateDeliveryStaffHandler> logger, IDeliveryPackageRepository deliveryPackageRepository)
    {
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _logger = logger;
        _deliveryPackageRepository = deliveryPackageRepository;
    }

    public async Task<Result<Result>> Handle(UpdateDeliveryStaffCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shopDeliveryStaff = await _shopDeliveryStaffRepository.GetByIdAndShopId(request.Id, shopId).ConfigureAwait(false);

        if (shopDeliveryStaff == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            if (request.Email != shopDeliveryStaff.Account.Email)
            {
                var account = _accountRepository.GetAccountByEmail(request.Email);
                if (account != null)
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription());
                }
                else
                {
                    shopDeliveryStaff.Account.Email = request.Email;
                }
            }

            if (request.PhoneNumber != shopDeliveryStaff.Account.PhoneNumber)
            {
                if (_accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
                {
                    throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
                }
                else
                {
                    shopDeliveryStaff.Account.PhoneNumber = request.PhoneNumber;
                }
            }

            if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.Online && request.Status != ShopDeliveryStaffStatus.Online)
            {
                var haveNotDone = await _deliveryPackageRepository.CheckHaveInDeliveryPackageNotDone(shopDeliveryStaff.Id).ConfigureAwait(false);
                if (haveNotDone)
                {
                    throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_IN_DELIVERY_PACKAGE.GetDescription());
                }
            }

            shopDeliveryStaff.Account.FullName = request.FullName;
            shopDeliveryStaff.Account.Genders = request.Gender;
            shopDeliveryStaff.Account.AvatarUrl = request.AvatarUrl;
            shopDeliveryStaff.Status = request.Status;

            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
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