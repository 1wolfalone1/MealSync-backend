using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum ResponseCode
{
    [Description("Common Error")] CommonError = 1,

    [Description("Validation Error")] ValidationError = 2,

    [Description("Mapping Error")] MappingError = 3,

    [Description("Unauthorized")] Unauthorized = 4,
}
