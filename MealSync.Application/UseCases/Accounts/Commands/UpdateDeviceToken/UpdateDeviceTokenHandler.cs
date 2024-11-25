using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Accounts.Commands.UpdateDeviceToken;

public class UpdateDeviceTokenHandler : ICommandHandler<UpdateDeviceTokenCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<UpdateDeviceTokenCommand> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public UpdateDeviceTokenHandler(IUnitOfWork unitOfWork, IAccountRepository accountRepository, ILogger<UpdateDeviceTokenCommand> logger, ICurrentPrincipalService currentPrincipalService)
    {
        _unitOfWork = unitOfWork;
        _accountRepository = accountRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(UpdateDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var account = _accountRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
            account.DeviceToken = request.DeviceToken;
            _accountRepository.Update(account);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success();
    }
}