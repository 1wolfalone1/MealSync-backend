using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Models;

public class AccountDetailForModManageDto
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public Genders Genders { get; set; }

    public AccountStatus Status { get; set; }

    public int NumOfFlag { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public List<AccountFlagDetailDto> AccountFlags { get; set; }

    public OrderSummaryDto OrderSummary { get; set; }

    public class OrderSummaryDto
    {
        public int TotalOrderInProcess { get; set; }

        public int TotalCancelByCustomer { get; set; }

        public int TotalCancelOrRejectByShop { get; set; }

        public int TotalDelivered { get; set; }

        public int TotalFailDeliveredByCustomer { get; set; }

        public int TotalFailDeliveredByShop { get; set; }

        public int TotalReportResolved { get; set; }
    }

    public class AccountFlagDetailDto
    {
        public long Id { get; set; }

        public AccountActionTypes ActionType { get; set; }

        public AccountTargetTypes TargetType { get; set; }

        public string TargetId { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
    }
}