using System.Text.Json.Serialization;
using MealSync.Application.Common.Constants;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

public class DeliveryPackageForAssignUpdateResponse
{
    public long DeliveryPackageId { get; set; }

    public int Total { get; set; }

    public int Waiting { get; set; }

    public int Delivering { get; set; }

    public int Successful { get; set; }

    public int Failed { get; set; }

    [JsonIgnore]
    public double CurrentDistance {
        get
        {
            return Dormitories.Count * DevidedOrderConstant.DistanceLoad;
        }
    }

    [JsonIgnore]
    public double CurrentTaskLoad {
        get
        {
            return Waiting * DevidedOrderConstant.WaitingTaskWorkLoad + Delivering * DevidedOrderConstant.DeliveringTaskWorkLoad;
        }
    }

    public DeliveryPackageForAssignResponse.ShopStaffInforResponse ShopDeliveryStaff { get; set; }

    public List<DeliveryPackageForAssignResponse.DormitoryStasisticForEachStaff> Dormitories { get; set; } = new();

    public List<OrderDetailForShopResponse> Orders { get; set; } = new();
}