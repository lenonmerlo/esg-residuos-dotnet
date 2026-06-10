using System.Text;
using System.Text.Json;

namespace EsgResiduos.Tests;

internal static class AuthTestHelper
{
    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client)
    {
        string email = $"test_{Guid.NewGuid()}@test.com";
        object payload = new { name = "Usuário Teste", email, password = "Test@1234" };
        StringContent content = new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/auth/register", content);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString()!;
    }
}
