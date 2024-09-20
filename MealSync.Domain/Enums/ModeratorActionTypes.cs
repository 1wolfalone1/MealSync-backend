using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum ModeratorActionTypes
{
    [Description("GET")]
    Read = 1,
    [Description("POST")]
    Create = 2,
    [Description("PUT")]
    Update = 3,
    [Description("DELETE")]
    Delete = 4,
}