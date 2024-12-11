using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Moderators.Queries.GetActivityLog;

public class GetActivityLogQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public int TargetType { get; set; }
}