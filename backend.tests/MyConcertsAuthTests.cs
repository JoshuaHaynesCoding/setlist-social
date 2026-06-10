using System.Net;
using System.Net.Http.Json;

namespace SetlistSocial.Api.Tests;

public sealed class MyConcertsAuthTests : IDisposable
{
    private readonly SetlistSocialApiFactory _factory = new();

    [Theory]
    [InlineData("/api/me")]
    [InlineData("/api/me/dashboard")]
    [InlineData("/api/me/concerts")]
    public async Task Protected_current_user_endpoints_return_401_when_unauthenticated(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Signed_in_user_can_create_and_read_their_own_concert()
    {
        await _factory.SeedUserAsync("user-one", "User One");
        var client = CreateAuthenticatedClient("user-one");

        var createResponse = await client.PostAsJsonAsync("/api/me/concerts", NewConcert("First Show"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ConcertDto>();
        Assert.NotNull(created);
        Assert.Equal("First Show", created.Title);
        Assert.Equal("The Test Artist", created.ArtistName);

        var readResponse = await client.GetAsync($"/api/me/concerts/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        var read = await readResponse.Content.ReadFromJsonAsync<ConcertDto>();
        Assert.NotNull(read);
        Assert.Equal(created.Id, read.Id);
        Assert.Equal("First Show", read.Title);
    }

    [Fact]
    public async Task Another_signed_in_user_cannot_read_update_or_delete_a_concert_they_do_not_own()
    {
        await _factory.SeedUserAsync("owner-user", "Owner User");
        await _factory.SeedUserAsync("other-user", "Other User");

        var ownerClient = CreateAuthenticatedClient("owner-user");
        var createResponse = await ownerClient.PostAsJsonAsync("/api/me/concerts", NewConcert("Private Show"));
        var created = await createResponse.Content.ReadFromJsonAsync<ConcertDto>();
        Assert.NotNull(created);

        var otherClient = CreateAuthenticatedClient("other-user");

        var readResponse = await otherClient.GetAsync($"/api/me/concerts/{created.Id}");
        var updateResponse = await otherClient.PutAsJsonAsync(
            $"/api/me/concerts/{created.Id}",
            NewConcert("Updated By Other User"));
        var deleteResponse = await otherClient.DeleteAsync($"/api/me/concerts/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, readResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var ownerReadResponse = await ownerClient.GetAsync($"/api/me/concerts/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, ownerReadResponse.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string oauthSubject)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, oauthSubject);
        return client;
    }

    private static ConcertRequestDto NewConcert(string title)
    {
        return new ConcertRequestDto(
            title,
            "The Test Artist",
            "Test Venue",
            "Chicago",
            "IL",
            "USA",
            new DateTimeOffset(2026, 6, 10, 12, 0, 0, TimeSpan.Zero));
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private sealed record ConcertRequestDto(
        string Title,
        string ArtistName,
        string? VenueName,
        string? City,
        string? Region,
        string? Country,
        DateTimeOffset ConcertDate);

    private sealed record ConcertDto(
        int Id,
        string Title,
        string ArtistName,
        string? VenueName,
        string? City,
        string? Region,
        string? Country,
        DateTimeOffset ConcertDate,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);
}
