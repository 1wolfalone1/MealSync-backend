using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateAvatar;

public class UpdateAvatarHandler : ICommandHandler<UpdateAvatarCommand, Result>
{
    private readonly IStorageService _storageService;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAvatarHandler> _logger;

    public UpdateAvatarHandler(
        IStorageService storageService, IAccountRepository accountRepository,
        ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork,
        ILogger<UpdateAvatarHandler> logger, ISystemResourceRepository systemResourceRepository)
    {
        _storageService = storageService;
        _accountRepository = accountRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var url = await _storageService.UploadFileAsync(request.File).ConfigureAwait(false);
        var account = _accountRepository.GetById(accountId);
        account!.AvatarUrl = url;

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            _accountRepository.Update(account);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(new
            {
                Code = MessageCode.I_SHOP_UPDATE_AVATAR_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_UPDATE_AVATAR_SUCCESS.GetDescription()),
                AvatarUrl = url,
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }
}