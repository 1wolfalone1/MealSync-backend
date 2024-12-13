using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Delete;

public class ShopDeletePackingUnitHandler : ICommandHandler<ShopDeletePackingUnitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<ShopDeletePackingUnitHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public ShopDeletePackingUnitHandler(IUnitOfWork unitOfWork, IFoodPackingUnitRepository foodPackingUnitRepository, ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(ShopDeletePackingUnitCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var fpu = _foodPackingUnitRepository.GetById(request.Id);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            _foodPackingUnitRepository.Remove(fpu);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }

        return Result.Success(new
        {
            Code = MessageCode.I_FOOD_PACKING_UNIT_DELETE_SUCCESS.GetDescription(),
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_FOOD_PACKING_UNIT_DELETE_SUCCESS.GetDescription(), fpu.Name),
        });
    }

    private void Validate(ShopDeletePackingUnitCommand request)
    {
        if (_foodPackingUnitRepository.Get(fpu => fpu.Id == request.Id && fpu.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault() == null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
        }

        if (_foodPackingUnitRepository.Get(fpu => fpu.Id == request.Id && fpu.ShopId == _currentPrincipalService.CurrentPrincipalId && fpu.Foods.Count() > 0).SingleOrDefault() != null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_HAVE_FOOD_LINKED.GetDescription(), new object[] { request.Id });
        }
    }
}