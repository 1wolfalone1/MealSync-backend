using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OperatingSlots.Models;

namespace MealSync.Application.UseCases.OperatingSlots.Queries;

public class GetAllShopOperatingSlotHandler : IQueryHandler<GetAllShopOperatingSlotQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IMapper _mapper;

    public GetAllShopOperatingSlotHandler(ICurrentPrincipalService currentPrincipalService, IOperatingSlotRepository operatingSlotRepository, IMapper mapper)
    {
        _currentPrincipalService = currentPrincipalService;
        _operatingSlotRepository = operatingSlotRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetAllShopOperatingSlotQuery request, CancellationToken cancellationToken)
    {
        var operatingSlots = _operatingSlotRepository.Get(op => op.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .OrderBy(op => op.StartTime).ToList();
        var response = _mapper.Map<List<OperatingSlotResponse>>(operatingSlots);
        return Result.Success(response);
    }
}