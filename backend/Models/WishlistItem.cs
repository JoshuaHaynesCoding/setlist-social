namespace SetlistSocial.Api.Models;

public sealed class WishlistItem : IHasTimestamps
{
    public int Id { get; set; }
    public string? Notes { get; set; }
    public string? SourceName { get; set; }
    public string? SourceUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public int? ArtistId { get; set; }
    public Artist? Artist { get; set; }
}
