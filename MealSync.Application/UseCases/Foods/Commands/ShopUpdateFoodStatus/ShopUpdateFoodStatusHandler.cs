using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Foods.Commands.ShopUpdateFoodStatus;

public class ShopUpdateFoodStatusHandler : ICommandHandler<ShopUpdateFoodStatusCommand, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShopUpdateFoodStatusHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IMapper _mapper;

    public ShopUpdateFoodStatusHandler(IFoodRepository foodRepository, IUnitOfWork unitOfWork, ILogger<ShopUpdateFoodStatusHandler> logger, IMapper mapper, ICurrentPrincipalService currentPrincipalService,
        ISystemResourceRepository systemResourceRepository)
    {
        _foodRepository = foodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(ShopUpdateFoodStatusCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var food = _foodRepository.GetById(request.Id);
            food.Status = request.Status;
            _foodRepository.Update(food);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            var messageCode = request.Status == FoodStatus.Active ? MessageCode.I_FOOD_UPDATE_ACTIVE_SUCCESS.GetDescription() : MessageCode.I_FOOD_UPDATE_INACTIVE_SUCCESS.GetDescription();
            return Result.Success(new
            {
                Code = messageCode,
                Message = _systemResourceRepository.GetByResourceCode(messageCode, food.Name),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopUpdateFoodStatusCommand request)
    {
        var food = _foodRepository.Get(f => f.Id == request.Id && f.ShopId == _currentPrincipalService.CurrentPrincipalId && f.Status != FoodStatus.Delete).SingleOrDefault();
        if (food == default)
            throw new InvalidBusinessException(MessageCode.E_FOOD_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
    }
}