using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OptionGroups.Models;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Application.UseCases.OptionGroups.Queries.GetAllOptionGroupOfShop;

public class GetAllOptionGroupOfShopHandler : IQueryHandler<GetAllShopOptionGroupQuery, Result>
{
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetAllOptionGroupOfShopHandler(IOptionGroupRepository optionGroupRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _optionGroupRepository = optionGroupRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetAllShopOptionGroupQuery request, CancellationToken cancellationToken)
    {
        var data = _optionGroupRepository.GetAllShopOptonGroup(_currentPrincipalService.CurrentPrincipalId, request.PageIndex, request.PageSize, request.Title);
        var response = new PaginationResponse<ShopOptionGroupResponse>(
            _mapper.Map<List<ShopOptionGroupResponse>>(data.OptionGroups),
            data.TotalCount,
            request.PageIndex,
            request.PageSize);
        return Result.Success(response);
    }
}