using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Foods.Commands.Create;

public class CreateProductHandler : ICommandHandler<CreateFoodCommand, Result>
{
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<Result>> Handle(CreateFoodCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!;

        await ValidateBusinessRequest(request, accountId.Value).ConfigureAwait(false);

        // Create new product
        var shop = await _shopRepository.GetByAccountId(accountId.Value).ConfigureAwait(false);

        List<FoodOperatingSlot> operatingSlots = new List<FoodOperatingSlot>();
        request.OperatingSlots.ForEach(operatingSlotId =>
        {
            operatingSlots.Add(new FoodOperatingSlot
            {
                OperatingSlotId = operatingSlotId,
            });
        });

        List<FoodOptionGroup> foodOptionGroups = new List<FoodOptionGroup>();
        if (request.FoodOptionGroups != null && request.FoodOptionGroups.Count != 0)
        {
            request.FoodOptionGroups.ForEach(foodOptionGroup =>
            {
                foodOptionGroups.Add(new FoodOptionGroup
                {
                    OptionGroupId = foodOptionGroup.OptionGroupId,
                    DisplayOrder = foodOptionGroup.DisplayOrder,
                });
            });
        }

        var food = new Food
        {
            ShopId = shop.Id,
            PlatformCategoryId = request.PlatformCategoryId,
            ShopCategoryId = request.ShopCategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImgUrl,
            TotalOrder = 0,
            IsSoldOut = false,
            Status = FoodStatus.Active,
            FoodOperatingSlots = operatingSlots,
            FoodOptionGroups = foodOptionGroups,
        };

        // Save product
        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _foodRepository.AddAsync(food).ConfigureAwait(false);

            // Update total food of shop
            shop.TotalFood += 1;
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Create(_mapper.Map<FoodDetailResponse>(_foodRepository.GetByIdIncludeAllInfo(food.Id)));
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }

    private async Task ValidateBusinessRequest(CreateFoodCommand request, long accountId)
    {

        // Check existed platform category
        var existedPlatformCategory = _platformCategoryRepository.CheckExistedById(request.PlatformCategoryId);

        if (!existedPlatformCategory)
        {
            throw new InvalidBusinessException(
                MessageCode.E_PLATFORM_CATEGORY_NOT_FOUND.GetDescription(),
                new object[] { request.PlatformCategoryId }
            );
        }

        // Check existed shop category
        var existedShopCategory = _shopCategoryRepository.CheckExistedByIdAndShopId(request.ShopCategoryId, accountId);

        if (!existedShopCategory)
        {
            throw new InvalidBusinessException(
                MessageCode.E_SHOP_CATEGORY_NOT_FOUND.GetDescription(),
                new object[] { request.ShopCategoryId }
            );
        }

        // Check existed operating slots
        request.OperatingSlots.ForEach(id =>
        {
            var operatingSlot = _operatingSlotRepository.GetByIdAndShopId(id, accountId);
            if (operatingSlot == null)
            {
                throw new InvalidBusinessException(
                    MessageCode.E_OPERATING_SLOT_NOT_FOUND.GetDescription(),
                    new object[] { id }
                );
            }
        });

        // Check existed option groups if present
        if (request.FoodOptionGroups != null && request.FoodOptionGroups.Count > 0)
        {
            request.FoodOptionGroups.ForEach(foodOptionGroup =>
            {
                var existedOptionGroup = _optionGroupRepository.CheckExistedByIdAndShopId(foodOptionGroup.OptionGroupId, accountId);
                if (!existedOptionGroup)
                {
                    throw new InvalidBusinessException(
                        MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(),
                        new object[] { foodOptionGroup.OptionGroupId }
                    );
                }
            });
        }
    }
}