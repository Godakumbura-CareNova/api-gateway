using System.Text.Json;

namespace CareNova.ApiGateway.Tests.Unit;

public class ApiGatewayRoutingTests
{
    [Fact]
    public void UserStatisticsRouteForwardsToAuthServiceWithoutAllowingMutations()
    {
        using var configuration = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ocelot.json")));
        var routes = configuration.RootElement.GetProperty("Routes").EnumerateArray().ToArray();
        var dashboard = Assert.Single(routes, route => route.GetProperty("UpstreamPathTemplate").GetString() == "/admin/dashboard/users");
        var users = Assert.Single(routes, route => route.GetProperty("UpstreamPathTemplate").GetString() == "/admin/users");

        Assert.Equal("/api/admin/dashboard/users", dashboard.GetProperty("DownstreamPathTemplate").GetString());
        Assert.Equal("https", dashboard.GetProperty("DownstreamScheme").GetString());
        Assert.Equal(users.GetProperty("DownstreamHostAndPorts").GetRawText(), dashboard.GetProperty("DownstreamHostAndPorts").GetRawText());
        Assert.Equal(443, Assert.Single(dashboard.GetProperty("DownstreamHostAndPorts").EnumerateArray()).GetProperty("Port").GetInt32());
        Assert.Equal(new[] { "GET", "OPTIONS" }, dashboard.GetProperty("UpstreamHttpMethod").EnumerateArray().Select(method => method.GetString()).ToArray());
    }
}
