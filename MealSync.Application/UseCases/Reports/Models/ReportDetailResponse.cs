using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Models;

public class ReportDetailResponse
{
    public long Id { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public List<string> ImageUrls { get; set; }

    public ReportStatus Status { get; set; }

    public string? Reason { get; set; }

    public bool IsReportedByCustomer { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}