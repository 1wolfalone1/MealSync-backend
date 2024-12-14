using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Customers.Models;

public class CustomerInfoResponse
{
    public long Id { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? FullName { get; set; }

    public Genders Genders { get; set; }

    public BuildingResponse Building { get; set; }

    public class BuildingResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}