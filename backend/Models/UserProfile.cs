namespace SetlistSocial.Api.Models;

public sealed class UserProfile : IHasTimestamps
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public required string OAuthSubject { get; set; }
    public string? Bio { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Concert> Concerts { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
    public ICollection<ActivityEvent> ActivityEvents { get; set; } = [];
}
