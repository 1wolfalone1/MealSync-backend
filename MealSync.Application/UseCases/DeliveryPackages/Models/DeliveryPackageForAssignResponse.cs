using System.Text.Json.Serialization;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

public class DeliveryPackageForAssignResponse
{
    public int Total { get; set; }

    public int Waiting { get; set; }

    public int Delivering { get; set; }

    public int Successful { get; set; }

    public int Failed { get; set; }

    [JsonIgnore]
    public double CurrentDistance { get; set; }

    public ShopStaffInforResponse ShopDeliveryStaff { get; set; }

    public List<DormitoryStasisticForEachStaff> Dormitories { get; set; } = new();

    public List<OrderForShopByStatusResponse> Orders { get; set; } = new();

    public class DormitoryStasisticForEachStaff
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Total { get; set; }

        public int Waiting { get; set; }

        public int Delivering { get; set; }

        public int Successful { get; set; }

        public int Failed { get; set; }

        public ShopStaffInforResponse ShopDeliveryStaff { get; set; }
    }

    public class ShopStaffInforResponse
    {
        public long DeliveryPackageId { get; set; }

        public long Id { get; set; }

        public string FullName { get; set; }

        [JsonIgnore]
        public string PhoneNumber { get; set; }

        [JsonIgnore]
        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        [JsonIgnore]
        public bool IsShopOwner { get; set; }

        public bool IsShopOwnerShip { get => IsShopOwner; }

        [JsonIgnore]
        public double CurrentTaskLoad { get; set; }
    }
}