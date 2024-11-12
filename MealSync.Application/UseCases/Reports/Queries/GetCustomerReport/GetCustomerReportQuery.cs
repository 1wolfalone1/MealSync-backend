using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reports.Queries.GetCustomerReport;

public class GetCustomerReportQuery : IQuery<Result>
{
    public long ReportId { get; set; }
}