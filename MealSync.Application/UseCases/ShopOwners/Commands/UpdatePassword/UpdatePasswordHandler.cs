using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdatePassword;

public class UpdatePasswordHandler : ICommandHandler<UpdatePasswordCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<UpdatePasswordHandler> _logger;

    public UpdatePasswordHandler(
        IAccountRepository accountRepository, ISystemResourceRepository systemResourceRepository,
        IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService,
        ILogger<UpdatePasswordHandler> logger)
    {
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var account = _accountRepository.GetById(shopId);

        if (account == default || !BCrypUnitls.Verify(request.OldPassword, account.Password))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_INVALID_OLD_PASSWORD.GetDescription());
        }
        else
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                account.Password = BCrypUnitls.Hash(request.NewPassword);
                _accountRepository.Update(account);
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
                Code = MessageCode.I_ACCOUNT_UPDATE_PASSWORD_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ACCOUNT_UPDATE_PASSWORD_SUCCESS.GetDescription()),
            });
        }
    }
}