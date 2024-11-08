using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopDeliveryStaffs.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Commands.Create;

public class CreateDeliveryStaffHandler : ICommandHandler<CreateDeliveryStaffCommand, Result>
{
    private readonly ILogger<CreateDeliveryStaffHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public CreateDeliveryStaffHandler(
        ILogger<CreateDeliveryStaffHandler> logger, IAccountRepository accountRepository,
        IUnitOfWork unitOfWork, ISystemResourceRepository systemResourceRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(CreateDeliveryStaffCommand request, CancellationToken cancellationToken)
    {
        var account = _accountRepository.GetAccountByEmail(request.Email);
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;

        // 1. Existed account
        if (account != null)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_EMAIL_EXIST.GetDescription());
        }
        else
        {
            // 2. Not exist account
            // 2.1 Return an error if the phone number exists.
            if (_accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
            }

            // 2.2 Create a new account.
            var shopDeliveryStaff = new ShopDeliveryStaff
            {
                ShopId = shopId,
                Status = request.ShopDeliveryStaffStatus,
            };
            var newAccount = new Account
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = BCrypUnitls.Hash(request.Password),
                AvatarUrl = _systemResourceRepository.GetByResourceCode(ResourceCode.ACCOUNT_AVATAR.GetDescription()) ?? string.Empty,
                FullName = request.FullName,
                RoleId = (int)Domain.Enums.Roles.ShopDelivery,
                Type = AccountTypes.Local,
                Status = AccountStatus.Verify,
                Genders = Genders.UnKnown,
                ShopDeliveryStaff = shopDeliveryStaff,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                await _accountRepository.AddAsync(newAccount).ConfigureAwait(false);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }

            return Result.Create(_mapper.Map<ShopDeliveryStaffInfoResponse>(newAccount));
        }
    }
}