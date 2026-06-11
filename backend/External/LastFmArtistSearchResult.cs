namespace SetlistSocial.Api.External;

public sealed record LastFmArtistSearchResult(
    string Name,
    string? Url,
    int? Listeners,
    string? ImageUrl);
