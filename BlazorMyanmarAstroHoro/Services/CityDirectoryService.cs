using System.Net.Http.Json;

namespace BlazorMyanmarAstroHoro.Services;

/// <summary>Loads the original CleanVB6 city, coordinate, and standard-time catalog.</summary>
public sealed class CityDirectoryService(HttpClient httpClient)
{
    public async Task<IReadOnlyList<CityLocation>> GetLegacyCitiesAsync()
        => await httpClient.GetFromJsonAsync<List<CityLocation>>("data/legacy-cities.json") ?? [];
}

public sealed class CityLocation
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Country { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double UtcOffset { get; init; }
}
