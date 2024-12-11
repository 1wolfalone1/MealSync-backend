using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Moderators.Models;

public class ModeratorActivityLogResponse
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public ModeratorActionTypes ActionType { get; set; }

    public ModeratorTargetTypes TargetType { get; set; }

    public long TargetId { get; set; }

    public string ActionDetail { get; set; }

    public bool IsSuccess { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }

    public AccountInActivityLog Account { get; set; }

    public class AccountInActivityLog
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }
    }
}