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

        (string Path, string[] Methods)[] routes = [
            ("/api/public/home", ["GET", "OPTIONS"]),
            ("/api/public/pharmacy-information", ["GET", "OPTIONS"]),
            ("/api/public/contact-inquiries", ["POST", "OPTIONS"]),
            ("/api/admin/pharmacy-information", ["GET", "PUT", "OPTIONS"]),
            ("/api/admin/pharmacy-information/images", ["POST", "OPTIONS"]),
            ("/api/admin/contact-inquiries", ["GET", "OPTIONS"]),
            ("/api/admin/contact-inquiries/{id}/reviewed", ["PATCH", "OPTIONS"]),
            ("/uploads/pharmacy-information/{everything}", ["GET", "OPTIONS"]),
        ];
        var index = configuration.GetSection("Routes").GetChildren().Count();
        var additions = new Dictionary<string, string?>();
        foreach (var route in routes) {
            var prefix = $"Routes:{index++}:";
            additions[prefix + "DownstreamPathTemplate"] = route.Path;
            additions[prefix + "UpstreamPathTemplate"] = route.Path;
            additions[prefix + "DownstreamScheme"] = uri.Scheme;
            additions[prefix + "DownstreamHostAndPorts:0:Host"] = uri.Host;
            additions[prefix + "DownstreamHostAndPorts:0:Port"] = uri.Port.ToString();
            additions[prefix + "Priority"] = "10";
            if (route.Path == "/api/public/contact-inquiries") additions[prefix + "UpstreamHeaderTransform:X-Forwarded-For"] = "{RemoteIpAddress}";
            for (var method = 0; method < route.Methods.Length; method++) additions[prefix + $"UpstreamHttpMethod:{method}"] = route.Methods[method];
        }
        configuration.AddInMemoryCollection(additions);
    }
}
