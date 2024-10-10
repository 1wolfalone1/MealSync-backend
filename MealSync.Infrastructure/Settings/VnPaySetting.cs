using System.ComponentModel.DataAnnotations;

namespace MealSync.Infrastructure.Settings;

public class VnPaySetting
{
    [Required]
    public string PaymentUrl { get; set; } = default!;

    [Required]
    public string ReturnUrl { get; set; } = default!;

    [Required]
    public string RefundUrl { get; set; } = default!;

    [Required]
    public string TmpCode { get; set; } = default!;

    [Required]
    public string HashSecret { get; set; } = default!;
}