using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.AdminManage.Create;

public class AdminCreateFoodPackingUnitHandler : ICommandHandler<AdminCreateFoodPackingUnitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly ILogger<AdminCreateFoodPackingUnitHandler> _logger;
    private readonly IMapper _mapper;

    public AdminCreateFoodPackingUnitHandler(IUnitOfWork unitOfWork, IFoodPackingUnitRepository foodPackingUnitRepository, ILogger<AdminCreateFoodPackingUnitHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(AdminCreateFoodPackingUnitCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var fpu = new FoodPackingUnit()
        {
            Name = request.Name,
            Weight = request.Weight,
            Type = FoodPackingUnitType.System,
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

        return Result.Success(_mapper.Map<FoodPackingUnitAdminResponse>(fpu));
    }

    private void Validate(AdminCreateFoodPackingUnitCommand request)
    {
        if (_foodPackingUnitRepository.Get(x => x.Name == request.Name && x.Type == FoodPackingUnitType.System).FirstOrDefault() != null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_DOUBLE_NAME.GetDescription(), new object[] { request.Name });
        }
    }
}