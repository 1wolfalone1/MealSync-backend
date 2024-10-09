using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OptionGroups.Commands.DeleteOptionGroups;

public class DeleteOptionGroupHandler : ICommandHandler<DeleteOptionGroupCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<DeleteOptionGroupHandler> _logger;

    public DeleteOptionGroupHandler(IUnitOfWork unitOfWork, IOptionGroupRepository optionGroupRepository, IFoodOptionGroupRepository foodOptionGroupRepository, ILogger<DeleteOptionGroupHandler> logger, ICurrentPrincipalService currentPrincipalService, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _optionGroupRepository = optionGroupRepository;
        _foodOptionGroupRepository = foodOptionGroupRepository;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(DeleteOptionGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        if (!request.IsConfirm.Value)
        {
            var foodLinks = _foodOptionGroupRepository.Get(fog => fog.OptionGroupId == request.Id).ToList();
            if (foodLinks.Count > 0)
            {
                var message = _systemResourceRepository.GetByResourceCode(MessageCode.W_OPTION_GROUP_HAVE_FOOD_LINKED.GetDescription(), foodLinks.Count);
                return Result.Warning(new
                {
                    Code = MessageCode.W_OPTION_GROUP_HAVE_FOOD_LINKED.GetDescription(),
                    Message = message,
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var optionGroup = _optionGroupRepository.GetById(request.Id);
            optionGroup.Status = OptionGroupStatus.Delete;
            var foodOptionGroups = _foodOptionGroupRepository.Get(fog => fog.OptionGroupId == request.Id).ToList();
            _foodOptionGroupRepository.RemoveRange(foodOptionGroups);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(new
            {
                Code = MessageCode.I_OPTION_GROUP_DELETE_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_OPTION_GROUP_DELETE_SUCCESS.GetDescription()),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(DeleteOptionGroupCommand request)
    {
        var optionGroup = _optionGroupRepository.Get(og => og.Id == request.Id && og.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault();
        if (optionGroup == default)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.Id}, HttpStatusCode.NotFound);
    }
}