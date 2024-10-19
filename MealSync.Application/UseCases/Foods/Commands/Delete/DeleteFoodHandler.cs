using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Foods.Commands.Delete;

public class DeleteFoodHandler : ICommandHandler<DeleteFoodCommand, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IFoodRepository _foodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<DeleteFoodHandler> _logger;

    public DeleteFoodHandler(ILogger<DeleteFoodHandler> logger, IFoodRepository foodRepository, ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _foodRepository = foodRepository;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Result>> Handle(DeleteFoodCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var food = _foodRepository.Get(f => f.Id == request.Id)
                .Include(f => f.FoodOperatingSlots)
                .Include(f => f.FoodOptionGroups).SingleOrDefault();
            food.Status = FoodStatus.Delete;
            food.FoodOperatingSlots = null;
            food.FoodOptionGroups = null;
            food.ShopCategoryId = null;
            _foodRepository.Update(food);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(new
            {
                Code = MessageCode.I_FOOD_DELETE_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_FOOD_DELETE_SUCCESS.GetDescription(), food.Name),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(DeleteFoodCommand request)
    {
        if (_foodRepository.Get(f => f.Id == request.Id
                                     && f.ShopId == _currentPrincipalService.CurrentPrincipalId && f.Status != FoodStatus.Delete).SingleOrDefault() == default)
        {
            throw new InvalidBusinessException(
                MessageCode.E_FOOD_NOT_FOUND.GetDescription(),
                new object[] { request.Id }
            );
        }
    }
}