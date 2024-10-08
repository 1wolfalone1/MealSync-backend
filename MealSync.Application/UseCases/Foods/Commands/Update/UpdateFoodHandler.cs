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

namespace MealSync.Application.UseCases.Foods.Commands.Update;

public class UpdateFoodHandler : ICommandHandler<UpdateFoodCommand, Result>
{
    private readonly ILogger<UpdateFoodHandler> _logger;
    private readonly IPlatformCategoryRepository _platformCategoryRepository;
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFoodHandler(
        ILogger<UpdateFoodHandler> logger, IPlatformCategoryRepository platformCategoryRepository,
        IShopCategoryRepository shopCategoryRepository, IOperatingSlotRepository operatingSlotRepository,
        IOptionGroupRepository optionGroupRepository, IFoodRepository foodRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper, IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _platformCategoryRepository = platformCategoryRepository;
        _shopCategoryRepository = shopCategoryRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _optionGroupRepository = optionGroupRepository;
        _foodRepository = foodRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Result>> Handle(UpdateFoodCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;

        // Validate request
        await ValidateBusinessRequest(request, accountId).ConfigureAwait(false);

        var food = _foodRepository.GetByIdIncludeAllInfo(request.Id);

        var foodOperatingSlots = new List<FoodOperatingSlot>();
        request.OperatingSlots.ForEach(operatingSlotId =>
        {
            FoodOperatingSlot foodOperatingSlot = new FoodOperatingSlot
            {
                OperatingSlotId = operatingSlotId,
                FoodId = food.Id,
            };
            foodOperatingSlots.Add(foodOperatingSlot);
        });

        var foodOptionGroups = new List<FoodOptionGroup>();
        if (request.FoodOptionGroups != null && request.FoodOptionGroups.Count != 0)
        {
            for (var i = 0; i < request.FoodOptionGroups.Count; i++)
            {
                foodOptionGroups.Add(new FoodOptionGroup
                {
                    OptionGroupId = request.FoodOptionGroups[i],
                    DisplayOrder = i + 1,
                });
            }
        }

        food.Name = request.Name;
        food.Description = request.Description;
        food.Price = request.Price;
        food.ImageUrl = request.ImgUrl;
        food.PlatformCategoryId = request.PlatformCategoryId;
        food.ShopCategoryId = request.ShopCategoryId;
        food.FoodOperatingSlots = foodOperatingSlots;
        food.FoodOptionGroups = foodOptionGroups;

        // Update product
        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            _foodRepository.Update(food);
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

    private async Task ValidateBusinessRequest(UpdateFoodCommand request, long accountId)
    {
        // Check existed food
        var existedFood = await _foodRepository.CheckForUpdateByIdAndShopId(request.Id, accountId).ConfigureAwait(false);
        if (!existedFood)
        {
            throw new InvalidBusinessException(
                MessageCode.E_FOOD_NOT_FOUND.GetDescription(),
                new object[] { request.Id }
            );
        }

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
            request.FoodOptionGroups.ForEach(foodOptionGroupId =>
            {
                var existedOptionGroup = _optionGroupRepository.CheckExistedByIdAndShopId(foodOptionGroupId, accountId);
                if (!existedOptionGroup)
                {
                    throw new InvalidBusinessException(
                        MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(),
                        new object[] { foodOptionGroupId }
                    );
                }
            });
        }
    }
}