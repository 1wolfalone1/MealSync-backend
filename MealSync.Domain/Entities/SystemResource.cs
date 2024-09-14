using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("system_resource")]
public class SystemResource : BaseEntity
{
    [Key]
    public long Id { get; set; }

    public string ResourceCode { get; set; }

    public string ResourceContent { get; set; }
}