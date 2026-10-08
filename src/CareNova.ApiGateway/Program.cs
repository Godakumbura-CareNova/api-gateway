using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);
CareNova.ApiGateway.Configuration.PublicContentRouting.Configure(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://carenova-frontend-avh6beggfqapfjfb.centralindia-01.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOcelot(builder.Configuration);
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
    CareNova.ApiGateway.Configuration.PublicContentProxyTrust.Configure(options, builder.Configuration));

var app = builder.Build();
app.UseForwardedHeaders();

// The inquiry route forwards the connection's client address, never a caller-supplied header.
app.Use(async (context, next) => {
    if (context.Request.Path.Equals("/api/public/contact-inquiries")) context.Request.Headers.Remove("X-Forwarded-For");
    await next();
});

app.UseCors("FrontendPolicy");

app.UseHttpMetrics();

app.UseMetricServer();

await app.UseOcelot();

app.Run();
