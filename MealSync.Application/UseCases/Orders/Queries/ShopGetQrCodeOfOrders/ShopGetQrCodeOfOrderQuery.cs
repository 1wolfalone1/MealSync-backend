using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.ShopGetQrCodeOfOrders;

public class ShopGetQrCodeOfOrderQuery : IQuery<Result>
{
    public long Id { get; set; }
}