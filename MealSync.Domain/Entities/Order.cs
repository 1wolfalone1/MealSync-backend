using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("order")]
public class Order : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? PromotionId { get; set; }

    public long ShopId { get; set; }

    public long CustomerId { get; set; }

    public long? DeliveryPackageId { get; set; }

    public long ShopLocationId { get; set; }

    public long CustomerLocationId { get; set; }

    public long BuildingId { get; set; }

    public string BuildingName { get; set; }

    public OrderStatus Status { get; set; }

    public string? Note { get; set; }

    public double ShippingFee { get; set; }

    public double TotalPrice { get; set; }

    public double TotalPromotion { get; set; }

    public double ChargeFee { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime IntendedReceiveDate { get; set; }

    public DateTimeOffset? CancelAt { get; set; }

    public DateTimeOffset? RejectAt { get; set; }

    public DateTimeOffset? ReceiveAt { get; set; }

    public DateTimeOffset? LastestDeliveryFailAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? ResolveAt { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    [Column(TypeName = "text")]
    public string? QrScanToDeliveried { get; set; }

    public string? DeliverySuccessImageUrl { get; set; }

    public string? EvidenceDeliveryFailJson { get; set; }

    public bool IsRefund { get; set; } = false;

    public bool IsReport { get; set; } = false;

    public bool IsPaidToShop { get; set; }

    public string? Reason { get; set; }

    public string? ReasonIdentity { get; set; }

    public string? HistoryAssignJson { get; set; }

    public virtual Promotion? Promotion { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Location ShopLocation { get; set; }

    public virtual Location CustomerLocation { get; set; }

    public virtual Building Building { get; set; }

    public virtual DeliveryPackage DeliveryPackage { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}

public class ShopDeliveyFailEvidence
{
    public string ImageUrl { get; set; }

    public DateTimeOffset TakePictureDateTime { get; set; }
}

public class HistoryAssign
{
    public long Id { get; set; }

    public DateTimeOffset AssignDate { get; set; }
}