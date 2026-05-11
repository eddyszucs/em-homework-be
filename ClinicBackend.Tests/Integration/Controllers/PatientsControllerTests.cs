using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicBackend.Tests.Integration.Controllers;

public class PatientsControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _assistantClient;
    private readonly HttpClient _doctorClient;

    public PatientsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _assistantClient = _factory.CreateClient();
        _doctorClient = _factory.CreateClient();
    }

    private async Task<string> GetTokenAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task GetAll_AsAssistant_Returns200WithPatients()
    {
        var token = await GetTokenAsync(_assistantClient, "assistent1", "Asst123!");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/patients");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _assistantClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var patients = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(patients.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetAll_AsDoctor_Returns403()
    {
        var token = await GetTokenAsync(_doctorClient, "dr_kovacs", "Doc123!");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/patients");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _doctorClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePatient_AsAssistant_Returns201()
    {
        var token = await GetTokenAsync(_assistantClient, "assistent1", "Asst123!");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/patients");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(new
        {
            name = "Test Beteg",
            address = "Budapest, Teszt utca 1.",
            tajNumber = "999-888-777",
            complaint = "Fejfájás",
            specialty = "Belgyógyász"
        });

        var response = await _assistantClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Test Beteg", body.GetProperty("name").GetString());
        Assert.Equal("Waiting", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task CreatePatient_AsDoctor_Returns403()
    {
        var token = await GetTokenAsync(_doctorClient, "dr_kovacs", "Doc123!");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/patients");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(new
        {
            name = "Test Beteg",
            address = "Budapest",
            tajNumber = "999-888-777",
            complaint = "Fejfájás",
            specialty = "Belgyógyász"
        });

        var response = await _doctorClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePatient_WithInvalidTaj_Returns400()
    {
        var token = await GetTokenAsync(_assistantClient, "assistent1", "Asst123!");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/patients");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(new
        {
            name = "Test Beteg",
            address = "Budapest",
            tajNumber = "invalid",
            complaint = "Fejfájás",
            specialty = "Belgyógyász"
        });

        var response = await _assistantClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreatePatient_WithInvalidName_Returns400()
    {
        var token = await GetTokenAsync(_assistantClient, "assistent1", "Asst123!");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/patients");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = JsonContent.Create(new
        {
            name = "Invalid",
            address = "Budapest",
            tajNumber = "999-888-777",
            complaint = "Fejfájás",
            specialty = "Belgyógyász"
        });

        var response = await _assistantClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _assistantClient.Dispose();
        _doctorClient.Dispose();
    }
}