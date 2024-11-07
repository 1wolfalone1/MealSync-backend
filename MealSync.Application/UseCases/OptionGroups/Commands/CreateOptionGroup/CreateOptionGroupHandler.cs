using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OptionGroups.Commands.CreateOptionGroup;

public class CreateOptionGroupHandler : ICommandHandler<CreateOptionGroupCommand, Result>
{
    private readonly ILogger<CreateOptionGroupHandler> _logger;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateOptionGroupHandler(
        ILogger<CreateOptionGroupHandler> logger, IOptionGroupRepository optionGroupRepository,
        ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork, IMapper mapper
    )
    {
        _logger = logger;
        _optionGroupRepository = optionGroupRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(CreateOptionGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate request
        var optionGroupCheck = _optionGroupRepository.CheckExistTitleOptionGroup(request.Title, _currentPrincipalService.CurrentPrincipalId!.Value);
        if (optionGroupCheck)
        {
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_DOUBLE_TITLE.GetDescription(), new object[] { request.Title }, HttpStatusCode.Conflict);
        }

        if (request.Type == OptionGroupTypes.Radio && request.IsRequire)
        {
            // Must have only 1 option default
            var totalDefault = request.Options.Count(o => o.IsDefault);
            if (totalDefault != 1)
            {
                throw new InvalidBusinessException(
                    MessageCode.E_OPTION_GROUP_RADIO_REQUIRED_VALIDATE.GetDescription()
                );
            }
        }

        if (request.Type == OptionGroupTypes.CheckBox && request.IsRequire)
        {
            // Must have only 1 option default
            var totalDefault = request.Options.Count(o => o.IsDefault);
            if (totalDefault < request.MinChoices || totalDefault > request.MaxChoices)
            {
                throw new InvalidBusinessException(
                    MessageCode.E_OPTION_GROUP_CHECKBOX_REQUIRED_VALIDATE.GetDescription(),
                    new object[] { request.MinChoices, request.MaxChoices }
                );
            }
        }

        if (!request.IsRequire)
        {
            // Must have only 0 option default
            var totalDefault = request.Options.Count(o => o.IsDefault);
            if (totalDefault != 0)
            {
                throw new InvalidBusinessException(
                    MessageCode.E_OPTION_GROUP_NOT_REQUIRED_VALIDATE.GetDescription()
                );
            }
        }

        var accountId = _currentPrincipalService.CurrentPrincipalId!;
        List<Option> options = new List<Option>();

        request.Options.ForEach(option =>
        {
            options.Add(new Option
            {
                IsDefault = option.IsDefault,
                Title = option.Title,
                IsCalculatePrice = option.IsCalculatePrice,
                Price = option.Price,
                ImageUrl = option.ImageUrl,
                Status = option.Status,
            });
        });

        OptionGroup optionGroup = new OptionGroup
        {
            ShopId = accountId.Value,
            Title = request.Title,
            IsRequire = request.IsRequire,
            Type = request.Type,
            Status = request.Status,
            MinChoices = request.MinChoices,
            MaxChoices = request.MaxChoices,
            Options = options,
        };

        // Save option group
        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _optionGroupRepository.AddAsync(optionGroup).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Create(_mapper.Map<OptionGroupResponse>(_optionGroupRepository.GetByIdIncludeOption(optionGroup.Id)));
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