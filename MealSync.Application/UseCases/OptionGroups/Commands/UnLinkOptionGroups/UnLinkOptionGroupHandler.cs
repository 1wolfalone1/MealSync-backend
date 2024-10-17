using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UnLinkOptionGroups;

public class UnLinkOptionGroupHandler : ICommandHandler<UnLinkOptionGroupCommand, Result>
{
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnLinkOptionGroupHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public UnLinkOptionGroupHandler(IFoodOptionGroupRepository foodOptionGroupRepository, IUnitOfWork unitOfWork, ILogger<UnLinkOptionGroupHandler> logger, ICurrentPrincipalService currentPrincipalService, IOptionGroupRepository optionGroupRepository, IFoodRepository foodRepository, ISystemResourceRepository systemResourceRepository)
    {
        _foodOptionGroupRepository = foodOptionGroupRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _optionGroupRepository = optionGroupRepository;
        _foodRepository = foodRepository;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UnLinkOptionGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var foodOptionGroup = _foodOptionGroupRepository.Get(og => og.FoodId == request.FoodId && og.OptionGroupId == request.OptionGroupId).SingleOrDefault();
        if (foodOptionGroup == default)
            return Result.Success(new
            {
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.E_FOOD_OPTION_GROUP_ALREADY_UNLINK.GetDescription(), request.FoodId),
                Code = MessageCode.E_FOOD_OPTION_GROUP_ALREADY_UNLINK.GetDescription(),
            });

        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            var foodOptionGroupDelete = _foodOptionGroupRepository.Get(og => og.FoodId == request.FoodId && og.OptionGroupId == request.OptionGroupId).Single();
            _foodOptionGroupRepository.Remove(foodOptionGroupDelete);
            var message = _systemResourceRepository.GetByResourceCode(MessageCode.I_FOOD_OPTION_GROUP_UNLINK_SUCCESS.GetDescription(), request.FoodId);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(new
            {
                Message = message,
                Code = MessageCode.I_FOOD_OPTION_GROUP_UNLINK_SUCCESS.GetDescription(),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(UnLinkOptionGroupCommand request)
    {
        var optionGroup = _optionGroupRepository.Get(og => og.Id == request.OptionGroupId && og.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault();
        if (optionGroup == default)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.OptionGroupId}, HttpStatusCode.NotFound);

        var food = _foodRepository.Get(og => og.Id == request.FoodId && og.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault();
        if (food == default)
            throw new InvalidBusinessException(MessageCode.E_FOOD_NOT_FOUND.GetDescription(), new object[]{request.FoodId}, HttpStatusCode.NotFound);
    }
}