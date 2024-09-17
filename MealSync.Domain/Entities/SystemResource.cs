using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("system_resource")]
public class SystemResource : BaseEntity
{
    [Key]
    public long Id { get; set; }

    public string ResourceCode { get; set; }

    public string ResourceContent { get; set; }

    public SystemResourceTypes ResourceType { get; set; }
}