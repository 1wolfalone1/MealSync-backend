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

    public string Description
    {
        get
        {
            var target = string.Empty;
            switch (TargetType)
            {
                case ModeratorTargetTypes.Order:
                    target = "đơn hàng";
                    break;
                case ModeratorTargetTypes.Customer:
                    target = "khách hàng";
                    break;
                case ModeratorTargetTypes.Report:
                    target = "báo cáo";
                    break;
                case ModeratorTargetTypes.Shop:
                    target = "cửa hàng";
                    break;
                case ModeratorTargetTypes.Withdrawal:
                    target = "yêu cầu rút tiền";
                    break;
            }

            var action = string.Empty;
            switch (ActionType)
            {
                case ModeratorActionTypes.Create:
                    action = "tạo mới";
                    break;
                case ModeratorActionTypes.Delete:
                    action = "xóa";
                    break;
                case ModeratorActionTypes.Update:
                    action = "cập nhật";
                    break;
            }

            return $"Moderator vừa {action} {target} vào lúc {CreatedDate.ToString("dd-MM-yyyy hh:mm:ss")}";
        }
    }

    public class AccountInActivityLog
    {
        public long Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string AvatarUrl { get; set; }
    }
}