using MealSync.Application.Common.Utils;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class FoodDetailResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public string ImageUrl { get; set; } = null!;

    public FoodStatus Status { get; set; }

    public bool IsSoldOut { get; set; }

    public PlatformCategoryResponse PlatformCategory { get; set; }

    public ShopCategoryResponse ShopCategory { get; set; }

    public List<OperatingSlotResponse> OperatingSlots { get; set; }

    public List<FoodOptionGroupResponse> OptionGroups { get; set; }

    public class PlatformCategoryResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class ShopCategoryResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class OperatingSlotResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public int StartTime { get; set; }

        private int _endTime; // Backing field

        public int EndTime
        {
            get
            {
                return TimeFrameUtils.ConvertEndTime(_endTime); // Use the backing field
            }

            set
            {
                _endTime = value; // Set the backing field
            }
        }

        public bool IsReceivingOrderPaused { get; set; }
    }

    public class FoodOptionGroupResponse
    {
        public int DisplayOrder { get; set; }

        public long OptionGroupId { get; set; }

        public OptionGroupResponse OptionGroup { get; set; }
    }

    public class OptionGroupResponse
    {
        public long Id { get; set; }

        public string Title { get; set; } = null!;

        public bool IsRequire { get; set; }

        public OptionGroupTypes Type { get; set; }

        public OptionGroupStatus Status { get; set; }

        public int MinChoices { get; set; }

        public int MaxChoices { get; set; }

        public List<OptionResponse> Options { get; set; }
    }

    public class OptionResponse
    {
        public long Id { get; set; }

        public bool IsDefault { get; set; }

        public string Title { get; set; } = null!;

        public bool IsCalculatePrice { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public OptionStatus Status { get; set; }
    }
}