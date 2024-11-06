using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopCategories.Commands.LinkFoods;

public class LinkFoodHandler : ICommandHandler<LinkFoodCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodRepository _foodRepository;
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<LinkFoodHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public LinkFoodHandler(IUnitOfWork unitOfWork, IFoodRepository foodRepository, ILogger<LinkFoodHandler> logger, IShopCategoryRepository shopCategoryRepository, ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _foodRepository = foodRepository;
        _logger = logger;
        _shopCategoryRepository = shopCategoryRepository;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(LinkFoodCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var food = _foodRepository.GetById(request.FoodId);
            food.ShopCategoryId = request.ShopCategoryId;
            _foodRepository.Update(food);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_FOOD_SHOP_CATEGORY_LINK_SUCCESS.GetDescription(), new object[] { request.FoodId, request.ShopCategoryId }),
                Code = MessageCode.I_FOOD_SHOP_CATEGORY_LINK_SUCCESS.GetDescription(),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(LinkFoodCommand request)
    {
        if (_shopCategoryRepository.GetByIdAndShopId(request.ShopCategoryId, _currentPrincipalService.CurrentPrincipalId.Value) == default)
            throw new InvalidBusinessException(MessageCode.E_SHOP_CATEGORY_NOT_FOUND.GetDescription(), new object[] { request.ShopCategoryId }, HttpStatusCode.NotFound);

        if (_foodRepository.Get(f => f.Id == request.FoodId
                                     && f.ShopId == _currentPrincipalService.CurrentPrincipalId && f.Status != FoodStatus.Delete).SingleOrDefault() == default)
        {
            throw new InvalidBusinessException(
                MessageCode.E_FOOD_NOT_FOUND.GetDescription(),
                new object[] { request.FoodId }
            );
        }
    }
}