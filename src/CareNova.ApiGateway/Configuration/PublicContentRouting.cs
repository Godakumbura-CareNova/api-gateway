namespace CareNova.ApiGateway.Configuration;

public static class PublicContentRouting
{
    // The deployment address is configuration, never a guessed Azure hostname.
    public static void Configure(ConfigurationManager configuration, bool isDevelopment)
    {
        var address = configuration["PublicContent:BaseUrl"];
        if (string.IsNullOrWhiteSpace(address))
        {
            if (!isDevelopment) return;
            address = "http://localhost:5136";
        }
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https") || uri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidOperationException("PublicContent:BaseUrl must be the HTTP or HTTPS origin of the appointment-integration service.");

        var prefix = $"Routes:{configuration.GetSection("Routes").GetChildren().Count()}:";
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [prefix + "DownstreamPathTemplate"] = "/api/public/home",
            [prefix + "DownstreamScheme"] = uri.Scheme,
            [prefix + "DownstreamHostAndPorts:0:Host"] = uri.Host,
            [prefix + "DownstreamHostAndPorts:0:Port"] = uri.Port.ToString(),
            [prefix + "UpstreamPathTemplate"] = "/api/public/home",
            [prefix + "UpstreamHttpMethod:0"] = "GET",
            [prefix + "UpstreamHttpMethod:1"] = "OPTIONS"
        });
    }
}
