using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Customers.Queries.GetCustomerInfo;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Customers.Commands.UpdateProfile;

public class UpdateProfileHandler : ICommandHandler<UpdateProfileCommand, Result>
{

    private readonly IAccountRepository _accountRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICustomerBuildingRepository _customerBuildingRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProfileHandler> _logger;
    private readonly IMediator _mediator;

    public UpdateProfileHandler(
        IAccountRepository accountRepository, IBuildingRepository buildingRepository,
        ICustomerBuildingRepository customerBuildingRepository, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, ILogger<UpdateProfileHandler> logger, IMediator mediator)
    {
        _accountRepository = accountRepository;
        _buildingRepository = buildingRepository;
        _customerBuildingRepository = customerBuildingRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<Result<Result>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var account = _accountRepository.GetById(accountId)!;

        if (account.PhoneNumber != request.PhoneNumber && _accountRepository.CheckExistByPhoneNumber(request.PhoneNumber))
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_PHONE_NUMBER_EXIST.GetDescription());
        }
        else if (request.BuildingId > 0 && _buildingRepository.GetById(request.BuildingId) == default)
        {
            throw new InvalidBusinessException(MessageCode.E_BUILDING_NOT_FOUND.GetDescription(), new object[] { request.BuildingId });
        }
        else
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                account.PhoneNumber = request.PhoneNumber;
                account.FullName = request.FullName;
                account.Genders = request.Genders;
                _accountRepository.Update(account);

                if (request.BuildingId > 0)
                {
                    var defaultCustomerBuilding = _customerBuildingRepository.GetDefaultByCustomerId(accountId)!;
                    if (defaultCustomerBuilding.BuildingId != request.BuildingId)
                    {
                        var presentCustomerBuilding = _customerBuildingRepository.GetByBuildingIdAndCustomerId(request.BuildingId, accountId);
                        if (presentCustomerBuilding == default)
                        {
                            var customerBuilding = new CustomerBuilding
                            {
                                CustomerId = accountId,
                                BuildingId = request.BuildingId,
                                IsDefault = true,
                            };

                            await _customerBuildingRepository.AddAsync(customerBuilding).ConfigureAwait(false);
                        }
                        else
                        {
                            presentCustomerBuilding.IsDefault = true;
                            _customerBuildingRepository.Update(presentCustomerBuilding);
                        }

                        defaultCustomerBuilding.IsDefault = false;
                        _customerBuildingRepository.Update(defaultCustomerBuilding);
                    }
                }

                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw new("Internal Server Error");
            }
        }
        var result = await _mediator.Send(new GetCustomerInfoQuery()).ConfigureAwait(false);
        return Result.Success(result.Value);
    }
}