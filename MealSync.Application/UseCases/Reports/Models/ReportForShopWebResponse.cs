using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Models;

public class ReportForShopWebResponse
{
    public long Id { get; set; }

    public long? ShopId { get; set; }

    public string? ShopName { get; set; }

    public long? CustomerId { get; set; }

    public long OrderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public string ImageUrl { get; set; }

    public ReportStatus Status { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public CustomerInforInReport Customer { get; set; }

    public ShopDeliveryStaffInforInReport ShopDeliveryStaff { get; set; }

    public class CustomerInforInReport
    {
        public long Id { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public string? FullName { get; set; }
    }

    public class ShopDeliveryStaffInforInReport
    {
        public long DeliveryPackageId { get; set; }

        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        public bool IsShopOwnerShip { get; set; }
    }
}