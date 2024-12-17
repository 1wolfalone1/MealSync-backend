using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Models;

public class AccountResponse
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public long RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public Genders Genders { get; set; }

    public bool IsSelectedBuilding { get; set; }

    public string PhoneNumber { get; set; }

    public BuildingInAccount Building { get; set; }

    public class BuildingInAccount
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}