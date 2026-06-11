using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SetlistSocial.Api.External;

public sealed class LastFmClient(HttpClient httpClient, IConfiguration configuration) : ILastFmClient
{
    private const string BaseUrl = "https://ws.audioscrobbler.com/2.0/";

    public async Task<IReadOnlyList<LastFmArtistSearchResult>> SearchArtistsAsync(
        string artistName,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["LastFm:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Last.fm API key is not configured.");
        }

        var requestUrl =
            $"{BaseUrl}?method=artist.search&artist={Uri.EscapeDataString(artistName.Trim())}" +
            $"&api_key={Uri.EscapeDataString(apiKey)}&format=json&limit=12";

        using var response = await httpClient.GetAsync(requestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<LastFmSearchResponse>(
            stream,
            cancellationToken: cancellationToken);

        var artists = payload?.Results?.ArtistMatches?.Artists ?? [];

        return artists
            .Where(artist => !string.IsNullOrWhiteSpace(artist.Name))
            .Select(artist => new LastFmArtistSearchResult(
                artist.Name!,
                artist.Url,
                ParseListeners(artist.Listeners),
                SelectImageUrl(artist.Images)))
            .ToList();
    }

    private static int? ParseListeners(string? listeners)
    {
        return int.TryParse(listeners, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static string? SelectImageUrl(IReadOnlyList<LastFmImage>? images)
    {
        return images?
            .LastOrDefault(image => !string.IsNullOrWhiteSpace(image.Url))
            ?.Url;
    }

    private sealed class LastFmSearchResponse
    {
        [JsonPropertyName("results")]
        public LastFmResults? Results { get; init; }
    }

    private sealed class LastFmResults
    {
        [JsonPropertyName("artistmatches")]
        public LastFmArtistMatches? ArtistMatches { get; init; }
    }

    private sealed class LastFmArtistMatches
    {
        [JsonPropertyName("artist")]
        public List<LastFmArtist> Artists { get; init; } = [];
    }

    private sealed class LastFmArtist
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("url")]
        public string? Url { get; init; }

        [JsonPropertyName("listeners")]
        public string? Listeners { get; init; }

        [JsonPropertyName("image")]
        public List<LastFmImage> Images { get; init; } = [];
    }

    private sealed class LastFmImage
    {
        [JsonPropertyName("#text")]
        public string? Url { get; init; }
    }
}
