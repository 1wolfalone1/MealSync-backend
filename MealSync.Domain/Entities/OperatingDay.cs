using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MealSync.Domain.Enums;

namespace MealSync.Domain.Entities;

[Table("operating_day")]
public class OperatingDay : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ShopOwnerId { get; set; }

    public DayOfWeeks DayOfWeek { get; set; }

    public bool IsClose { get; set; }

    public virtual ShopOwner ShopOwner { get; set; }

    public virtual ICollection<OperatingFrame> OperatingFrames { get; set; } = new List<OperatingFrame>();

    public virtual ICollection<ProductOperatingHour> ProductOperatingHours { get; set; } = new List<ProductOperatingHour>();

    public List<OperatingDay> CreateListOperatingDayForNewShop()
    {
        var daysOfWeeks = Enum.GetValues(typeof(DayOfWeeks)).Cast<DayOfWeeks>();
        var operatingDays = new List<OperatingDay>();
        foreach (var day in daysOfWeeks)
        {
            var operatingDay = new OperatingDay
            {
                ShopOwnerId = ShopOwnerId,
                DayOfWeek = day,
                IsClose = true,
            };

            var operatingFrames = new List<OperatingFrame>();

            for (var i = 0; i < 24 * 2; i++)
            {
                var hours = i / 2; // Calculate the hour (0 to 23)
                var minutes = i % 2 * 30; // Calculate the minutes (either 0 or 30)

                // Format the StartTime and EndTime as 4-digit integers (e.g., 0830)
                var startTime = hours * 100 + minutes; // Convert to 4-digit format (HHMM)
                var endTime = minutes == 0 ? hours * 100 + 30 : (hours + 1) * 100; // Calculate end time for each frame

                var operatingFrame = new OperatingFrame
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    IsActive = false,
                };

                operatingFrames.Add(operatingFrame);
            }

            // Associate the frames with the day
            operatingDay.OperatingFrames = operatingFrames;
            operatingDays.Add(operatingDay);
        }

        return operatingDays;
    }
}