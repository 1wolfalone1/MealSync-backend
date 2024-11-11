using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetOptionGroupDetail;

public class GetOptionGroupDetailHandler : IQueryHandler<GetOptionGroupDetailQuery, Result>
{
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetOptionGroupDetailHandler(IOptionGroupRepository optionGroupRepository, IFoodOptionGroupRepository foodOptionGroupRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _optionGroupRepository = optionGroupRepository;
        _foodOptionGroupRepository = foodOptionGroupRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetOptionGroupDetailQuery request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var optionGroup = _optionGroupRepository.Get(og => og.Id == request.Id)
            .Include(og => og.Options.Where(o => o.Status != OptionStatus.Delete))
            .Include(og => og.FoodOptionGroups)
            .ThenInclude(fog => fog.Food).SingleOrDefault();
        var response = _mapper.Map<OptionGroupDetailResponse>(optionGroup);
        return Result.Success(response);
    }

    private void Validate(GetOptionGroupDetailQuery request)
    {
        if (_optionGroupRepository.Get(og => og.Id == request.Id && og.Status != OptionGroupStatus.Delete && og.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .SingleOrDefault() == default)
            throw new InvalidBusinessException(MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[]{request.Id}, HttpStatusCode.NotFound);
    }
}