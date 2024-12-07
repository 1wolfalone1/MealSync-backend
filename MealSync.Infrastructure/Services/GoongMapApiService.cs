using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Map;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Infrastructure.Services;

public class GoongMapApiService : BaseService, IMapApiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoongMapApiService> _logger;

    public GoongMapApiService(IConfiguration configuration, ILogger<GoongMapApiService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DistanceMatrixResponse> GetDistanceMatrixAsync(Location origins, List<Location> destinations, VehicleMaps vehicle)
    {
        try
        {
            // Base URL for the Goong Map API
            var url = "https://rsapi.goong.io/distancematrix";

            string desinationStr = string.Join('|', destinations.Select(x => x.GetLocationForGoongMap()));
            // Define query parameters
            var query = $"origins={origins.GetLocationForGoongMap()}" +
                        $"&destinations={desinationStr}" +
                        $"&vehicle={vehicle.GetDescription()}" +
                        $"&api_key={_configuration["GOONG_MAP_KEY"]}";

            // Combine base URL with query parameters
            var fullUrl = $"{url}?{query}";

            // Send the GET request
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode(); // Throw exception if not successful

            // Read and return the response content
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DistanceMatrixResponse>(responseString);
        }
        catch (Exception ex)
        {
            // Handle exceptions as needed
            _logger.LogError(ex, ex.Message);
            throw new InvalidBusinessException(MessageCode.E_SUGGEST_ORDER_NOT_AVAILABLE.GetDescription());
        }
    }

    public async Task<Element> GetDistanceOneDestinationAsync(Location origins, Location destination, VehicleMaps vehicle)
    {
        var row = await GetDistanceMatrixAsync(origins, new List<Location>() { destination }, vehicle).ConfigureAwait(false);
        var element = row.Rows.FirstOrDefault().Elements.Where(e => e.Status == "OK").FirstOrDefault();
        return element;
    }

    public async Task<DirectionResponse> GetDirectionsAsync(Location origins, Location destination, VehicleMaps vehicle)
    {
        try
        {
            // Build the API URL with query parameters
            var baseUrl = "https://rsapi.goong.io/direction";
            var query = $"origin={origins.GetLocationForGoongMap()}&destination={destination.GetLocationForGoongMap()}&vehicle={vehicle.GetDescription()}&api_key={_configuration["GOONG_MAP_KEY"]}";
            var fullUrl = $"{baseUrl}?{query}";

            // Send the GET request
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode(); // Throws if not 2xx

            // Read the response content
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Deserialize JSON into DirectionResponse object
            return JsonConvert.DeserializeObject<DirectionResponse>(jsonResponse);
        }
        catch (Exception ex)
        {
            // Handle exceptions as needed
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }
}