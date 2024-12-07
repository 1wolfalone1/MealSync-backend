using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("location")]
public class Location : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Address { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual Building Building { get; set; }

    public virtual Dormitory Dormitory { get; set; }

    public virtual Order OrderShop { get; set; }

    public virtual Order OrderCustomer { get; set; }

    public string GetLocationForGoongMap()
    {
        return $"{Latitude},{Longitude}";
    }
}