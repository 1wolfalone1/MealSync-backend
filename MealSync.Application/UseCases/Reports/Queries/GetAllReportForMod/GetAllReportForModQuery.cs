using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Queries.GetAllReportForMod;

public class GetAllReportForModQuery : PaginationRequest, IQuery<Result>
{
    public bool? IsAllowAction { get; set; }

    public string? SearchValue { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public ReportStatus? Status { get; set; }

    public long? DormitoryId { get; set; }

    public FilterReportOrderBy OrderBy { get; set; } = FilterReportOrderBy.CreatedDate;

    public FilterReportDirection Direction { get; set; } = FilterReportDirection.DESC;

    public enum FilterReportOrderBy
    {
        CreatedDate = 1,
        ShopName = 2,
        CustomerName = 3,
        Title = 4,
        Content = 5,
    }

    public enum FilterReportDirection
    {
        ASC = 1,
        DESC = 2,
    }
}