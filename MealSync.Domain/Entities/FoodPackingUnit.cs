using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("food_packing_unit")]
public class FoodPackingUnit : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? ShopId { get; set; }

    public string Name { get; set; }

    public double Weight { get; set; }

    public FoodPackingUnitType Type { get; set; }

    public virtual Shop Shop { get; set; }

    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();
}