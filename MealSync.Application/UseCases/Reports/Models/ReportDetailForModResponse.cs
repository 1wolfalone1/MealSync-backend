using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Models;

public class ReportDetailForModResponse
{
    public bool IsAllowAction { get; set; }

    public bool IsUnderReview { get; set; }

    public bool IsNotAllowReject { get; set; }

    public CustomerInfoForModResponse CustomerInfo { get; set; }

    public ShopInfoForModResponse ShopInfo { get; set; }

    public OrderInfoForModResponse OrderInfo { get; set; }

    public List<ReportResponse> Reports { get; set; }

    public class ReportResponse
    {
        public long Id { get; set; }

        public long OrderId { get; set; }

        public string OrderDescription { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public List<string> ImageUrls { get; set; }

        public ReportStatus Status { get; set; }

        public string? Reason { get; set; }

        public bool IsReportedByCustomer { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
    }

    public class CustomerInfoForModResponse
    {
        public long Id { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public string? FullName { get; set; }

        public CustomerStatus Status { get; set; }
    }

    public class ShopInfoForModResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? LogoUrl { get; set; }

        public string? BannerUrl { get; set; }

        public string? Description { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public ShopStatus Status { get; set; }
    }

    public class OrderInfoForModResponse
    {
        public OrderStatus Status { get; set; }

        public string? ReasonIdentity { get; set; }
    }
}