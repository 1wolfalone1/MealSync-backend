using MealSync.Application.Common.Utils;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class ShopOwnerFoodResponse
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; }

    public string? ImageUrl { get; set; }

    public List<FoodResponse> Foods { get; set; } = new();

    public class FoodResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public double Price { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool IsSoldOut { get; set; }

        public int Status { get; set; }

        public int TotalOrderInNextTwoHours { get; set; }

        public List<OperatingSlotInFood> OperatingSlots { get; set; } = new();

        public class OperatingSlotInFood
        {
            public long Id { get; set; }

            public string Title { get; set; }

            public int StartTime { get; set; }

            public int EndTime { get; set; }

            public string TimeFrameFormat
            {
                get
                {
                    return TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
                }
            }
        }
    }
}