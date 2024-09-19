using System.ComponentModel.DataAnnotations;

namespace MealSync.Infrastructure.Settings;

public class RedisSetting
{
    [Required]
    public string ConnectionString { get; set; } = default!;
}