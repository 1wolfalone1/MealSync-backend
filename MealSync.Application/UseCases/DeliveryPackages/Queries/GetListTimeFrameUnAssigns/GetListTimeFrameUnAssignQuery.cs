using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetListTimeFrameUnAssigns;

public class GetListTimeFrameUnAssignQuery : IQuery<Result>
{
    public DateTime IntendedReceiveDate { get; set; }
}