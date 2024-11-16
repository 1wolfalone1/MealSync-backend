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

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.UpdateInfoForStaff;

public class UpdateInfoForStaffHandler : ICommandHandler<UpdateInfoForStaffCommand, Result>
{
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateInfoForStaffHandler> _logger;

    public UpdateInfoForStaffHandler(
        IShopDeliveryStaffRepository shopDeliveryStaffRepository, IAccountRepository accountRepository,
        IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService,
        IMapper mapper, ILogger<UpdateInfoForStaffHandler> logger)
    {
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(UpdateInfoForStaffCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var account = _accountRepository.GetById(accountId)!;

        if (request.PhoneNumber != account.PhoneNumber)
        {
            if (_accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
            }
            else
            {
                account.PhoneNumber = request.PhoneNumber;
            }
        }

        if (!string.IsNullOrEmpty(request.AvatarUrl))
        {
            account.AvatarUrl = request.AvatarUrl;
        }

        account.FullName = request.FullName;
        account.Genders = request.Gender;

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            _accountRepository.Update(account);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            var shopDeliveryStaff = await _shopDeliveryStaffRepository.GetByIdIncludeAccount(accountId).ConfigureAwait(false);
            return Result.Success(_mapper.Map<ShopDeliveryStaffInfoResponse>(shopDeliveryStaff));
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }
}