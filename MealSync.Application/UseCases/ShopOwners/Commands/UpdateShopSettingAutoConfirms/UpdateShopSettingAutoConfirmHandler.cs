using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAcceptOrderNextDays;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirms;

public class UpdateShopSettingAutoConfirmHandler : ICommandHandler<UpdateShopSettingAutoConfirmCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;
    private readonly ILogger<UpdateShopSettingAutoConfirmHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public UpdateShopSettingAutoConfirmHandler(IUnitOfWork unitOfWork, IShopRepository shopRepository, ILogger<UpdateShopSettingAutoConfirmHandler> logger, ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UpdateShopSettingAutoConfirmCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            shop.IsAutoOrderConfirmation = request.IsAutoOrderConfirmation;
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var responseCode = request.IsAutoOrderConfirmation ? MessageCode.I_SHOP_SET_AUTO_CONFIRM_ORDER_SUCCESS.GetDescription() : MessageCode.I_SHOP_SET_NOT_AUTO_CONFIRM_ORDER_SUCCESS.GetDescription();
            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(responseCode),
                Code = responseCode,
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}