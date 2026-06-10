namespace SetlistSocial.Api.Models;

public sealed class Review : IHasTimestamps
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Body { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public int ConcertId { get; set; }
    public Concert Concert { get; set; } = null!;
}
