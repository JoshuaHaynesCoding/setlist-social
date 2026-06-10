namespace SetlistSocial.Api.Models;

public sealed class ActivityEvent
{
    public int Id { get; set; }
    public required string EventType { get; set; }
    public required string Summary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public int? UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }

    public int? ConcertId { get; set; }
    public Concert? Concert { get; set; }
}
