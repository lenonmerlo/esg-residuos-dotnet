using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using EsgResiduos.Tests.Support;

using Microsoft.AspNetCore.Mvc.Testing;

using Reqnroll;

namespace EsgResiduos.Tests.Steps;

[Binding]
public sealed class GestaoResiduosSteps : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly List<HttpResponseMessage> _responses = [];
    private HttpClient _client;
    private string _email = string.Empty;
    private string _password = "Test@1234";
    private HttpResponseMessage? _lastResponse;
    private int _wasteTypeId;
    private int _collectionPointId;
    private int _collectionId;
    private int _destinationId;
    private int _limitAlertId;

    public GestaoResiduosSteps()
    {
        _client = _factory.CreateClient();
    }

    [Given("que sou um usuário autenticado")]
    public async Task DadoQueSouUmUsuarioAutenticado()
    {
        await RegisterAsync();
        string json = await ReadJsonAsync(_lastResponse!);
        string token = JsonDocument.Parse(json).RootElement.GetProperty("token").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [When("cadastro um novo usuário")]
    public Task QuandoCadastroUmNovoUsuario() => RegisterAsync();

    [Then("o cadastro deve retornar 201 e um token conforme o contrato")]
    public async Task EntaoCadastroValido()
    {
        Assert.Equal(HttpStatusCode.Created, _lastResponse!.StatusCode);
        string json = await ReadJsonAsync(_lastResponse);
        JsonSchemaContract.AssertValid(json, "auth-response.schema.json");
        Assert.Equal(_email, JsonDocument.Parse(json).RootElement.GetProperty("email").GetString());
    }

    [When("autentico o usuário cadastrado")]
    public async Task QuandoAutenticoUsuario()
    {
        _lastResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = _email,
            password = _password
        });
    }

    [Then("o login deve retornar 200 e um token conforme o contrato")]
    public async Task EntaoLoginValido()
    {
        Assert.Equal(HttpStatusCode.OK, _lastResponse!.StatusCode);
        JsonSchemaContract.AssertValid(await ReadJsonAsync(_lastResponse), "auth-response.schema.json");
    }

    [When("consulto todas as APIs protegidas sem token")]
    public async Task QuandoConsultoApisSemToken()
    {
        foreach (string resource in new[] { "wastetypes", "collectionpoints", "collections", "destinations", "collectionalerts" })
        {
            _responses.Add(await _client.GetAsync($"/api/{resource}"));
        }
    }

    [Then("todas as consultas devem retornar 401")]
    public void EntaoTodasRetornam401()
    {
        Assert.Equal(5, _responses.Count);
        Assert.All(_responses, response => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode));
    }

    [When("executo o ciclo completo de tipo de resíduo, ponto, coleta e destinação")]
    public async Task QuandoExecutoCicloCompleto()
    {
        _wasteTypeId = await CreateAndAssertAsync(
            "/api/wastetypes",
            new { wasteCategory = $"VIDRO-{Guid.NewGuid():N}"[..30], description = "Vidro reciclável" },
            "waste-type.schema.json");
        await AssertGetAsync("/api/wastetypes", HttpStatusCode.OK, "paged-waste-types.schema.json");
        await AssertGetAsync($"/api/wastetypes/{_wasteTypeId}", HttpStatusCode.OK, "waste-type.schema.json");
        await AssertPutAsync($"/api/wastetypes/{_wasteTypeId}",
            new { wasteCategory = $"VIDRO-{Guid.NewGuid():N}"[..30], description = "Vidro atualizado" },
            "waste-type.schema.json");

        _collectionPointId = await CreateAndAssertAsync(
            "/api/collectionpoints",
            new { name = "Ecoponto BDD", capacityKg = 100m, alertVolumeKg = 50m },
            "collection-point.schema.json");
        await AssertGetAsync("/api/collectionpoints", HttpStatusCode.OK, "paged-collection-points.schema.json");
        await AssertGetAsync($"/api/collectionpoints/{_collectionPointId}", HttpStatusCode.OK, "collection-point.schema.json");
        await AssertPutAsync($"/api/collectionpoints/{_collectionPointId}",
            new { name = "Ecoponto BDD atualizado", capacityKg = 100m, alertVolumeKg = 50m },
            "collection-point.schema.json");

        _collectionId = await CreateAndAssertAsync(
            "/api/collections",
            new { collectionPointId = _collectionPointId, wasteTypeId = _wasteTypeId, volumeKg = 60m },
            "collection.schema.json");
        await AssertGetAsync("/api/collections", HttpStatusCode.OK, "paged-collections.schema.json");
        await AssertGetAsync($"/api/collections/{_collectionId}", HttpStatusCode.OK, "collection.schema.json");

        JsonElement alerts = await AssertGetAsync("/api/collectionalerts", HttpStatusCode.OK, "paged-alerts.schema.json");
        JsonElement limitAlert = alerts.GetProperty("data").EnumerateArray()
            .First(item => item.GetProperty("alertType").GetString() == "LIMIT_REACHED");
        _limitAlertId = limitAlert.GetProperty("id").GetInt32();
        await AssertGetAsync($"/api/collectionalerts/{_limitAlertId}", HttpStatusCode.OK, "alert.schema.json");

        _destinationId = await CreateAndAssertAsync(
            "/api/destinations",
            new
            {
                collectionId = _collectionId,
                destinationName = "Cooperativa Verde",
                processingType = "Reciclagem",
                destinatedVolumeKg = 60m
            },
            "destination.schema.json");
        await AssertGetAsync("/api/destinations", HttpStatusCode.OK, "paged-destinations.schema.json");
        await AssertGetAsync($"/api/destinations/{_destinationId}", HttpStatusCode.OK, "destination.schema.json");
    }

    [Then("todas as APIs devem responder com status e contratos JSON válidos")]
    public void EntaoTodasApisValidas()
    {
        Assert.True(_wasteTypeId > 0);
        Assert.True(_collectionPointId > 0);
        Assert.True(_collectionId > 0);
        Assert.True(_destinationId > 0);
        Assert.True(_limitAlertId > 0);
    }

    [Then("deve existir um alerta automático de limite atingido")]
    public async Task EntaoExisteAlertaDeLimite()
    {
        JsonElement alerts = await AssertGetAsync("/api/collectionalerts", HttpStatusCode.OK, "paged-alerts.schema.json");
        Assert.Contains(alerts.GetProperty("data").EnumerateArray(),
            item => item.GetProperty("alertType").GetString() == "LIMIT_REACHED");
    }

    [Then("deve existir uma notificação de destinação correta")]
    public async Task EntaoExisteNotificacaoDeDestinacao()
    {
        JsonElement alerts = await AssertGetAsync("/api/collectionalerts", HttpStatusCode.OK, "paged-alerts.schema.json");
        JsonElement notification = alerts.GetProperty("data").EnumerateArray()
            .First(item => item.GetProperty("alertType").GetString() == "DESTINATION_NOTIFICATION");
        int id = notification.GetProperty("id").GetInt32();
        await AssertGetAsync($"/api/collectionalerts/{id}", HttpStatusCode.OK, "alert.schema.json");
    }

    [Then("os recursos devem ser excluídos na ordem segura")]
    public async Task EntaoExcluiRecursos()
    {
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/collections/{_collectionId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/collectionpoints/{_collectionPointId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/wastetypes/{_wasteTypeId}")).StatusCode);
    }

    [When("consulto {string} com página zero")]
    public async Task QuandoConsultoComPaginaZero(string recurso)
    {
        _lastResponse = await _client.GetAsync($"/api/{recurso}?page=0&pageSize=10");
    }

    [Then("a resposta deve ter status 400")]
    public void EntaoResposta400() => Assert.Equal(HttpStatusCode.BadRequest, _lastResponse!.StatusCode);

    [When("tento autenticar com credenciais inexistentes")]
    public async Task QuandoTentoCredenciaisInvalidas()
    {
        _lastResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = $"inexistente_{Guid.NewGuid():N}@test.com",
            password = "senha-incorreta"
        });
    }

    [Then("a resposta deve ter status 401 e contrato de erro válido")]
    public async Task EntaoResposta401ComContrato()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, _lastResponse!.StatusCode);
        JsonSchemaContract.AssertValid(await ReadJsonAsync(_lastResponse), "problem-details.schema.json");
    }

    private async Task RegisterAsync()
    {
        _email = $"bdd_{Guid.NewGuid():N}@test.com";
        _lastResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Usuário BDD",
            email = _email,
            password = _password
        });
    }

    private async Task<int> CreateAndAssertAsync(string uri, object body, string schema)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(uri, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string json = await ReadJsonAsync(response);
        JsonSchemaContract.AssertValid(json, schema);
        return JsonDocument.Parse(json).RootElement.GetProperty("id").GetInt32();
    }

    private async Task AssertPutAsync(string uri, object body, string schema)
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync(uri, body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonSchemaContract.AssertValid(await ReadJsonAsync(response), schema);
    }

    private async Task<JsonElement> AssertGetAsync(string uri, HttpStatusCode status, string schema)
    {
        HttpResponseMessage response = await _client.GetAsync(uri);
        Assert.Equal(status, response.StatusCode);
        string json = await ReadJsonAsync(response);
        JsonSchemaContract.AssertValid(json, schema);
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static async Task<string> ReadJsonAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        foreach (HttpResponseMessage response in _responses)
        {
            response.Dispose();
        }

        _lastResponse?.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }
}
