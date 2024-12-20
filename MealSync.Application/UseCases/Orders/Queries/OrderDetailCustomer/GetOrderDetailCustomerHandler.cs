using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MealSync.Application.UseCases.Orders.Queries.OrderDetailCustomer;

public class GetOrderDetailCustomerHandler : IQueryHandler<GetOrderDetailCustomerQuery, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private IMapper _mapper;

    public GetOrderDetailCustomerHandler(IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetOrderDetailCustomerQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var order = await _orderRepository.GetByIdAndCustomerIdForDetail(request.Id, customerId).ConfigureAwait(false);

        if (order == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            var detailOrderCustomerResponse = _mapper.Map<DetailOrderCustomerResponse>(order);
            var evidence = string.IsNullOrEmpty(order.EvidenceDeliveryFailJson)
                ? default
                : JsonConvert.DeserializeObject<List<ShopDeliveyFailEvidence>>(order.EvidenceDeliveryFailJson, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                });

            if (evidence != default)
            {
                var shopDeliveryFailEvidenceResponses = _mapper.Map<List<DetailOrderCustomerResponse.ShopDeliveryFailEvidenceResponse>>(evidence);
                detailOrderCustomerResponse.ShopDeliveryFailEvidence = shopDeliveryFailEvidenceResponses;
            }

            return Result.Success(detailOrderCustomerResponse);
        }
    }
}