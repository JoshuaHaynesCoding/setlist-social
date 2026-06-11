using System.Net;
using System.Net.Http.Json;

namespace SetlistSocial.Api.Tests;

public sealed class PublicWishlistAndExternalApiTests : IDisposable
{
    private readonly SetlistSocialApiFactory _factory = new();

    [Theory]
    [InlineData("/api/public/stats")]
    [InlineData("/api/public/artists")]
    [InlineData("/api/public/activity")]
    public async Task Public_endpoints_return_200(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public async Task Wishlist_endpoints_return_401_when_unauthenticated(string method)
    {
        var client = _factory.CreateClient();

        var response = method == "GET"
            ? await client.GetAsync("/api/me/wishlist")
            : await client.PostAsJsonAsync("/api/me/wishlist", NewWishlistItem("Sade"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Signed_in_user_can_add_and_read_their_own_wishlist_item()
    {
        await _factory.SeedUserAsync("wishlist-user", "Wishlist User");
        var client = CreateAuthenticatedClient("wishlist-user");

        var createResponse = await client.PostAsJsonAsync("/api/me/wishlist", NewWishlistItem("Sade"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<WishlistItemDto>();
        Assert.NotNull(created);
        Assert.Equal("Sade", created.ArtistName);
        Assert.Equal("https://www.last.fm/music/example", created.SourceUrl);
        Assert.Equal("Test source", created.SourceName);

        // Add a second item to verify ordering
        var secondResponse = await client.PostAsJsonAsync("/api/me/wishlist", NewWishlistItem("Kendrick Lamar"));
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        var secondCreated = await secondResponse.Content.ReadFromJsonAsync<WishlistItemDto>();
        Assert.NotNull(secondCreated);

        var listResponse = await client.GetAsync("/api/me/wishlist");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var wishlist = await listResponse.Content.ReadFromJsonAsync<List<WishlistItemDto>>();
        Assert.NotNull(wishlist);
        Assert.True(wishlist.Count >= 2, "Wishlist should contain at least 2 items");
        Assert.Contains(wishlist, item => item.Id == created.Id && item.ArtistName == "Sade");
        Assert.Contains(wishlist, item => item.Id == secondCreated.Id && item.ArtistName == "Kendrick Lamar");
        
        // Verify ordering by CreatedAt descending (newest first)
        Assert.True(wishlist[0].CreatedAt >= wishlist[1].CreatedAt, 
            "Wishlist should be ordered by creation date descending");
    }

    [Fact]
    public async Task Another_signed_in_user_cannot_delete_a_wishlist_item_they_do_not_own()
    {
        await _factory.SeedUserAsync("wishlist-owner", "Wishlist Owner");
        await _factory.SeedUserAsync("wishlist-other", "Wishlist Other");

        var ownerClient = CreateAuthenticatedClient("wishlist-owner");
        var createResponse = await ownerClient.PostAsJsonAsync("/api/me/wishlist", NewWishlistItem("Kendrick Lamar"));
        var created = await createResponse.Content.ReadFromJsonAsync<WishlistItemDto>();
        Assert.NotNull(created);

        var otherClient = CreateAuthenticatedClient("wishlist-other");
        var deleteResponse = await otherClient.DeleteAsync($"/api/me/wishlist/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var ownerListResponse = await ownerClient.GetAsync("/api/me/wishlist");
        var ownerWishlist = await ownerListResponse.Content.ReadFromJsonAsync<List<WishlistItemDto>>();
        Assert.NotNull(ownerWishlist);
        Assert.Contains(ownerWishlist, item => item.Id == created.Id);
    }

    [Fact]
    public async Task Wishlist_prevents_duplicate_artist_saves_for_the_same_user()
    {
        await _factory.SeedUserAsync("duplicate-user", "Duplicate User");
        var client = CreateAuthenticatedClient("duplicate-user");

        var firstResponse = await client.PostAsJsonAsync("/api/me/wishlist", NewWishlistItem("De La Soul"));
        var first = await firstResponse.Content.ReadFromJsonAsync<WishlistItemDto>();
        Assert.NotNull(first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Duplicate with different casing and extra spaces
        var duplicateResponse = await client.PostAsJsonAsync("/api/me/wishlist", NewWishlistItem("  de la soul  "));
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var conflictBody = await duplicateResponse.Content.ReadFromJsonAsync<ConflictResponseDto>();
        Assert.NotNull(conflictBody);
        Assert.NotNull(conflictBody.Item);
        Assert.Equal("De La Soul", conflictBody.Item.ArtistName);
        Assert.Equal(first.Id, conflictBody.Item.Id);
    }

    [Fact]
    public async Task LastFm_search_with_empty_artist_returns_400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/external/lastfm/search?artist=");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LastFm_search_uses_stubbed_client_for_successful_search()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/external/lastfm/search?artist=Sade");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var results = await response.Content.ReadFromJsonAsync<List<LastFmArtistDto>>();
        Assert.NotNull(results);
        var result = Assert.Single(results);
        Assert.Equal("Sade Test Result", result.Name);
        Assert.Equal(12345, result.Listeners);
    }

    [Fact]
    public async Task Wishlist_POST_with_invalid_source_url_returns_400()
    {
        await _factory.SeedUserAsync("validation-user", "Validation User");
        var client = CreateAuthenticatedClient("validation-user");

        var invalidUrlRequest = new WishlistItemRequestDto(
            "Test Artist",
            "not-a-valid-url",
            "Test source");

        var response = await client.PostAsJsonAsync("/api/me/wishlist", invalidUrlRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string oauthSubject)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, oauthSubject);
        return client;
    }

    private static WishlistItemRequestDto NewWishlistItem(string artistName)
    {
        return new WishlistItemRequestDto(
            artistName,
            "https://www.last.fm/music/example",
            "Test source");
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private sealed record WishlistItemRequestDto(
        string ArtistName,
        string? SourceUrl,
        string? SourceName);

    private sealed record WishlistItemDto(
        int Id,
        string ArtistName,
        string? SourceUrl,
        string? SourceName,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    private sealed record LastFmArtistDto(
        string Name,
        string? Url,
        int? Listeners,
        string? ImageUrl);

    private sealed record ConflictResponseDto(
        string Message,
        WishlistItemDto Item);
}
