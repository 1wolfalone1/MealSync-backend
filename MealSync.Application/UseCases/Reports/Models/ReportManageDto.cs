using System.Text.Json.Serialization;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Models;

public class ReportManageDto
{
    public long Id { get; set; }

    public string ShopName { get; set; }

    public string? CustomerName { get; set; }

    public long OrderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public ReportStatus Status { get; set; }

    public bool IsUnderReview { get; set; }

    public bool IsAllowAction { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    [JsonIgnore]
    public int TotalCount { get; set; }
}