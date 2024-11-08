using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopBanner;

public class UpdateShopBannerHandler : ICommandHandler<UpdateShopBannerCommand, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<UpdateShopBannerHandler> _logger;

    public UpdateShopBannerHandler(
        IShopRepository shopRepository, ISystemResourceRepository systemResourceRepository,
        IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, ILogger<UpdateShopBannerHandler> logger)
    {
        _shopRepository = shopRepository;
        _systemResourceRepository = systemResourceRepository;
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(UpdateShopBannerCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = _shopRepository.GetById(shopId);

        if (shop == default)
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { shopId });
        }
        else
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                shop.BannerUrl = request.BannerUrl;
                _shopRepository.Update(shop);
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
                Code = MessageCode.I_SHOP_UPDATE_BANNER_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_UPDATE_BANNER_SUCCESS.GetDescription()),
                BannerUrl = shop.BannerUrl,
            });
        }
    }
}