using System.ComponentModel.DataAnnotations;

namespace MealSync.Infrastructure.Settings;

public class JwtSetting
{
    [Required]
    public string Key { get; set; } = default!;

    [Required]
    public string Issuer { get; set; } = default!;

    [Required]
    public string Audience { get; set; } = default!;

    [Required]
    [Range(1, Int32.MaxValue)]
    public int TokenExpire { get; set; }

    [Required]
    [Range(1, Int32.MaxValue)]
    public int RefreshTokenExpire { get; set; }
}