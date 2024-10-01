using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Rearrange;

public class RearrangeShopCategoryHandler : ICommandHandler<RearrangeShopCategoryCommand, Result>
{
    private readonly ILogger<RearrangeShopCategoryHandler> _logger;
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RearrangeShopCategoryHandler(
        ILogger<RearrangeShopCategoryHandler> logger, IShopCategoryRepository shopCategoryRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper, IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _shopCategoryRepository = shopCategoryRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Result>> Handle(RearrangeShopCategoryCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!;
        List<ShopCategory> shopCategoryUpdateList = new List<ShopCategory>();

        // Ensure all requested IDs exist in the database for the current shop
        var shopCategoriesInDb = _shopCategoryRepository.GetAllByShopId(accountId.Value);
        var shopCategoryIdsInDb = shopCategoriesInDb.Select(sc => sc.Id).ToList();

        // Get the missing IDs that are present in the request but not in the DB
        var missingIds = request.Ids.Except(shopCategoryIdsInDb).ToList();

        if (missingIds.Any())
        {
            throw new InvalidBusinessException(
                MessageCode.E_SHOP_CATEGORY_NOT_FOUND.GetDescription(),
                new object[] { string.Join(", ", missingIds) }
            );
        }
        else if (shopCategoriesInDb.Count != request.Ids.Count)
        {
            throw new InvalidBusinessException(
                MessageCode.E_SHOP_CATEGORY_NOT_ENOUGH.GetDescription()
            );
        }
        else
        {
            // Update order shop category
            for (var i = 0; i < request.Ids.Count; i++)
            {
                var shopCategory = shopCategoriesInDb.First(sc => sc.Id == request.Ids[i]);
                shopCategory.DisplayOrder = i + 1;
                shopCategoryUpdateList.Add(shopCategory);
            }

            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                _shopCategoryRepository.UpdateRange(shopCategoryUpdateList);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                return Result.Create(_mapper.Map<List<ShopCategoryResponse>>(_shopCategoryRepository.GetAllByShopId(accountId.Value)));
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