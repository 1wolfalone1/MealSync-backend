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
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OptionGroups.Commands.UpdateOptionGroups;

public class UpdateOptionGroupHandler : ICommandHandler<UpdateOptionGroupCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IOptionRepository _optionRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<UpdateOptionGroupCommand> _logger;

    public UpdateOptionGroupHandler(IUnitOfWork unitOfWork, IOptionGroupRepository optionGroupRepository, IMapper mapper, ILogger<UpdateOptionGroupCommand> logger, ICurrentPrincipalService currentPrincipalService, IOptionRepository optionRepository)
    {
        _unitOfWork = unitOfWork;
        _optionGroupRepository = optionGroupRepository;
        _mapper = mapper;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _optionRepository = optionRepository;
    }

    public async Task<Result<Result>> Handle(UpdateOptionGroupCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
        try
        {
            var optionGroup = _optionGroupRepository.GetByIdIncludeOption(request.Id);
            var listOptionIds = request.Options.Select(o => o.Id).ToList();
            var listOptionOrigin = optionGroup.Options.Select(o => o.Id).ToList();
            // List for update (IDs that are the same in both lists)
            var listUpdateIds = listOptionIds.Intersect(listOptionOrigin).ToList();

            // List for delete (IDs in original list but missing from new list)
            var listDeleteIds = listOptionOrigin.Except(listOptionIds).ToList();

            foreach (var option in optionGroup.Options)
            {
                // Update
                if (listUpdateIds.Contains(option.Id))
                {
                    var optionUpdate = request.Options.Single(o => o.Id == option.Id);
                    option.IsDefault = request.IsRequire ? optionUpdate.IsDefault : false;
                    option.Title = optionUpdate.Title;
                    option.Status = optionUpdate.Status;
                    option.IsCalculatePrice = optionUpdate.IsCalculatePrice;
                    option.Price = optionUpdate.Price;
                    option.ImageUrl = optionUpdate.ImageUrl;
                }

                // Delete
                if (listDeleteIds.Contains(option.Id))
                {
                    option.Status = OptionStatus.Delete;
                }
            }

            // Add
            foreach (var optionAdd in request.Options.Where(o => o.Id == 0).ToList())
            {
                optionGroup.Options.Add(new Option()
                {
                    IsDefault = optionAdd.IsDefault,
                    Title = optionAdd.Title,
                    ImageUrl = optionAdd.ImageUrl,
                    IsCalculatePrice = optionAdd.IsCalculatePrice,
                    Price = optionAdd.Price,
                    Status = optionAdd.Status,
                });
            }

            optionGroup.Title = request.Title;
            optionGroup.IsRequire = request.IsRequire;
            optionGroup.Type = request.Type;
            optionGroup.Status = request.Status;
            optionGroup.MinChoices = request.MinChoices;
            optionGroup.MaxChoices = request.MaxChoices;

            _optionGroupRepository.Update(optionGroup);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            var optionGroupRe = _optionGroupRepository.GetByIdIncludeOption(request.Id);
            optionGroup.Options = optionGroupRe.Options.Where(o => o.Status != OptionStatus.Delete).ToList();
            var response = _mapper.Map<OptionGroupResponse>(optionGroupRe);
            return Result.Success(response);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(UpdateOptionGroupCommand request)
    {
        var optionGroup = _optionGroupRepository.Get(o => o.Id == request.Id
                                                          && o.ShopId == _currentPrincipalService.CurrentPrincipalId
                                                          && o.Status != OptionGroupStatus.Delete)
            .Include(og => og.Options).SingleOrDefault();
        if (optionGroup == default)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.Id}, HttpStatusCode.NotFound);

        if (_optionGroupRepository.CheckExistTitleOptionGroup(request.Title, _currentPrincipalService.CurrentPrincipalId.Value, request.Id))
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_DOUBLE_TITLE.GetDescription(), new object[]{request.Title}, HttpStatusCode.Conflict);

        foreach (var option in request.Options.Where(o => o.Id != 0))
        {
            if (_optionRepository.Get(o => o.Id == option.Id
                                           && o.OptionGroupId == request.Id
                                           && o.Status != OptionStatus.Delete).SingleOrDefault() == default)
                throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.Id}, HttpStatusCode.NotFound);
        }

        // Validate request
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
    }
}