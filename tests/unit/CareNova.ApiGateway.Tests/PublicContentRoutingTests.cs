using CareNova.ApiGateway.Configuration;
using Microsoft.Extensions.Configuration;
namespace CareNova.ApiGateway.Tests.Unit;
public class PublicContentRoutingTests
{
    [Fact]
    public void RoutesPublicReadsSubmissionsAdminEditingAndPhotosWithoutChangingExistingRoutes()
    {
        var config = new ConfigurationManager();
        config["Routes:0:UpstreamPathTemplate"] = "/existing";
        PublicContentRouting.Configure(config, true);
        var routes = config.GetSection("Routes").GetChildren().ToArray();
        Assert.Equal("/existing", routes[0]["UpstreamPathTemplate"]);
        Assert.Contains(routes, route => route["UpstreamPathTemplate"] == "/api/public/pharmacy-information" && route["UpstreamHttpMethod:0"] == "GET");
        Assert.Contains(routes, route => route["UpstreamPathTemplate"] == "/api/public/contact-inquiries" && route["UpstreamHttpMethod:0"] == "POST");
        Assert.Contains(routes, route => route["UpstreamPathTemplate"] == "/api/public/contact-inquiries" && route["UpstreamHeaderTransform:X-Forwarded-For"] == "{RemoteIpAddress}");
        Assert.Contains(routes, route => route["UpstreamPathTemplate"] == "/api/admin/pharmacy-information" && route["UpstreamHttpMethod:1"] == "PUT");
        Assert.Contains(routes, route => route["UpstreamPathTemplate"] == "/api/admin/contact-inquiries/{id}/reviewed" && route["UpstreamHttpMethod:0"] == "PATCH");
        Assert.Contains(routes, route => route["UpstreamPathTemplate"] == "/uploads/pharmacy-information/{everything}" && route["UpstreamHttpMethod:0"] == "GET");
    }
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
