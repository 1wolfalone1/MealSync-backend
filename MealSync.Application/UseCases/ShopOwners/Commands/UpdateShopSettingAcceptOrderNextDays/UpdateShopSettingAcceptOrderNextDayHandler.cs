using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopSettingAcceptOrderNextDays;

public class UpdateShopSettingAcceptOrderNextDayHandler : ICommandHandler<UpdateShopSettingAcceptOrderNextDayCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;
    private readonly ILogger<UpdateShopSettingAcceptOrderNextDayHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public UpdateShopSettingAcceptOrderNextDayHandler(IUnitOfWork unitOfWork, IShopRepository shopRepository, ILogger<UpdateShopSettingAcceptOrderNextDayHandler> logger, ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UpdateShopSettingAcceptOrderNextDayCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
            shop.IsAcceptingOrderNextDay = request.IsAcceptingOrderNextDay;
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var responseCode = request.IsAcceptingOrderNextDay ? MessageCode.I_SHOP_ACCEPT_ORDER_NEXT_DAY_SUCCESS.GetDescription() : MessageCode.I_SHOP_NOT_ACCEPT_ORDER_NEXT_DAY_SUCCESS.GetDescription();
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