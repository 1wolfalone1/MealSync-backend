using System.ComponentModel;

namespace MealSync.Application.Common.Services.Map;

public enum VehicleMaps
{
    [Description("car")]
    Car = 1,

    [Description("bike")]
    Bike = 2,

    [Description("taxi")]
    Taxi = 3,

    [Description("truck")]
    Truck = 4,

    [Description("hd")]
    Hd = 5,
}