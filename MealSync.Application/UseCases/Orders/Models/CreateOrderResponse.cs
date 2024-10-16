using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Orders.Models;

public class CreateOrderResponse
{
    public PaymentMethods PaymentMethod { get; set; }

    public string? PaymentLink { get; set; }

    public string Message { get; set; } = null!;

    public OrderResponse Order { get; set; }
}