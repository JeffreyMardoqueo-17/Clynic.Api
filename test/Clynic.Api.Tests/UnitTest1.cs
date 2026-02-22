using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Clynic.Api.Tests;

public class HealthVersionEndpointTests : IClassFixture<ClynicApiFactory>
{
    private readonly HttpClient _client;

    public HealthVersionEndpointTests(ClynicApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetVersion_DebeResponder200YNombreApi()
    {
        var response = await _client.GetAsync("/api/health/version");
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("Clynic API", body);
    }
}

public class ClynicApiFactory : WebApplicationFactory<Program>
{
    public ClynicApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Server=(localdb)\\mssqllocaldb;Database=Clynic_Test;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=Clynic_Test;Trusted_Connection=True;TrustServerCertificate=True"
            };

            config.AddInMemoryCollection(testSettings);
        });
    }
}
