using System.Net;
using CareNova.ApiGateway.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
namespace CareNova.ApiGateway.Tests.Unit;
public class PublicContentProxyTrustTests
{
    [Theory]
    [InlineData("10.0.0.1", "198.51.100.1")]
    [InlineData("203.0.113.1", "203.0.113.1")]
    public async Task GatewayUsesOriginalClientOnlyWhenEdgeProxyIsTrusted(string source, string expected)
    {
        var configuration = new ConfigurationManager();
        configuration["PublicContent:TrustedProxyAddresses:0"] = "10.0.0.1";
        var options = new ForwardedHeadersOptions();
        PublicContentProxyTrust.Configure(options, configuration);
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(source);
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.1";
        var middleware = new ForwardedHeadersMiddleware(_ => Task.CompletedTask, NullLoggerFactory.Instance, Options.Create(options));
        await middleware.Invoke(context);
        Assert.Equal(expected, context.Connection.RemoteIpAddress!.ToString());
    }
    [Fact]
    public void NeverAcceptsTrustAllNetworkConfiguration()
    {
        var configuration = new ConfigurationManager();
        configuration["PublicContent:TrustedProxyNetworks:0"] = "0.0.0.0/0";
        Assert.Throws<InvalidOperationException>(() => PublicContentProxyTrust.Configure(new(), configuration));
    }
}
