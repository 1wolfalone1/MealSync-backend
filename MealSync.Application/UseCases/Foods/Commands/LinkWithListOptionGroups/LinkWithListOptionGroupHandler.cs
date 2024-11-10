using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Foods.Commands.LinkWithListOptionGroups;

public class LinkWithListOptionGroupHandler : ICommandHandler<LinkWithListOptionGroupCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodRepository _foodRepository;
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;
    private readonly ILogger<LinkWithListOptionGroupHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public LinkWithListOptionGroupHandler(IUnitOfWork unitOfWork, IFoodRepository foodRepository, IFoodOptionGroupRepository foodOptionGroupRepository, ILogger<LinkWithListOptionGroupHandler> logger, ICurrentPrincipalService currentPrincipalService, IOptionGroupRepository optionGroupRepository, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _foodRepository = foodRepository;
        _foodOptionGroupRepository = foodOptionGroupRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _optionGroupRepository = optionGroupRepository;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(LinkWithListOptionGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var foodOptionGroups = new List<FoodOptionGroup>();
            var maxDisplayOrder = _foodOptionGroupRepository.GetMaxCurrentDisplayOrder(request.FoodId);
            foreach (var optionGroupId in request.OptionGroupIds)
            {
                maxDisplayOrder++;
                foodOptionGroups.Add(new FoodOptionGroup()
                {
                    FoodId = request.FoodId,
                    OptionGroupId = optionGroupId,
                    DisplayOrder = maxDisplayOrder,
                });
            }

            await _foodOptionGroupRepository.AddRangeAsync(foodOptionGroups).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        var food = _foodRepository.GetById(request.FoodId);
        return Result.Success(new
        {
            Code = MessageCode.I_FOOD_OPTION_GROUP_LINK_SUCCESS.GetDescription(),
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_FOOD_OPTION_GROUP_LINK_SUCCESS.GetDescription(), food.Name),
        });
    }

    private async Task ValidateAsync(LinkWithListOptionGroupCommand request)
    {
        // Check existed food
        var existedFood = await _foodRepository.CheckExistedByIdAndShopId(request.FoodId, _currentPrincipalService.CurrentPrincipalId.Value).ConfigureAwait(false);
        if (!existedFood)
        {
            throw new InvalidBusinessException(
                MessageCode.E_FOOD_NOT_FOUND.GetDescription(),
                new object[] { request.FoodId }
            );
        }

        // Check existed option groups if present
        if (request.OptionGroupIds != null && request.OptionGroupIds.Length > 0)
        {
            foreach (var optionGroupId in request.OptionGroupIds)
            {
                var existedOptionGroup = _optionGroupRepository.CheckExistedByIdAndShopId(optionGroupId, _currentPrincipalService.CurrentPrincipalId.Value);
                if (!existedOptionGroup)
                {
                    throw new InvalidBusinessException(
                        MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(),
                        new object[] { optionGroupId }
                    );
                }

                if (_foodOptionGroupRepository.Get(fog => fog.FoodId == request.FoodId && fog.OptionGroupId == optionGroupId).SingleOrDefault() != default)
                    throw new InvalidBusinessException(MessageCode.E_FOOD_OPTION_GROUP_ALREADY_LINK.GetDescription(), new object[] { request.FoodId });
            }
        }
    }
}