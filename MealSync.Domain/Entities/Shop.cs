using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("shop")]
public class Shop : BaseEntity
{
    [Key]
    public long Id { get; set; }

    public long LocationId { get; set; }

    public long WalletId { get; set; }

    public string Name { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    public string PhoneNumber { get; set; }

    public string? BankCode { get; set; }

    public string? BankShortName { get; set; }

    public string? BankAccountNumber { get; set; }

    public int TotalOrder { get; set; }

    public int TotalProduct { get; set; }

    public int TotalReview { get; set; }

    public int TotalRating { get; set; }

    public ShopStatus Status { get; set; } = ShopStatus.UnApprove;

    public bool IsAcceptingOrderNextDay { get; set; }

    public bool IsReceivingOrderPaused { get; set; }

    public double MinValueOrderFreeShip { get; set; }

    public double AdditionalShipFee { get; set; }

    public bool IsAutoOrderConfirmation { get; set; }

    public int MaxOrderHoursInAdvance { get; set; }

    public int MinOrderHoursInAdvance { get; set; }

    public int NumOfWarning { get; set; }

    public virtual Account Account { get; set; }

    public virtual Location Location { get; set; }

    public virtual Wallet Wallet { get; set; }

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<ShopDormitory> ShopDormitories { get; set; } = new List<ShopDormitory>();

    public virtual ICollection<StaffDelivery> StaffDeliveries { get; set; } = new List<StaffDelivery>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<DeliveryPackage> DeliveryPackages { get; set; } =
        new List<DeliveryPackage>();

    public virtual ICollection<OperatingSlot> OperatingSlots { get; set; } = new List<OperatingSlot>();

    public virtual ICollection<ShopCategory> ShopCategories { get; set; } = new List<ShopCategory>();
}