using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Models;

public class ReportDetailShopWebResponse
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public List<string> ImageUrls { get; set; }

    public ReportStatus Status { get; set; }

    public string? Reason { get; set; }

    public bool IsReportedByCustomer { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public CustomerInfoResponse CustomerInfo { get; set; }

    public ShopDeliveryStaffInfoResponse ShopDeliveryStaffInfo { get; set; }

    public class CustomerInfoResponse
    {
        public long Id { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public string? FullName { get; set; }
    }

    public class ShopDeliveryStaffInfoResponse
    {
        public long DeliveryPackageId { get; set; }

        public long Id { get; set; }

        public string? FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsShopOwnerShip { get; set; }
    }
}