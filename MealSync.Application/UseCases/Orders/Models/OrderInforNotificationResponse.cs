namespace MealSync.Application.UseCases.Orders.Models;

public class OrderInforNotificationResponse
{
    public long Id { get; set; }

    public CustomerInforNotification Customer { get; set; }

    public ShopInforNotification Shop { get; set; }

    public DeliveryStaffNotification DeliveryStaff { get; set; }

    public class DeliveryStaffNotification
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        public long RoleId { get; set; }
    }

    public class ShopInforNotification
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string ShopName { get; set; }

        public string PhoneNumber { get; set; }

        public string BannerUrl { get; set; }

        public string LogoUrl { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        public long RoleId { get; set; }
    }

    public class CustomerInforNotification
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        public long RoleId { get; set; }
    }
}