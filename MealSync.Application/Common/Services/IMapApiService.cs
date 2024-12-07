using MealSync.Application.Common.Services.Map;
using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services;

public interface IMapApiService
{
    Task<DistanceMatrixResponse> GetDistanceMatrixAsync(Location origins, List<Location> destinations, VehicleMaps vehicle);

    Task<Element> GetDistanceOneDestinationAsync(Location origins, Location destination, VehicleMaps vehicle);
}