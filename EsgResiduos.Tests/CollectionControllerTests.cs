using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EsgResiduos.Tests;

public class CollectionControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsHttpStatusCode200()
    {
        // Arrange
        string token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/collections");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<string> GetTokenAsync()
    {
        string email = $"test_{Guid.NewGuid()}@test.com";
        object payload = new { name = "Test User", email, password = "Test@1234" };
        StringContent content = new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/auth/register", content);
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString()!;
    }
}