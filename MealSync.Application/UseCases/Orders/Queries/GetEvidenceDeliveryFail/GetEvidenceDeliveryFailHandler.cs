using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Orders.Queries.GetEvidenceDeliveryFail;

public class GetEvidenceDeliveryFailHandler : IQueryHandler<GetEvidenceDeliveryFailQuery, Result>
{
    private readonly IMapper _mapper;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetEvidenceDeliveryFailHandler(IMapper mapper, IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService)
    {
        _mapper = mapper;
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetEvidenceDeliveryFailQuery request, CancellationToken cancellationToken)
    {
        // Vallidate
        Validate(request);

        var order = _orderRepository.GetById(request.OrderId);
        return Result.Success(_mapper.Map<EvidenceOrderResponse>(order));
    }

    private void Validate(GetEvidenceDeliveryFailQuery request)
    {
        if (_orderRepository.Get(o => o.Id == request.OrderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value).SingleOrDefault() == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);
    }
}