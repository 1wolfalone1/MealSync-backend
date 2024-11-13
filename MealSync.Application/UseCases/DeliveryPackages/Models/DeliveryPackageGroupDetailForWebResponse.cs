using MealSync.Application.UseCases.Orders.Models;

namespace MealSync.Application.UseCases.DeliveryPackages.Models;

public class DeliveryPackageGroupDetailForWebResponse
{
    public long Id
    {
        get
        {
            return DeliveryPackageId;
        }
    }

    public long DeliveryPackageId { get; set; }

    public int Total { get; set; }

    public int Waiting { get; set; }

    public int Delivering { get; set; }

    public int Successful { get; set; }

    public int Failed { get; set; }

    public DeliveryPackageGroupDetailResponse.ShopStaffInforInDelvieryPackage ShopDeliveryStaff { get; set; }

    public List<DeliveryPackageGroupDetailResponse.DormitoryStasisticForEachStaff> Dormitories { get; set; } = new();

    public List<OrderDetailForShopResponse> Orders { get; set; } = new();
}