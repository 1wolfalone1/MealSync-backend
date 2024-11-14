using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportForShopWebByFilter;

public class GetReportForShopWebByFilterQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public ReportStatus? Status { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}