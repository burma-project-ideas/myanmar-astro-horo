using System.Text.Json;
using Microsoft.JSInterop;

namespace BlazorMyanmarAstroHoro.Services;

/// <summary>Browser-local replacement for the legacy desktop .db horoscope files.</summary>
public sealed class SavedHoroscopeService(IJSRuntime js)
{
    private const string StorageKey = "myanmar-astro-horo.saved-records.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<SavedHoroscope>> LoadAsync()
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<SavedHoroscope>>(json, JsonOptions) ?? []; }
        catch (JsonException) { return []; }
    }

    public async Task<IReadOnlyList<SavedHoroscope>> SaveAsync(AstroInputModel input)
    {
        var records = (await LoadAsync()).ToList();
        records.Insert(0, new SavedHoroscope
        {
            Id = Guid.NewGuid().ToString("N"),
            SavedAt = DateTimeOffset.UtcNow,
            Input = Copy(input)
        });
        await PersistAsync(records);
        return records;
    }

    public async Task<IReadOnlyList<SavedHoroscope>> DeleteAsync(string id)
    {
        var records = (await LoadAsync()).Where(record => record.Id != id).ToList();
        await PersistAsync(records);
        return records;
    }

    private async Task PersistAsync(List<SavedHoroscope> records) => await js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(records, JsonOptions));
    private static AstroInputModel Copy(AstroInputModel input) => new()
    {
        Name = input.Name, Year = input.Year, Month = input.Month, Day = input.Day, Hour = input.Hour, Minute = input.Minute,
        Latitude = input.Latitude, Longitude = input.Longitude, LocationName = input.LocationName, IsLocalMeanTime = input.IsLocalMeanTime, StandardTimeOffset = input.StandardTimeOffset
    };
}

public sealed class SavedHoroscope
{
    public string Id { get; init; } = "";
    public DateTimeOffset SavedAt { get; init; }
    public AstroInputModel Input { get; init; } = new();
}
