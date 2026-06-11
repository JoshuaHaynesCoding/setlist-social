namespace SetlistSocial.Api.External;

public interface ILastFmClient
{
    Task<IReadOnlyList<LastFmArtistSearchResult>> SearchArtistsAsync(
        string artistName,
        CancellationToken cancellationToken);
}
