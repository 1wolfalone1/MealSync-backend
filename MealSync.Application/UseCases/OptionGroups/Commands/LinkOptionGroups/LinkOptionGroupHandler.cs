using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OptionGroups.Commands.LinkOptionGroups;

public class LinkOptionGroupHandler : ICommandHandler<LinkOptionGroupCommand, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<LinkOptionGroupCommand> _logger;

    public LinkOptionGroupHandler(ICurrentPrincipalService currentPrincipalService, IFoodOptionGroupRepository foodOptionGroupRepository, IFoodRepository foodRepository, ILogger<LinkOptionGroupCommand> logger, IOptionGroupRepository optionGroupRepository, IUnitOfWork unitOfWork, ISystemResourceRepository systemResourceRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _foodOptionGroupRepository = foodOptionGroupRepository;
        _foodRepository = foodRepository;
        _logger = logger;
        _optionGroupRepository = optionGroupRepository;
        _unitOfWork = unitOfWork;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(LinkOptionGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var maxDisplayOrder = _foodOptionGroupRepository.GetMaxCurrentDisplayOrder(request.FoodId);
            var foodOptionGroup = new FoodOptionGroup()
            {
                FoodId = request.FoodId,
                OptionGroupId = request.OptionGroupId,
                DisplayOrder = maxDisplayOrder + 1,
            };
            await _foodOptionGroupRepository.AddAsync(foodOptionGroup).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var food = _foodRepository.GetById(request.FoodId);
            return Result.Success(new
            {
                Code = MessageCode.I_FOOD_OPTION_GROUP_LINK_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_FOOD_OPTION_GROUP_LINK_SUCCESS.GetDescription(), food.Name),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(LinkOptionGroupCommand request)
    {
        var optionGroup = _optionGroupRepository.Get(og => og.Id == request.OptionGroupId && og.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault();
        if (optionGroup == default)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.OptionGroupId}, HttpStatusCode.NotFound);

        var food = _foodRepository.Get(og => og.Id == request.FoodId && og.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault();
        if (food == default)
            throw new InvalidBusinessException(MessageCode.E_FOOD_NOT_FOUND.GetDescription(), new object[]{request.FoodId}, HttpStatusCode.NotFound);

        var foodOptionGroup = _foodOptionGroupRepository.Get(og => og.FoodId == request.FoodId && og.OptionGroupId == request.OptionGroupId).SingleOrDefault();
        if (foodOptionGroup != default)
            throw new InvalidBusinessException(MessageCode.E_FOOD_OPTION_GROUP_ALREADY_LINK.GetDescription(), new object[]{request.FoodId}, HttpStatusCode.NotFound);
    }
}