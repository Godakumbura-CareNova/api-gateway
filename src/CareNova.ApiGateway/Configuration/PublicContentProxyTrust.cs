using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
namespace CareNova.ApiGateway.Configuration;
public static class PublicContentProxyTrust
{
    public static void Configure(ForwardedHeadersOptions options, IConfiguration configuration)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
        options.ForwardLimit = 1;
        options.KnownProxies.Add(IPAddress.Loopback);
        foreach (var address in configuration.GetSection("PublicContent:TrustedProxyAddresses").Get<string[]>() ?? []) {
            if (!IPAddress.TryParse(address, out var proxy)) throw new InvalidOperationException("Trusted proxy addresses must be IP addresses.");
            options.KnownProxies.Add(proxy);
        }
        foreach (var value in configuration.GetSection("PublicContent:TrustedProxyNetworks").Get<string[]>() ?? []) {
            var parts = value.Split('/');
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var network) || !int.TryParse(parts[1], out var prefix) ||
                prefix < 1 || prefix > (network.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128))
                throw new InvalidOperationException("Trusted proxy networks must be specific CIDR networks; trust-all ranges are not allowed.");
            options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(network, prefix));
        }
    }
}
