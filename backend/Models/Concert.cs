namespace SetlistSocial.Api.Models;

public sealed class Concert : IHasTimestamps
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? VenueName { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public DateTimeOffset ConcertDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
}
