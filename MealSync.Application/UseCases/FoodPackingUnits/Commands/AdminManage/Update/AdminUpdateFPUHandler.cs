using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.FoodPackingUnits.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.FoodPackingUnits.Commands.AdminManage.Update;

public class AdminUpdateFPUHandler : ICommandHandler<AdminUpdateFPUCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminUpdateFPUHandler> _logger;
    private readonly IFoodPackingUnitRepository _foodPackingUnitRepository;
    private readonly IMapper _mapper;

    public AdminUpdateFPUHandler(IUnitOfWork unitOfWork, ILogger<AdminUpdateFPUHandler> logger, IFoodPackingUnitRepository foodPackingUnitRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _foodPackingUnitRepository = foodPackingUnitRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(AdminUpdateFPUCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var fpu = _foodPackingUnitRepository.Get(fpu => fpu.Id == request.Id)
            .Include(fpu => fpu.Foods).Single();
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

        return Result.Success(_mapper.Map<FoodPackingUnitAdminResponse>(fpu));
    }

    private void Validate(AdminUpdateFPUCommand request)
    {
        if (_foodPackingUnitRepository.Get(x => x.Type == FoodPackingUnitType.System && x.Id == request.Id).FirstOrDefault() == null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
        }

        if (_foodPackingUnitRepository.Get(x => x.Name == request.Name && x.Type == FoodPackingUnitType.System && x.Id != request.Id).FirstOrDefault() != null)
        {
            throw new InvalidBusinessException(MessageCode.E_FOOD_PACKING_UNIT_DOUBLE_NAME.GetDescription(), new object[] { request.Name });
        }
    }
}