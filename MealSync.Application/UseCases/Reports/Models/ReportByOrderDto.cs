using System.Text.Json.Serialization;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Models;

public class ReportByOrderDto
{
    [JsonIgnore]
    public DateTimeOffset MinCreatedDate { get; set; }

    [JsonIgnore]
    public DateTime IntendedReceiveDate { get; set; }

    [JsonIgnore]
    public int StartTime { get; set; }

    [JsonIgnore]
    public int EndTime { get; set; }

    public long OrderId { get; set; }

    public string Description { get; set; }

    public bool IsAllowShopReply { get; set; }

    public List<ReportDto> Reports { get; set; }

    public class ReportDto
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
}