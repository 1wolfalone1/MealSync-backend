namespace MealSync.Application.Common.Services.Models;

public class FirebaseUser
{
    public string UserId { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string Name { get; set; }

    public string Picture { get; set; }

    public bool EmailVerified { get; set; }
}