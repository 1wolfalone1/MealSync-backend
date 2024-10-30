using System.Text.Json.Serialization;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.ShopDeliveryStaffs.Models;

public class ShopDeliveryStaffForAssignResponse
{
    public int Total { get; set; }

    public int Waiting { get; set; }

    public int Delivering { get; set; }

    public int Successful { get; set; }

    public int Failed { get; set; }

    public ShopStaffInforResponse StaffInfor { get; set; }

    public class ShopStaffInforResponse
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }

        public bool IsShopOwner { get; set; }
    }
}