using MealSync.Application.Common.Utils;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Models;

public class ShopFoodWebResponse
{
    public long Id { get; set; }

    public long ShopId { get; set; }

    public long PlatformCategoryId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? ImageUrl { get; set; }

    public int TotalOrder { get; set; }

    public FoodStatus Status { get; set; }

    public bool IsSoldOut { get; set; }

    public ShopCategoryForShopFoodWeb ShopCategory { get; set; }

    public List<OperatingSlotForShopFoodWeb> OperatingSlots { get; set; }

    public class ShopCategoryForShopFoodWeb
    {
        public long Id { get; set; }

        public long ShopId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }
    }

    public class OperatingSlotForShopFoodWeb
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
}