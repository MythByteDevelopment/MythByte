using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MythByte.Shared.Responses;
using MythByte.Shared.SystemInfo;

namespace MythByte.IntegrationTests;

public class SystemControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetVersion_Anonymous_ReturnsVersionWithoutCommitSuffix()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/system/version");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<VersionResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(body.Value?.Version));
        Assert.DoesNotContain("+", body.Value!.Version);
    }
}
