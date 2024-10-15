namespace MealSync.Application.UseCases.Orders.Models;

public class OrderDetailDescriptionDto
{
    public string? OptionGroupTitle { get; set; }

    public List<OptionDto> Options { get; set; }

    public class OptionDto
    {
        public string OptionTitle { get; set; }

        public string? OptionImageUrl { get; set; }

        public bool IsCalculatePrice { get; set; }

        public double Price { get; set; }
    }
}