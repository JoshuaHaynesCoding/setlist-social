namespace SetlistSocial.Api.Models;

public sealed class Artist : IHasTimestamps
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? LastFmUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Concert> Concerts { get; set; } = [];
}
