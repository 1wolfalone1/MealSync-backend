﻿using MealSync.Application.Common.Utils;
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

        public FoodPackingUnitOfShopResponse FoodPackingUnit { get; set; }

        public List<OperatingSlotInFood> OperatingSlots { get; set; } = new();

        public class OperatingSlotInFood
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

            public string TimeFrameFormat
            {
                get
                {
                    return TimeFrameUtils.GetTimeFrameString(StartTime, EndTime);
                }
            }
        }

        public class FoodPackingUnitOfShopResponse
        {
            public long Id { get; set; }

            public long ShopId { get; set; }

            public string Name { get; set; }

            public double Weight { get; set; }

            public FoodPackingUnitType Type { get; set; }
        }
    }
}