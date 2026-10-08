using CareNova.ApiGateway.Configuration;
using Microsoft.Extensions.Configuration;
namespace CareNova.ApiGateway.Tests.Unit;
public class PublicContentRoutingTests
{
    [Fact]
    public void ProductionUsesConfiguredIntegrationHostAndReadOnlyMethods()
    {
        var config = new ConfigurationManager();
        config["PublicContent:BaseUrl"] = "https://integration.example.test:8443";
        PublicContentRouting.Configure(config, false);
        Assert.Equal("integration.example.test", config["Routes:0:DownstreamHostAndPorts:0:Host"]);
        Assert.Equal("8443", config["Routes:0:DownstreamHostAndPorts:0:Port"]);
        Assert.Equal("GET", config["Routes:0:UpstreamHttpMethod:0"]);
        Assert.Null(config["Routes:0:UpstreamHttpMethod:2"]);
    }
    [Fact]
    public void ProductionNeverFallsBackToLocalhost()
    {
        var config = new ConfigurationManager();
        PublicContentRouting.Configure(config, false);
        Assert.Empty(config.GetSection("Routes").GetChildren());
    }
    [Fact]
    public void DevelopmentAddsLocalPublicHomeRoute()
    {
        var config = new ConfigurationManager();
        PublicContentRouting.Configure(config, true);
        Assert.Equal("localhost", config["Routes:0:DownstreamHostAndPorts:0:Host"]);
        Assert.Equal("5136", config["Routes:0:DownstreamHostAndPorts:0:Port"]);
    }
}
