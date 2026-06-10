using Microsoft.AspNetCore.Mvc.Testing;

using System.Net;
using System.Net.Http.Headers;

namespace EsgResiduos.Tests;

public class WasteTypeControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsHttpStatusCode200()
    {
        string token = await AuthTestHelper.RegisterAndGetTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _client.GetAsync("/api/wastetypes");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
