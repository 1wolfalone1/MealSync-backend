using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Application.UseCases.FoodPackingUnits.Queries.GetListFoodPackingUnitForShop;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Create.ShopCreate;

public class ShopCreateFoodPackingUnitHandler : ICommandHandler<ShopCreateFoodPackingUnitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<GetListFoodPackingUnitForShopHandler> _logger;
    private readonly IMapper _mapper;

    public ShopCreateFoodPackingUnitHandler(IUnitOfWork unitOfWork, IFoodPackingUnitRepository foodPackingUnitRepository, ICurrentPrincipalService currentPrincipalService, ILogger<GetListFoodPackingUnitForShopHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _currentPrincipalService = currentPrincipalService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopCreateFoodPackingUnitCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var fpu = new FoodPackingUnit()
        {
            Name = request.Name,
            Weight = request.Weight,
            Type = FoodPackingUnitType.Shop,
            ShopId = _currentPrincipalService.CurrentPrincipalId,
        };
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _foodPackingUnitRepository.AddAsync(fpu).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success(_mapper.Map<FoodPackingUnitResponse>(fpu));
    }

    private void Validate(ShopCreateFoodPackingUnitCommand request)
    {
        if (_foodPackingUnitRepository.Get(x => x.Name == request.Name).FirstOrDefault() != null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_DOUBLE_NAME.GetDescription(), new object[] { request.Name });
        }
    }
}