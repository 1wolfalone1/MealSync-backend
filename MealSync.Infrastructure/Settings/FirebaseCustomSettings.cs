using System.ComponentModel.DataAnnotations;

namespace MealSync.Infrastructure.Settings;

public class FirebaseCustomSettings
{
    [Required]
    public string ProjectId { get; set; } = default!;

    [Required]
    public string PrivateKey { get; set; } = default!;

    [Required]
    public string ClientEmail { get; set; } = default!;

    [Required]
    public string TokenUri { get; set; } = default!;
}