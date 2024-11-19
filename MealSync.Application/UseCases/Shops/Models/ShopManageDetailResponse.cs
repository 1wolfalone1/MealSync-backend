using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Shops.Models;

public class ShopManageDetailResponse
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string? Description { get; set; }

    public string PhoneNumber { get; set; }

    public string? BankCode { get; set; }

    public string? BankShortName { get; set; }

    public string? BankAccountNumber { get; set; }

    public int TotalOrder { get; set; }

    public int TotalFood { get; set; }

    public int TotalReview { get; set; }

    public int TotalRating { get; set; }

    public double TotalRevenue { get; set; }

    public ShopStatus Status { get; set; }

    public int NumOfWarning { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public AccountShopResponse AccountShop { get; set; }

    public LocationShopResponse LocationShop { get; set; }

    public List<ShopDormitoryResponse> ShopDormitories { get; set; }

    public List<ShopOperatingSlotResponse> ShopOperatingSlots { get; set; }

    public class AccountShopResponse
    {
        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public string? FullName { get; set; }

        public Genders Genders { get; set; }

        public int NumOfFlag { get; set; }
    }

    public class LocationShopResponse
    {
        public long Id { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class ShopDormitoryResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class ShopOperatingSlotResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public bool IsActive { get; set; }

        public bool IsReceivingOrderPaused { get; set; }
    }
}