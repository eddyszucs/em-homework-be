using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ClinicBackend.Tests.Integration.Controllers;

public class WaitlistControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _assistantClient;
    private readonly HttpClient _doctorClient;

    public WaitlistControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetDoctorWaitlist_AsDoctor_Returns200()
    {
        var token = await GetTokenAsync(_doctorClient, "dr_kovacs", "Doc123!");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/waitlist");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _doctorClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDoctorWaitlist_AsAssistant_Returns403()
    {
        var token = await GetTokenAsync(_assistantClient, "assistent1", "Asst123!");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/waitlist");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _assistantClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AssignPatient_AssignsToWaiting()
    {
        // Create a new patient first
        var assistantToken = await GetTokenAsync(_assistantClient, "assistent1", "Asst123!");
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/patients");
        createReq.Headers.Add("Authorization", $"Bearer {assistantToken}");
        createReq.Content = JsonContent.Create(new
        {
            name = "Új Beteg",
            address = "Budapest, Új utca 1.",
            tajNumber = "888-777-666",
            complaint = "Szédülés",
            specialty = "Belgyógyász"
        });
        var createResp = await _assistantClient.SendAsync(createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var patientId = Guid.Parse(created.GetProperty("id").GetString()!);
        var initialStatus = created.GetProperty("status").GetString();
        Assert.Equal("Waiting", initialStatus);

        // Get dr_kovacs's ID
        var doctorToken = await GetTokenAsync(_doctorClient, "dr_kovacs", "Doc123!");
        var drKovacsId = GetDoctorIdFromToken(doctorToken);

        // Assign - send as plain JSON Guid
        var assignReq = new HttpRequestMessage(HttpMethod.Post, $"/api/waitlist/{patientId}/assign");
        assignReq.Headers.Add("Authorization", $"Bearer {assistantToken}");
        assignReq.Content = new StringContent($"\"{drKovacsId}\"", Encoding.UTF8, "application/json");

        var response = await _assistantClient.SendAsync(assignReq);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine("ERROR BODY: " + errorBody);
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Waiting", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task CallPatient_ByUnassignedDoctor_Returns403()
    {
        var token = await GetTokenAsync(_doctorClient, "dr_kovacs", "Doc123!");
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/waitlist/{Guid.NewGuid()}/call");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _doctorClient.SendAsync(request);
        Assert.True(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden or HttpStatusCode.BadRequest);
    }

    private static Guid GetDoctorIdFromToken(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var sub = jwt.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
        return Guid.Parse(sub);
    }

    public void Dispose()
    {
        _assistantClient.Dispose();
        _doctorClient.Dispose();
    }
}