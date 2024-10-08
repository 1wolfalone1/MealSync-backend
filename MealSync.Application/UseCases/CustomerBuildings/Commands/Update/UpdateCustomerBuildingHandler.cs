using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.CustomerBuildings.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.CustomerBuildings.Commands.Update;

public class UpdateCustomerBuildingHandler : ICommandHandler<UpdateCustomerBuildingCommand, Result>
{
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCustomerBuildingHandler> _logger;
    private readonly IMapper _mapper;

    public UpdateCustomerBuildingHandler(
        ICustomerBuildingRepository customerBuildingRepository, IBuildingRepository buildingRepository,
        ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork,
        ILogger<UpdateCustomerBuildingHandler> logger, IMapper mapper
    )
    {
        _customerBuildingRepository = customerBuildingRepository;
        _buildingRepository = buildingRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(UpdateCustomerBuildingCommand request, CancellationToken cancellationToken)
    {
        // Check existed building
        var building = _buildingRepository.GetById(request.BuildingId);
        if (building == default)
        {
            throw new InvalidBusinessException(MessageCode.E_BUILDING_NOT_FOUND.GetDescription(), new object[] { request.BuildingId });
        }
        else
        {
            var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
            var defaultCustomerBuilding = _customerBuildingRepository.GetDefaultByCustomerId(accountId);
            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                if (defaultCustomerBuilding != null)
                {
                    defaultCustomerBuilding.IsDefault = false;
                    _customerBuildingRepository.Update(defaultCustomerBuilding);
                }

                var presentCustomerBuilding = _customerBuildingRepository.GetByBuildingIdAndCustomerId(request.BuildingId, accountId);
                if (presentCustomerBuilding != default)
                {
                    presentCustomerBuilding.IsDefault = true;
                    _customerBuildingRepository.Update(presentCustomerBuilding);
                }
                else
                {
                    var customerBuilding = new CustomerBuilding
                    {
                        CustomerId = accountId,
                        BuildingId = request.BuildingId,
                        IsDefault = true,
                    };
                    await _customerBuildingRepository.AddAsync(customerBuilding).ConfigureAwait(false);
                }

                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                return Result.Success(_mapper.Map<CustomerBuildingResponse>(
                    _customerBuildingRepository.GetByBuildingIdAndCustomerIdIncludeBuilding(request.BuildingId, accountId))
                );
            }
            catch (Exception e)
            {
                // Rollback when exception
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
        }
    }
}