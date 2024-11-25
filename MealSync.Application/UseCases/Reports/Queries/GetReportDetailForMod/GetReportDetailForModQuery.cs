using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportDetailForMod;

public class GetReportDetailForModQuery : IQuery<Result>
{
    public long ReportId { get; set; }
}