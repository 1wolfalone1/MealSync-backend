using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.CreateOrderDataSample;

public class CreateOrderDataSampleCommand : ICommand<Result>
{
    public long ShopId { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public List<long> CustomerIds { get; set; }
}