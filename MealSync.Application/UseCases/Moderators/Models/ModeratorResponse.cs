using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Moderators.Models;

public class ModeratorResponse
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public AccountStatus Status { get; set; }

    public List<DormitoryInModerator> Dormitories { get; set; }

    public class DormitoryInModerator
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}