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
            optionGroup.Title = request.Title;
            optionGroup.IsRequire = request.IsRequire;
            optionGroup.Type = request.Type;

            var options = new List<Option>();
            foreach (var option in request.Options)
            {
                options.Add(new Option()
                {
                    Id = option.Id,
                    IsDefault = request.IsRequire ? option.IsDefault : false,
                    Title = option.Title,
                    IsCalculatePrice = option.IsCalculatePrice,
                    Price = option.Price,
                    ImageUrl = option.ImageUrl,
                });
            }

            optionGroup.Options = options;
            _optionGroupRepository.Update(optionGroup);
            var response = _mapper.Map<OptionGroupResponse>(optionGroup);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
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

        foreach (var option in request.Options)
        {
            if (_optionRepository.Get(o => o.Id == option.Id
                                           && o.OptionGroupId == request.Id
                                           && o.Status != OptionStatus.Delete).SingleOrDefault() == default)
                throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.Id}, HttpStatusCode.NotFound);
        }

        if (optionGroup.Options.Count != request.Options.Count)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_UPDATE_NOT_ENOUGH_OPTION.GetDescription());

        if (request.IsRequire)
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
    }
}