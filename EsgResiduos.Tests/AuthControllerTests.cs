using Microsoft.AspNetCore.Mvc.Testing;

using System.Net;
using System.Text;
using System.Text.Json;

namespace EsgResiduos.Tests;

public class AuthControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_ReturnsHttpStatusCode200()
    {
        string email = $"auth_{Guid.NewGuid()}@test.com";
        object registerPayload = new { name = "Usuário Teste", email, password = "Test@1234" };
        StringContent registerContent = new(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/auth/register", registerContent);

        object loginPayload = new { email, password = "Test@1234" };
        StringContent loginContent = new(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync("/api/auth/login", loginContent);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
