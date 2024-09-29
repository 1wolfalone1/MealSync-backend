using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("report")]
public class Report : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? ShopId { get; set; }

    public long? CustomerId { get; set; }

    public long? StaffDeliveryId { get; set; }

    public long OrderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public string ImageUrl { get; set; }

    public ReportStatus Status { get; set; }

    public string? Reason { get; set; }

    public virtual Shop? Shop { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual StaffDelivery? StaffDelivery { get; set; }

    public virtual Order Order { get; set; }
}