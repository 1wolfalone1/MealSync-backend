using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAutoConfirmConditions;

public class UpdateShopSettingAutoConfirmConditionHandler : ICommandHandler<UpdateShopSettingAutoConfirmConditionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;
    private readonly ILogger<UpdateShopSettingAutoConfirmConditionHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public UpdateShopSettingAutoConfirmConditionHandler(IUnitOfWork unitOfWork, IShopRepository shopRepository, ILogger<UpdateShopSettingAutoConfirmConditionHandler> logger, ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UpdateShopSettingAutoConfirmConditionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            shop.MaxOrderHoursInAdvance = request.MaxOrderHoursInAdvance;
            shop.MinOrderHoursInAdvance = request.MinOrderHoursInAdvance;
            shop.IsAutoOrderConfirmation = request.IsAutoOrderConfirmation.HasValue ? request.IsAutoOrderConfirmation.Value : shop.IsAutoOrderConfirmation;
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_SET_AUTO_CONFIRM_CONDITION_SUCCESS.GetDescription()),
                Code = MessageCode.I_SHOP_SET_AUTO_CONFIRM_CONDITION_SUCCESS.GetDescription(),
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