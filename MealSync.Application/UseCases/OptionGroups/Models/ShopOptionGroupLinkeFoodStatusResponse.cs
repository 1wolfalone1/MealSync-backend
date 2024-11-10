using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.OptionGroups.Models;

public class ShopOptionGroupLinkeFoodStatusResponse
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public bool IsRequire { get; set; }

    public OptionGroupTypes Type { get; set; }

    public OptionGroupStatus Status { get; set; }

    public int MinChoices { get; set; }

    public int MaxChoices { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public bool IdLinked { get; set; }

    public List<OptionGroupResponse.OptionResponse> Options { get; set; }
}