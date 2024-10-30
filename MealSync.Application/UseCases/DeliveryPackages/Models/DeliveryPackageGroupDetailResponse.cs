using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.DeliveryPackages.Models;

public class DeliveryPackageGroupDetailResponse
{
    public long DeliveryPackageId { get; set; }

    public int Total { get; set; }

    public int Waiting { get; set; }

    public int Delivering { get; set; }

    public int Successful { get; set; }

    public int Failed { get; set; }

    public ShopStaffInforInDelvieryPackage ShopDeliveryStaff { get; set; }

    public List<DormitoryStasisticForEachStaff> Dormitories { get; set; } = new();

    public List<OrderForShopByStatusResponse> Orders { get; set; } = new();

    public class ShopStaffInforInDelvieryPackage
    {
        public long DeliveryPackageId { get; set; }

        public long Id { get; set; }

        public string FullName { get; set; }

        public string AvatarUrl { get; set; }

        public bool IsShopOwnerShip { get; set; }
    }

    public class DormitoryStasisticForEachStaff
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Total { get; set; }

        public int Waiting { get; set; }

        public int Delivering { get; set; }

        public int Successful { get; set; }

        public int Failed { get; set; }

        public ShopStaffInforInDelvieryPackage ShopDeliveryStaff { get; set; }
    }
}