using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Favourites.Commands.MarkFavourite;

public class MarkFavouriteHandler : ICommandHandler<MarkFavouriteCommand, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly IFavouriteRepository _favouriteRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<MarkFavouriteHandler> _logger;

    public MarkFavouriteHandler(
        IShopRepository shopRepository, IFavouriteRepository favouriteRepository,
        ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork,
        ISystemResourceRepository systemResourceRepository, ILogger<MarkFavouriteHandler> logger
    )
    {
        _shopRepository = shopRepository;
        _favouriteRepository = favouriteRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(MarkFavouriteCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;

        // Check existed shop
        var shop = _shopRepository.GetById(request.ShopId);
        if (shop == null || (shop.Status != ShopStatus.Active && shop.Status != ShopStatus.InActive))
        {
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
        }
        else
        {
            var favourite = _favouriteRepository.GetByShopIdAndAccountId(request.ShopId, accountId);
            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                if (favourite == null)
                {
                    // Mark favourite shop
                    Favourite newFavourite = new Favourite
                    {
                        CustomerId = accountId,
                        ShopId = request.ShopId,
                    };
                    await _favouriteRepository.AddAsync(newFavourite).ConfigureAwait(false);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                    return Result.Success(new
                    {
                        Message = _systemResourceRepository.GetByResourceCode(
                            MessageCode.E_SHOP_MARK_FAVOURITE.GetDescription(), new object[] { request.ShopId }
                        ),
                    });
                }
                else
                {
                    // Un mark favourite shop
                    _favouriteRepository.Remove(favourite);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                    return Result.Success(new
                    {
                        Message = _systemResourceRepository.GetByResourceCode(
                            MessageCode.E_SHOP_UN_MARK_FAVOURITE.GetDescription(), new object[] { request.ShopId }
                        ),
                    });
                }
            }
            catch (Exception e)
            {
                // Rollback when exception
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
        }
    }
}