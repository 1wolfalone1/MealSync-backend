using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("dormitory_distance")]
public class DormitoryDistance : BaseEntity
{
    public long DormitoryFromId { get; set; }

    public long DormitoryToId { get; set; }

    public double Distance { get; set; }

    public int Duration { get; set; }

    public virtual Dormitory DormitoryFrom { get; set; }

    public virtual Dormitory DormitoryTo { get; set; }
}