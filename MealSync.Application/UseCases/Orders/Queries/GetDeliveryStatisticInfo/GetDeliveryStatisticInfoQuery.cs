using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Queries.GetDeliveryStatisticInfo;

public class GetDeliveryStatisticInfoQuery : ICommand<Result>
{
    public int StartTime { get; set; }

    public int EndTime { get; set; }

    public long[] OrderIds { get; set; }
}