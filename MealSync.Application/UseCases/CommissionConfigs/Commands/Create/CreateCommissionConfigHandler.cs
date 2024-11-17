using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.CommissionConfigs.Commands.Create;

public class CreateCommissionConfigHandler : ICommandHandler<CreateCommissionConfigCommand, Result>
{
    private ICommissionConfigRepository _commissionConfigRepository;
    private ISystemResourceRepository _systemResourceRepository;
    private IUnitOfWork _unitOfWork;
    private ILogger<CreateCommissionConfigHandler> _logger;

    public CreateCommissionConfigHandler(
        ICommissionConfigRepository commissionConfigRepository, IUnitOfWork unitOfWork,
        ILogger<CreateCommissionConfigHandler> logger, ISystemResourceRepository systemResourceRepository)
    {
        _commissionConfigRepository = commissionConfigRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(CreateCommissionConfigCommand request, CancellationToken cancellationToken)
    {
        var commissionConfig = new CommissionConfig
        {
            CommissionRate = request.CommissionRate,
        };
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            await _commissionConfigRepository.AddAsync(commissionConfig).ConfigureAwait(false);

            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(new
            {
                Code = MessageCode.I_COMMISSION_CONFIG_UPDATE_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_COMMISSION_CONFIG_UPDATE_SUCCESS.GetDescription()),
                Data = new
                {
                    Id = commissionConfig.Id,
                    CommissionRate = commissionConfig.CommissionRate,
                    CreatedDate = commissionConfig.CreatedDate,
                    UpdatedDate = commissionConfig.UpdatedDate,
                },
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }
}