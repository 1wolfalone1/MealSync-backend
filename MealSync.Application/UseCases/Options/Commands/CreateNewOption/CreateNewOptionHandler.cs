using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Application.UseCases.Options.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Options.Commands.CreateNewOption;

public class CreateNewOptionHandler : ICommandHandler<CreateNewOptionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionRepository _optionRepository;
    private readonly IMapper _mapper;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ILogger<CreateNewOptionHandler> _logger;

    public CreateNewOptionHandler(IUnitOfWork unitOfWork, IOptionRepository optionRepository, ILogger<CreateNewOptionHandler> logger, IMapper mapper, IOptionGroupRepository optionGroupRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _unitOfWork = unitOfWork;
        _optionRepository = optionRepository;
        _logger = logger;
        _mapper = mapper;
        _optionGroupRepository = optionGroupRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(CreateNewOptionCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var option = new Option()
            {
                OptionGroupId = request.OptionGroupId,
                IsDefault = false,
                Title = request.Title,
                ImageUrl = request.ImageUrl,
                IsCalculatePrice = request.IsCalculatePrice,
                Price = request.Price,
                Status = OptionStatus.Active,
            };
            await _optionRepository.AddAsync(option).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            var response = _mapper.Map<OptionResponse>(option);
            return Result.Success(response);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(CreateNewOptionCommand request)
    {
        if (_optionGroupRepository.Get(og => og.Id == request.OptionGroupId
                                             && og.ShopId == _currentPrincipalService.CurrentPrincipalId
                                             && og.Status != OptionGroupStatus.Delete).SingleOrDefault() == default)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.OptionGroupId}, HttpStatusCode.NotFound);

    }
}