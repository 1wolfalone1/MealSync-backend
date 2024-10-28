using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Promotions.Commands.Create;

public class CreatePromotionCommand : ICommand<Result>
{
    public string Title { get; set; }

    public string? Description { get; set; }

    public string? BannerUrl { get; set; }

    public int? AmountRate { get; set; }

    public int? MaximumApplyValue { get; set; }

    public int? AmountValue { get; set; }

    public int MinOrdervalue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int UsageLimit { get; set; }

    public PromotionApplyTypes ApplyType { get; set; }

    public PromotionStatus Status { get; set; } = PromotionStatus.Active;
}