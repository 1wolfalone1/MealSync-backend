using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.ShopGetQrCodeOfOrders;

public class ShopGetQrCodeOfOrderHandler : IQueryHandler<ShopGetQrCodeOfOrderQuery, Result>
{
    private readonly IOrderRepository _orderRepository;

    public ShopGetQrCodeOfOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Result>> Handle(ShopGetQrCodeOfOrderQuery request, CancellationToken cancellationToken)
    {
        var order = _orderRepository.GetById(request.Id);
        return Result.Success(order.QrScanToDeliveried);
    }
}