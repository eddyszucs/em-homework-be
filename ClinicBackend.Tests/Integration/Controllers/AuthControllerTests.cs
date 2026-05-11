using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicBackend.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidAssistantCredentials_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "assistent1",
            password = "Asst123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("token", out var token) && !string.IsNullOrEmpty(token.GetString()));
        Assert.Equal("assistent1", body.GetProperty("user").GetProperty("username").GetString());
        Assert.Equal("Assistant", body.GetProperty("user").GetProperty("role").GetString());
    }

    [Fact]
    public async Task Login_WithValidDoctorCredentials_Returns200WithDoctorRole()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "dr_kovacs",
            password = "Doc123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Doctor", body.GetProperty("user").GetProperty("role").GetString());
        Assert.Equal("Belgyógyász", body.GetProperty("user").GetProperty("specialty").GetString());
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "assistent1",
            password = "WrongPassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("INVALID_CREDENTIALS", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Login_WithNonexistentUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "nonexistent",
            password = "AnyPassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}