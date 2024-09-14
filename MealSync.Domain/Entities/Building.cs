using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("building")]
public class Building : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long DomitoryId { get; set; }

    public long LocationId { get; set; }

    public string Name { get; set; }

    public virtual Dormitory Dormitory { get; set; }

    public virtual Location Location { get; set; }

    public virtual ICollection<CustomerBuilding> CustomerBuildings { get; set; } = new List<CustomerBuilding>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}