using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.Update.ShopUpdateFPU;

public class ShopUpdateFPUHandler : ICommandHandler<ShopUpdateFPUCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShopUpdateFPUHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly IMapper _mapper;

    public ShopUpdateFPUHandler(IUnitOfWork unitOfWork, ILogger<ShopUpdateFPUHandler> logger, ICurrentPrincipalService currentPrincipalService, IFoodPackingUnitRepository foodPackingUnitRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopUpdateFPUCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var fpu = _foodPackingUnitRepository.GetById(request.Id);
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            fpu.Name = request.Name;
            fpu.Weight = request.Weight;
            _foodPackingUnitRepository.Update(fpu);
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

    private void Validate(ShopUpdateFPUCommand request)
    {
        if (_foodPackingUnitRepository.Get(fpu => fpu.Id == request.Id && fpu.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault() == null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }

        if (_foodPackingUnitRepository.Get(x => x.Name == request.Name && x.Id != request.Id).FirstOrDefault() != null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_DOUBLE_NAME.GetDescription(), new object[] { request.Name });
        }
    }
}