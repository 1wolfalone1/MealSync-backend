using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroupStatus;

public class UpdateOptionGroupStatusHandler : ICommandHandler<UpdateOptionGroupStatusCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly ILogger<UpdateOptionGroupStatusHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public UpdateOptionGroupStatusHandler(IUnitOfWork unitOfWork, IOptionGroupRepository optionGroupRepository, ILogger<UpdateOptionGroupStatusHandler> logger, ICurrentPrincipalService currentPrincipalService,
        ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _optionGroupRepository = optionGroupRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UpdateOptionGroupStatusCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            var optionGroup = _optionGroupRepository.GetById(request.Id);
            optionGroup.Status = request.Status;
            _optionGroupRepository.Update(optionGroup);
            var message = _systemResourceRepository.GetByResourceCode(MessageCode.I_OPTION_GROUP_UPDATE_STATUS_SUCCESS.GetDescription());
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(new
            {
                Message = message,
                Code = MessageCode.I_OPTION_GROUP_UPDATE_STATUS_SUCCESS.GetDescription(),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(UpdateOptionGroupStatusCommand request)
    {
        var optionGroup = _optionGroupRepository.Get(o => o.Id == request.Id
                                                          && o.ShopId == _currentPrincipalService.CurrentPrincipalId
                                                          && o.Status != OptionGroupStatus.Delete)
            .Include(og => og.Options).SingleOrDefault();
        if (optionGroup == default)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);
    }
}